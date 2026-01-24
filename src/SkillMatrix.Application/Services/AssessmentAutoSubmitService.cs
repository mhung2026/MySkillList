using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkillMatrix.Application.Interfaces;
using SkillMatrix.Domain.Enums;
using SkillMatrix.Infrastructure.Persistence;

namespace SkillMatrix.Application.Services;

/// <summary>
/// Background service that automatically submits assessments when time expires
/// </summary>
public class AssessmentAutoSubmitService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AssessmentAutoSubmitService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

    public AssessmentAutoSubmitService(
        IServiceProvider serviceProvider,
        ILogger<AssessmentAutoSubmitService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Assessment Auto-Submit Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSubmitExpiredAssessments(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for expired assessments");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Assessment Auto-Submit Service stopped");
    }

    private async Task CheckAndSubmitExpiredAssessments(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SkillMatrixDbContext>();
        var assessmentService = scope.ServiceProvider.GetRequiredService<IAssessmentService>();

        // Find all in-progress assessments that have expired
        var expiredAssessments = await context.Assessments
            .Include(a => a.TestTemplate)
            .Where(a => a.Status == AssessmentStatus.InProgress
                && a.TestTemplate != null
                && a.TestTemplate.TimeLimitMinutes != null
                && a.StartedAt != null)
            .ToListAsync(stoppingToken);

        var now = DateTime.UtcNow;
        var submittedCount = 0;

        foreach (var assessment in expiredAssessments)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            var deadline = assessment.StartedAt!.Value.AddMinutes(assessment.TestTemplate!.TimeLimitMinutes!.Value);

            if (now > deadline)
            {
                try
                {
                    _logger.LogInformation(
                        "Auto-submitting expired assessment {AssessmentId} for employee {EmployeeId}. " +
                        "Started: {StartedAt}, Deadline: {Deadline}",
                        assessment.Id, assessment.EmployeeId, assessment.StartedAt, deadline);

                    await assessmentService.SubmitAssessmentAsync(assessment.Id);
                    submittedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to auto-submit assessment {AssessmentId}",
                        assessment.Id);
                }
            }
        }

        if (submittedCount > 0)
        {
            _logger.LogInformation("Auto-submitted {Count} expired assessment(s)", submittedCount);
        }
    }
}

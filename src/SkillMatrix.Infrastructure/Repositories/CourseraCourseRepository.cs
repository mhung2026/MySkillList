using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace SkillMatrix.Infrastructure.Repositories;

/// <summary>
/// Repository for accessing Coursera courses from SFIA database
/// </summary>
public class CourseraCourseRepository : ICourseraCourseRepository
{
    private readonly string _connectionString;

    public CourseraCourseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<List<CourseraCourseModel>> GetCoursesBySkillCodeAsync(string skillCode)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var sql = @"
            SELECT c.*
            FROM ""CourseraCourse"" c
            INNER JOIN ""SFIASkillCoursera"" s ON c.""SkillId"" = s.""SkillId""
            WHERE s.""SkillCode"" = @SkillCode
            ORDER BY c.""Rating"" DESC NULLS LAST, c.""ReviewsCount"" DESC
            LIMIT 10";

        var courses = await connection.QueryAsync<CourseraCourseModel>(sql, new { SkillCode = skillCode });
        return courses.ToList();
    }

    public async Task<List<CourseraCourseModel>> SearchCoursesBySkillNameAsync(string skillName)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        var sql = @"
            SELECT c.*
            FROM ""CourseraCourse"" c
            INNER JOIN ""SFIASkillCoursera"" s ON c.""SkillId"" = s.""SkillId""
            WHERE s.""SkillName"" ILIKE @SkillName
            ORDER BY c.""Rating"" DESC NULLS LAST, c.""ReviewsCount"" DESC
            LIMIT 10";

        var courses = await connection.QueryAsync<CourseraCourseModel>(
            sql,
            new { SkillName = $"%{skillName}%" });

        return courses.ToList();
    }
}

/// <summary>
/// Repository interface for Coursera courses
/// </summary>
public interface ICourseraCourseRepository
{
    Task<List<CourseraCourseModel>> GetCoursesBySkillCodeAsync(string skillCode);
    Task<List<CourseraCourseModel>> SearchCoursesBySkillNameAsync(string skillName);
}

/// <summary>
/// Model mapping to Coursera course table from SFIA database
/// </summary>
public class CourseraCourseModel
{
    public int Id { get; set; }
    public Guid SkillId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string[]? Instructor { get; set; }
    public string? Organization { get; set; }
    public string? Description { get; set; }
    public decimal? Rating { get; set; }
    public int? ReviewsCount { get; set; }
    public int? EnrollmentCount { get; set; }
    public string? Duration { get; set; }
    public string? Level { get; set; }
    public string? Language { get; set; }
    public string[]? Skills { get; set; }
    public string? Price { get; set; }
    public bool? CertificateAvailable { get; set; }
}

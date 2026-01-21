using SkillMatrix.Domain.Entities.Organization;

namespace SkillMatrix.Infrastructure.Persistence.SeedData;

public static class TeamSeed
{
    public static List<Team> GetTeams()
    {
        return new List<Team>
        {
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                Name = "Team SoEzy",
                Description = "Team phát triển sản phẩm SoEzy - 17 thành viên",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                Name = "Team Mezy",
                Description = "Team phát triển sản phẩm Mezy - 4 thành viên",
                IsActive = true
            }
        };
    }
}

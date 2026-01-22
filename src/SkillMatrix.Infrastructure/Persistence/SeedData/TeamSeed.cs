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
                Description = "SoEzy product development team - 17 members",
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                Name = "Team Mezy",
                Description = "Mezy product development team - 4 members",
                IsActive = true
            }
        };
    }
}

using SkillMatrix.Domain.Entities.Taxonomy;
using SkillMatrix.Domain.Enums;

namespace SkillMatrix.Infrastructure.Persistence.SeedData;

/// <summary>
/// SFIA 9 Skill Taxonomy Seed Data
/// Based on SFIA 9 Summary Chart - The global skills and competency framework for the digital world
/// </summary>
public static class SkillTaxonomySeed
{
    public static List<SkillDomain> GetSkillDomains()
    {
        return new List<SkillDomain>
        {
            // SFIA 9 - 6 Main Categories
            new()
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Code = "STAC",
                Name = "Strategy and architecture",
                Description = "Skills for defining business strategy and the technical architecture needed to support it",
                DisplayOrder = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Code = "CHTR",
                Name = "Change and transformation",
                Description = "Skills for implementing and managing change and transformation initiatives",
                DisplayOrder = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Code = "DVIM",
                Name = "Development and implementation",
                Description = "Skills for designing, developing, testing and implementing systems and solutions",
                DisplayOrder = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Code = "DLOP",
                Name = "Delivery and operation",
                Description = "Skills for delivering and operating digital services and infrastructure",
                DisplayOrder = 4,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Code = "PEOS",
                Name = "People and skills",
                Description = "Skills for developing people capabilities and managing performance",
                DisplayOrder = 5,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Code = "RLMT",
                Name = "Relationships and engagement",
                Description = "Skills for managing stakeholder relationships and business engagement",
                DisplayOrder = 6,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            }
        };
    }

    public static List<SkillSubcategory> GetSkillSubcategories()
    {
        return new List<SkillSubcategory>
        {
            // ============================================
            // STRATEGY AND ARCHITECTURE
            // ============================================
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Code = "STPL",
                Name = "Strategy and planning",
                Description = "Strategic planning and business technology strategy",
                DisplayOrder = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Code = "FVMG",
                Name = "Financial and value management",
                Description = "Financial management and value optimization",
                DisplayOrder = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Code = "SEPR",
                Name = "Security and privacy",
                Description = "Information security and privacy management",
                DisplayOrder = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Code = "GORC",
                Name = "Governance, risk and compliance",
                Description = "IT governance, risk management and compliance",
                DisplayOrder = 4,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000005"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Code = "ADVG",
                Name = "Advice and guidance",
                Description = "Consultancy and specialist advice",
                DisplayOrder = 5,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // ============================================
            // CHANGE AND TRANSFORMATION
            // ============================================
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000006"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Code = "CHIM",
                Name = "Change implementation",
                Description = "Portfolio, programme and project management",
                DisplayOrder = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000007"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Code = "CHAN",
                Name = "Change analysis",
                Description = "Business analysis and requirements definition",
                DisplayOrder = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000008"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Code = "CHPL",
                Name = "Change planning",
                Description = "Business process improvement and organizational change",
                DisplayOrder = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // ============================================
            // DEVELOPMENT AND IMPLEMENTATION
            // ============================================
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000009"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Code = "SYDE",
                Name = "Systems development",
                Description = "Software development and engineering",
                DisplayOrder = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000010"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Code = "DAAN",
                Name = "Data and analytics",
                Description = "Data management, analytics and AI",
                DisplayOrder = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000011"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Code = "UCDE",
                Name = "User centred design",
                Description = "User experience and interface design",
                DisplayOrder = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000012"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Code = "CTMG",
                Name = "Content management",
                Description = "Content design, publishing and management",
                DisplayOrder = 4,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000013"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Code = "CMSC",
                Name = "Computational science",
                Description = "Scientific modelling and high-performance computing",
                DisplayOrder = 5,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // ============================================
            // DELIVERY AND OPERATION
            // ============================================
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000014"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Code = "TCMG",
                Name = "Technology management",
                Description = "Technology service management and support",
                DisplayOrder = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000015"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Code = "SVMG",
                Name = "Service management",
                Description = "Service catalogue, availability and capacity management",
                DisplayOrder = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000016"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Code = "SSVC",
                Name = "Security services",
                Description = "Security operations and vulnerability assessment",
                DisplayOrder = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000017"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Code = "DAOP",
                Name = "Data and records operations",
                Description = "Data management and database administration",
                DisplayOrder = 4,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // ============================================
            // PEOPLE AND SKILLS
            // ============================================
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000018"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Code = "POMG",
                Name = "People management",
                Description = "Performance, employee experience and workforce planning",
                DisplayOrder = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000019"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Code = "SKMG",
                Name = "Skills management",
                Description = "Learning, development and competency management",
                DisplayOrder = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },

            // ============================================
            // RELATIONSHIPS AND ENGAGEMENT
            // ============================================
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000020"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Code = "STMG",
                Name = "Stakeholder management",
                Description = "Sourcing, supplier and contract management",
                DisplayOrder = 1,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000021"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Code = "SABM",
                Name = "Sales and bid management",
                Description = "Sales, selling and bid management",
                DisplayOrder = 2,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000022"),
                SkillDomainId = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Code = "MKTG",
                Name = "Marketing",
                Description = "Marketing, brand and customer engagement",
                DisplayOrder = 3,
                IsActive = true,
                IsCurrent = true,
                EffectiveFrom = DateTime.UtcNow
            }
        };
    }

    public static List<Skill> GetSkills()
    {
        return new List<Skill>
        {
            // ============================================
            // STRATEGY AND ARCHITECTURE - Strategy and planning
            // ============================================
            CreateSkill("30000000-0000-0000-0000-000000000001", "20000000-0000-0000-0000-000000000001",
                "ITSP", "Strategic planning", "Defining IT strategy aligned with business goals", SkillCategory.Domain, SkillType.Specialty, 1, "4,5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000002", "20000000-0000-0000-0000-000000000001",
                "ISCO", "Information systems coordination", "Coordinating information systems strategy", SkillCategory.Domain, SkillType.Specialty, 2, "6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000003", "20000000-0000-0000-0000-000000000001",
                "IRMG", "Information management", "Managing information as a strategic resource", SkillCategory.Domain, SkillType.Specialty, 3, "4,5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000004", "20000000-0000-0000-0000-000000000001",
                "STPL", "Enterprise and business architecture", "Designing enterprise architecture", SkillCategory.Domain, SkillType.Specialty, 4, "5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000005", "20000000-0000-0000-0000-000000000001",
                "ARCH", "Solution architecture", "Designing solution architectures", SkillCategory.Technical, SkillType.Specialty, 5, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000006", "20000000-0000-0000-0000-000000000001",
                "INOV", "Innovation management", "Managing innovation initiatives", SkillCategory.Domain, SkillType.Specialty, 6, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000007", "20000000-0000-0000-0000-000000000001",
                "EMRG", "Emerging technology monitoring", "Monitoring and evaluating emerging technologies", SkillCategory.Technical, SkillType.Adjacent, 7, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000008", "20000000-0000-0000-0000-000000000001",
                "RSCH", "Formal research", "Conducting formal research", SkillCategory.Domain, SkillType.Specialty, 8, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000009", "20000000-0000-0000-0000-000000000001",
                "SUST", "Sustainability", "Managing sustainability initiatives", SkillCategory.Domain, SkillType.Specialty, 9, "3,4,5,6,7"),

            // Financial and value management
            CreateSkill("30000000-0000-0000-0000-000000000010", "20000000-0000-0000-0000-000000000002",
                "FMIT", "Financial management", "Managing IT financial resources", SkillCategory.Domain, SkillType.Specialty, 1, "4,5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000011", "20000000-0000-0000-0000-000000000002",
                "INVA", "Investment appraisal", "Evaluating IT investments", SkillCategory.Domain, SkillType.Specialty, 2, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000012", "20000000-0000-0000-0000-000000000002",
                "BENM", "Benefits management", "Managing and realizing benefits", SkillCategory.Domain, SkillType.Specialty, 3, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000013", "20000000-0000-0000-0000-000000000002",
                "BUDF", "Budgeting and forecasting", "Budget planning and forecasting", SkillCategory.Domain, SkillType.Specialty, 4, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000014", "20000000-0000-0000-0000-000000000002",
                "FNAN", "Financial analysis", "Financial analysis and reporting", SkillCategory.Domain, SkillType.Specialty, 5, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000015", "20000000-0000-0000-0000-000000000002",
                "COAG", "Cost management", "Managing and controlling costs", SkillCategory.Domain, SkillType.Specialty, 6, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000016", "20000000-0000-0000-0000-000000000002",
                "DEMM", "Demand management", "Managing demand for services", SkillCategory.Domain, SkillType.Specialty, 7, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000017", "20000000-0000-0000-0000-000000000002",
                "MEAS", "Measurement", "Defining and using metrics", SkillCategory.Domain, SkillType.Core, 8, "2,3,4,5,6"),

            // Security and privacy
            CreateSkill("30000000-0000-0000-0000-000000000018", "20000000-0000-0000-0000-000000000003",
                "SCTY", "Information security", "Managing information security", SkillCategory.Technical, SkillType.Specialty, 1, "3,4,5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000019", "20000000-0000-0000-0000-000000000003",
                "INAS", "Information assurance", "Ensuring information assurance", SkillCategory.Technical, SkillType.Specialty, 2, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000020", "20000000-0000-0000-0000-000000000003",
                "PEDP", "Information and data compliance", "Managing data compliance", SkillCategory.Domain, SkillType.Specialty, 3, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000021", "20000000-0000-0000-0000-000000000003",
                "VURE", "Vulnerability research", "Researching vulnerabilities", SkillCategory.Technical, SkillType.Specialty, 4, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000022", "20000000-0000-0000-0000-000000000003",
                "THIN", "Threat intelligence", "Managing threat intelligence", SkillCategory.Technical, SkillType.Specialty, 5, "3,4,5,6"),

            // Governance, risk and compliance
            CreateSkill("30000000-0000-0000-0000-000000000023", "20000000-0000-0000-0000-000000000004",
                "GOVN", "Governance", "IT governance and management", SkillCategory.Domain, SkillType.Specialty, 1, "4,5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000024", "20000000-0000-0000-0000-000000000004",
                "BURM", "Risk management", "Managing IT and business risks", SkillCategory.Domain, SkillType.Specialty, 2, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000025", "20000000-0000-0000-0000-000000000004",
                "ATIC", "Artificial intelligence (AI) and data ethics", "Managing AI and data ethics", SkillCategory.Domain, SkillType.Specialty, 3, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000026", "20000000-0000-0000-0000-000000000004",
                "AUDT", "Audit", "Conducting IT audits", SkillCategory.Domain, SkillType.Specialty, 4, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000027", "20000000-0000-0000-0000-000000000004",
                "QUMG", "Quality management", "Managing quality processes", SkillCategory.Domain, SkillType.Specialty, 5, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000028", "20000000-0000-0000-0000-000000000004",
                "QUAS", "Quality assurance", "Ensuring quality standards", SkillCategory.Domain, SkillType.Core, 6, "2,3,4,5,6"),

            // Advice and guidance
            CreateSkill("30000000-0000-0000-0000-000000000029", "20000000-0000-0000-0000-000000000005",
                "CNSL", "Consultancy", "Providing consultancy services", SkillCategory.Domain, SkillType.Specialty, 1, "4,5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000030", "20000000-0000-0000-0000-000000000005",
                "TECH", "Specialist advice", "Providing technical advice", SkillCategory.Technical, SkillType.Specialty, 2, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000031", "20000000-0000-0000-0000-000000000005",
                "METL", "Methods and tools", "Defining methods and tools", SkillCategory.Technical, SkillType.Specialty, 3, "2,3,4,5,6"),

            // ============================================
            // CHANGE AND TRANSFORMATION - Change implementation
            // ============================================
            CreateSkill("30000000-0000-0000-0000-000000000032", "20000000-0000-0000-0000-000000000006",
                "PFMG", "Portfolio management", "Managing project portfolios", SkillCategory.Domain, SkillType.Specialty, 1, "5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000033", "20000000-0000-0000-0000-000000000006",
                "PGMG", "Programme management", "Managing programmes", SkillCategory.Domain, SkillType.Specialty, 2, "5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000034", "20000000-0000-0000-0000-000000000006",
                "PRMG", "Project management", "Managing projects", SkillCategory.Domain, SkillType.Core, 3, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000035", "20000000-0000-0000-0000-000000000006",
                "PROF", "Portfolio, programme and project support", "Supporting PPM activities", SkillCategory.Domain, SkillType.Specialty, 4, "2,3,4"),
            CreateSkill("30000000-0000-0000-0000-000000000036", "20000000-0000-0000-0000-000000000006",
                "DEMG", "Delivery management", "Managing delivery of solutions", SkillCategory.Domain, SkillType.Specialty, 5, "4,5,6"),

            // Change analysis
            CreateSkill("30000000-0000-0000-0000-000000000037", "20000000-0000-0000-0000-000000000007",
                "BUSA", "Business situation analysis", "Analyzing business situations", SkillCategory.Domain, SkillType.Specialty, 1, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000038", "20000000-0000-0000-0000-000000000007",
                "FEAS", "Feasibility assessment", "Assessing feasibility", SkillCategory.Domain, SkillType.Specialty, 2, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000039", "20000000-0000-0000-0000-000000000007",
                "REQM", "Requirements definition and management", "Managing requirements", SkillCategory.Domain, SkillType.Core, 3, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000040", "20000000-0000-0000-0000-000000000007",
                "BSMO", "Business modelling", "Creating business models", SkillCategory.Domain, SkillType.Specialty, 4, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000041", "20000000-0000-0000-0000-000000000007",
                "BPTS", "User acceptance testing", "Conducting acceptance testing", SkillCategory.Domain, SkillType.Core, 5, "2,3,4,5"),

            // Change planning
            CreateSkill("30000000-0000-0000-0000-000000000042", "20000000-0000-0000-0000-000000000008",
                "BPRE", "Business process improvement", "Improving business processes", SkillCategory.Domain, SkillType.Specialty, 1, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000043", "20000000-0000-0000-0000-000000000008",
                "OCDY", "Organizational capability development", "Developing organizational capabilities", SkillCategory.Domain, SkillType.Specialty, 2, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000044", "20000000-0000-0000-0000-000000000008",
                "JAAN", "Job analysis and design", "Analyzing and designing jobs", SkillCategory.Domain, SkillType.Specialty, 3, "3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000045", "20000000-0000-0000-0000-000000000008",
                "ORDI", "Organisation design and implementation", "Designing organizational structures", SkillCategory.Domain, SkillType.Specialty, 4, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000046", "20000000-0000-0000-0000-000000000008",
                "CIPM", "Organisational change management", "Managing organizational change", SkillCategory.Domain, SkillType.Specialty, 5, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000047", "20000000-0000-0000-0000-000000000008",
                "OCEN", "Organisational change enablement", "Enabling organizational change", SkillCategory.Domain, SkillType.Specialty, 6, "3,4,5,6"),

            // ============================================
            // DEVELOPMENT AND IMPLEMENTATION - Systems development
            // ============================================
            CreateSkill("30000000-0000-0000-0000-000000000048", "20000000-0000-0000-0000-000000000009",
                "PROD", "Product management", "Managing product development", SkillCategory.Domain, SkillType.Specialty, 1, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000049", "20000000-0000-0000-0000-000000000009",
                "DLMG", "Systems development management", "Managing systems development", SkillCategory.Domain, SkillType.Specialty, 2, "4,5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000050", "20000000-0000-0000-0000-000000000009",
                "SLEN", "Systems and software lifecycle engineering", "Engineering software lifecycle", SkillCategory.Technical, SkillType.Specialty, 3, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000051", "20000000-0000-0000-0000-000000000009",
                "DESN", "Systems design", "Designing systems", SkillCategory.Technical, SkillType.Specialty, 4, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000052", "20000000-0000-0000-0000-000000000009",
                "SWDN", "Software design", "Designing software solutions", SkillCategory.Technical, SkillType.Specialty, 5, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000053", "20000000-0000-0000-0000-000000000009",
                "NTDS", "Network design", "Designing networks", SkillCategory.Technical, SkillType.Specialty, 6, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000054", "20000000-0000-0000-0000-000000000009",
                "IFDN", "Infrastructure design", "Designing infrastructure", SkillCategory.Technical, SkillType.Specialty, 7, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000055", "20000000-0000-0000-0000-000000000009",
                "HWDE", "Hardware design", "Designing hardware", SkillCategory.Technical, SkillType.Specialty, 8, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000056", "20000000-0000-0000-0000-000000000009",
                "PROG", "Programming/software development", "Developing software", SkillCategory.Technical, SkillType.Core, 9, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000057", "20000000-0000-0000-0000-000000000009",
                "SINT", "Systems integration and build", "Integrating systems", SkillCategory.Technical, SkillType.Specialty, 10, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000058", "20000000-0000-0000-0000-000000000009",
                "TEST", "Testing", "Testing software and systems", SkillCategory.Technical, SkillType.Core, 11, "1,2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000059", "20000000-0000-0000-0000-000000000009",
                "NFTS", "Non-functional testing", "Non-functional testing", SkillCategory.Technical, SkillType.Specialty, 12, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000060", "20000000-0000-0000-0000-000000000009",
                "PRTS", "Process testing", "Testing processes", SkillCategory.Technical, SkillType.Specialty, 13, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000061", "20000000-0000-0000-0000-000000000009",
                "PORT", "Software configuration", "Configuring software", SkillCategory.Technical, SkillType.Specialty, 14, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000062", "20000000-0000-0000-0000-000000000009",
                "RESD", "Real-time/embedded systems development", "Developing embedded systems", SkillCategory.Technical, SkillType.Specialty, 15, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000063", "20000000-0000-0000-0000-000000000009",
                "SFEN", "Safety engineering", "Engineering safety-critical systems", SkillCategory.Technical, SkillType.Specialty, 16, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000064", "20000000-0000-0000-0000-000000000009",
                "SFAS", "Safety assessment", "Assessing safety", SkillCategory.Technical, SkillType.Specialty, 17, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000065", "20000000-0000-0000-0000-000000000009",
                "RFEN", "Radio frequency engineering", "Engineering RF systems", SkillCategory.Technical, SkillType.Specialty, 18, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000066", "20000000-0000-0000-0000-000000000009",
                "ADEV", "Animation development", "Developing animations", SkillCategory.Technical, SkillType.Specialty, 19, "2,3,4,5,6"),

            // Data and analytics
            CreateSkill("30000000-0000-0000-0000-000000000067", "20000000-0000-0000-0000-000000000010",
                "DATM", "Data management", "Managing data assets", SkillCategory.Technical, SkillType.Specialty, 1, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000068", "20000000-0000-0000-0000-000000000010",
                "DTAN", "Data modelling and design", "Modelling and designing data", SkillCategory.Technical, SkillType.Specialty, 2, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000069", "20000000-0000-0000-0000-000000000010",
                "DBDS", "Database design", "Designing databases", SkillCategory.Technical, SkillType.Specialty, 3, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000070", "20000000-0000-0000-0000-000000000010",
                "DATS", "Data analytics", "Analyzing data", SkillCategory.Technical, SkillType.Specialty, 4, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000071", "20000000-0000-0000-0000-000000000010",
                "DASC", "Data science", "Applying data science", SkillCategory.Technical, SkillType.Specialty, 5, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000072", "20000000-0000-0000-0000-000000000010",
                "MLNG", "Machine learning", "Developing machine learning solutions", SkillCategory.Technical, SkillType.Specialty, 6, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000073", "20000000-0000-0000-0000-000000000010",
                "BINT", "Business intelligence", "Delivering business intelligence", SkillCategory.Technical, SkillType.Specialty, 7, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000074", "20000000-0000-0000-0000-000000000010",
                "DENG", "Data engineering", "Engineering data solutions", SkillCategory.Technical, SkillType.Specialty, 8, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000075", "20000000-0000-0000-0000-000000000010",
                "VISL", "Data visualisation", "Visualizing data", SkillCategory.Technical, SkillType.Specialty, 9, "1,2,3,4,5"),

            // User centred design
            CreateSkill("30000000-0000-0000-0000-000000000076", "20000000-0000-0000-0000-000000000011",
                "URCH", "User research", "Conducting user research", SkillCategory.Domain, SkillType.Specialty, 1, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000077", "20000000-0000-0000-0000-000000000011",
                "CEXP", "Customer experience", "Managing customer experience", SkillCategory.Domain, SkillType.Specialty, 2, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000078", "20000000-0000-0000-0000-000000000011",
                "ACIN", "Accessibility and inclusion", "Ensuring accessibility", SkillCategory.Domain, SkillType.Core, 3, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000079", "20000000-0000-0000-0000-000000000011",
                "UNAN", "User experience analysis", "Analyzing user experience", SkillCategory.Domain, SkillType.Specialty, 4, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000080", "20000000-0000-0000-0000-000000000011",
                "HCEV", "User experience design", "Designing user experiences", SkillCategory.Technical, SkillType.Specialty, 5, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000081", "20000000-0000-0000-0000-000000000011",
                "USEV", "User experience evaluation", "Evaluating user experiences", SkillCategory.Domain, SkillType.Specialty, 6, "2,3,4,5"),

            // Content management
            CreateSkill("30000000-0000-0000-0000-000000000082", "20000000-0000-0000-0000-000000000012",
                "INCA", "Content design and authoring", "Designing and authoring content", SkillCategory.Domain, SkillType.Specialty, 1, "1,2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000083", "20000000-0000-0000-0000-000000000012",
                "COPU", "Content publishing", "Publishing content", SkillCategory.Domain, SkillType.Specialty, 2, "1,2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000084", "20000000-0000-0000-0000-000000000012",
                "KNOW", "Knowledge management", "Managing knowledge", SkillCategory.Domain, SkillType.Specialty, 3, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000085", "20000000-0000-0000-0000-000000000012",
                "GRDN", "Graphic design", "Creating graphic designs", SkillCategory.Technical, SkillType.Specialty, 4, "1,2,3,4,5,6"),

            // Computational science
            CreateSkill("30000000-0000-0000-0000-000000000086", "20000000-0000-0000-0000-000000000013",
                "SCMO", "Scientific modelling", "Creating scientific models", SkillCategory.Technical, SkillType.Specialty, 1, "3,4,5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000087", "20000000-0000-0000-0000-000000000013",
                "NUAN", "Numerical analysis", "Performing numerical analysis", SkillCategory.Technical, SkillType.Specialty, 2, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000088", "20000000-0000-0000-0000-000000000013",
                "HPCC", "High-performance computing", "Developing HPC solutions", SkillCategory.Technical, SkillType.Specialty, 3, "3,4,5,6"),

            // ============================================
            // DELIVERY AND OPERATION - Technology management
            // ============================================
            CreateSkill("30000000-0000-0000-0000-000000000089", "20000000-0000-0000-0000-000000000014",
                "ITMG", "Technology service management", "Managing technology services", SkillCategory.Domain, SkillType.Specialty, 1, "4,5,6,7"),
            CreateSkill("30000000-0000-0000-0000-000000000090", "20000000-0000-0000-0000-000000000014",
                "ASUP", "Application support", "Supporting applications", SkillCategory.Technical, SkillType.Specialty, 2, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000091", "20000000-0000-0000-0000-000000000014",
                "ITOP", "Infrastructure operations", "Operating infrastructure", SkillCategory.Technical, SkillType.Specialty, 3, "1,2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000092", "20000000-0000-0000-0000-000000000014",
                "SYSP", "System software administration", "Administering system software", SkillCategory.Technical, SkillType.Specialty, 4, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000093", "20000000-0000-0000-0000-000000000014",
                "NTAS", "Network support", "Supporting networks", SkillCategory.Technical, SkillType.Specialty, 5, "1,2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000094", "20000000-0000-0000-0000-000000000014",
                "HSIN", "Systems installation and removal", "Installing systems", SkillCategory.Technical, SkillType.Specialty, 6, "1,2,3,4"),
            CreateSkill("30000000-0000-0000-0000-000000000095", "20000000-0000-0000-0000-000000000014",
                "CFMG", "Configuration management", "Managing configurations", SkillCategory.Technical, SkillType.Specialty, 7, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000096", "20000000-0000-0000-0000-000000000014",
                "RELM", "Release management", "Managing releases", SkillCategory.Technical, SkillType.Specialty, 8, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000097", "20000000-0000-0000-0000-000000000014",
                "DPLS", "Deployment", "Deploying solutions", SkillCategory.Technical, SkillType.Core, 9, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000098", "20000000-0000-0000-0000-000000000014",
                "SRMG", "Storage management", "Managing storage", SkillCategory.Technical, SkillType.Specialty, 10, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000099", "20000000-0000-0000-0000-000000000014",
                "DCMA", "Facilities management", "Managing facilities", SkillCategory.Domain, SkillType.Specialty, 11, "2,3,4,5,6"),

            // Service management
            CreateSkill("30000000-0000-0000-0000-000000000100", "20000000-0000-0000-0000-000000000015",
                "SLMO", "Service level management", "Managing service levels", SkillCategory.Domain, SkillType.Specialty, 1, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000101", "20000000-0000-0000-0000-000000000015",
                "SCMG", "Service catalogue management", "Managing service catalogues", SkillCategory.Domain, SkillType.Specialty, 2, "3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000102", "20000000-0000-0000-0000-000000000015",
                "AVMT", "Availability management", "Managing availability", SkillCategory.Domain, SkillType.Specialty, 3, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000103", "20000000-0000-0000-0000-000000000015",
                "CPMG", "Capacity management", "Managing capacity", SkillCategory.Domain, SkillType.Specialty, 4, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000104", "20000000-0000-0000-0000-000000000015",
                "ICPM", "Incident management", "Managing incidents", SkillCategory.Domain, SkillType.Core, 5, "2,3,4"),
            CreateSkill("30000000-0000-0000-0000-000000000105", "20000000-0000-0000-0000-000000000015",
                "PBMG", "Problem management", "Managing problems", SkillCategory.Domain, SkillType.Specialty, 6, "3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000106", "20000000-0000-0000-0000-000000000015",
                "CHMG", "Change control", "Controlling changes", SkillCategory.Domain, SkillType.Specialty, 7, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000107", "20000000-0000-0000-0000-000000000015",
                "APTS", "Asset management", "Managing assets", SkillCategory.Domain, SkillType.Specialty, 8, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000108", "20000000-0000-0000-0000-000000000015",
                "SEAC", "Service acceptance", "Accepting services", SkillCategory.Domain, SkillType.Specialty, 9, "3,4,5"),

            // Security services
            CreateSkill("30000000-0000-0000-0000-000000000109", "20000000-0000-0000-0000-000000000016",
                "SCAD", "Security operations", "Operating security services", SkillCategory.Technical, SkillType.Specialty, 1, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000110", "20000000-0000-0000-0000-000000000016",
                "IAMT", "Identity and access management", "Managing identity and access", SkillCategory.Technical, SkillType.Specialty, 2, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000111", "20000000-0000-0000-0000-000000000016",
                "VUAS", "Vulnerability assessment", "Assessing vulnerabilities", SkillCategory.Technical, SkillType.Specialty, 3, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000112", "20000000-0000-0000-0000-000000000016",
                "DGFS", "Digital forensics", "Conducting digital forensics", SkillCategory.Technical, SkillType.Specialty, 4, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000113", "20000000-0000-0000-0000-000000000016",
                "CRIM", "Cybercrime investigation", "Investigating cybercrime", SkillCategory.Technical, SkillType.Specialty, 5, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000114", "20000000-0000-0000-0000-000000000016",
                "OCOP", "Offensive cyber operations", "Conducting offensive operations", SkillCategory.Technical, SkillType.Specialty, 6, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000115", "20000000-0000-0000-0000-000000000016",
                "PENT", "Penetration testing", "Conducting penetration testing", SkillCategory.Technical, SkillType.Specialty, 7, "2,3,4,5,6"),

            // Data and records operations
            CreateSkill("30000000-0000-0000-0000-000000000116", "20000000-0000-0000-0000-000000000017",
                "RMGT", "Records management", "Managing records", SkillCategory.Domain, SkillType.Specialty, 1, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000117", "20000000-0000-0000-0000-000000000017",
                "ANCC", "Analytical classification and coding", "Classifying and coding data", SkillCategory.Technical, SkillType.Specialty, 2, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000118", "20000000-0000-0000-0000-000000000017",
                "DBAD", "Database administration", "Administering databases", SkillCategory.Technical, SkillType.Specialty, 3, "2,3,4,5"),

            // ============================================
            // PEOPLE AND SKILLS - People management
            // ============================================
            CreateSkill("30000000-0000-0000-0000-000000000119", "20000000-0000-0000-0000-000000000018",
                "PEMT", "Performance management", "Managing performance", SkillCategory.Leadership, SkillType.Core, 1, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000120", "20000000-0000-0000-0000-000000000018",
                "EEXP", "Employee experience", "Managing employee experience", SkillCategory.Leadership, SkillType.Specialty, 2, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000121", "20000000-0000-0000-0000-000000000018",
                "OFCL", "Organisational facilitation", "Facilitating organizational activities", SkillCategory.Leadership, SkillType.Specialty, 3, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000122", "20000000-0000-0000-0000-000000000018",
                "PDSV", "Professional development", "Developing professionals", SkillCategory.Leadership, SkillType.Specialty, 4, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000123", "20000000-0000-0000-0000-000000000018",
                "WFPL", "Workforce planning", "Planning workforce", SkillCategory.Leadership, SkillType.Specialty, 5, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000124", "20000000-0000-0000-0000-000000000018",
                "RESC", "Resourcing", "Managing resourcing", SkillCategory.Leadership, SkillType.Specialty, 6, "3,4,5"),

            // Skills management
            CreateSkill("30000000-0000-0000-0000-000000000125", "20000000-0000-0000-0000-000000000019",
                "ETMG", "Learning and development management", "Managing L&D", SkillCategory.Leadership, SkillType.Specialty, 1, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000126", "20000000-0000-0000-0000-000000000019",
                "TMCH", "Learning design and development", "Designing learning", SkillCategory.Domain, SkillType.Specialty, 2, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000127", "20000000-0000-0000-0000-000000000019",
                "ETDL", "Learning delivery", "Delivering learning", SkillCategory.Domain, SkillType.Core, 3, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000128", "20000000-0000-0000-0000-000000000019",
                "LEDA", "Competency assessment", "Assessing competency", SkillCategory.Domain, SkillType.Specialty, 4, "3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000129", "20000000-0000-0000-0000-000000000019",
                "CSOP", "Certification scheme operation", "Operating certification schemes", SkillCategory.Domain, SkillType.Specialty, 5, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000130", "20000000-0000-0000-0000-000000000019",
                "TEAC", "Teaching", "Teaching and instruction", SkillCategory.Domain, SkillType.Specialty, 6, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000131", "20000000-0000-0000-0000-000000000019",
                "SUBF", "Subject formation", "Forming subject matter", SkillCategory.Domain, SkillType.Specialty, 7, "4,5,6,7"),

            // ============================================
            // RELATIONSHIPS AND ENGAGEMENT - Stakeholder management
            // ============================================
            CreateSkill("30000000-0000-0000-0000-000000000132", "20000000-0000-0000-0000-000000000020",
                "SORC", "Sourcing", "Managing sourcing", SkillCategory.Domain, SkillType.Specialty, 1, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000133", "20000000-0000-0000-0000-000000000020",
                "SUPP", "Supplier management", "Managing suppliers", SkillCategory.Domain, SkillType.Specialty, 2, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000134", "20000000-0000-0000-0000-000000000020",
                "ITCM", "Contract management", "Managing contracts", SkillCategory.Domain, SkillType.Specialty, 3, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000135", "20000000-0000-0000-0000-000000000020",
                "RLMT", "Stakeholder relationship management", "Managing relationships", SkillCategory.Domain, SkillType.Core, 4, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000136", "20000000-0000-0000-0000-000000000020",
                "CSMG", "Customer service support", "Supporting customer service", SkillCategory.Domain, SkillType.Core, 5, "1,2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000137", "20000000-0000-0000-0000-000000000020",
                "ADMN", "Business administration", "Administering business", SkillCategory.Domain, SkillType.Core, 6, "1,2,3,4"),

            // Sales and bid management
            CreateSkill("30000000-0000-0000-0000-000000000138", "20000000-0000-0000-0000-000000000021",
                "BIDM", "Bid/proposal management", "Managing bids", SkillCategory.Domain, SkillType.Specialty, 1, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000139", "20000000-0000-0000-0000-000000000021",
                "SALE", "Selling", "Selling products/services", SkillCategory.Domain, SkillType.Specialty, 2, "2,3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000140", "20000000-0000-0000-0000-000000000021",
                "SSUP", "Sales support", "Supporting sales", SkillCategory.Domain, SkillType.Specialty, 3, "1,2,3,4"),

            // Marketing
            CreateSkill("30000000-0000-0000-0000-000000000141", "20000000-0000-0000-0000-000000000022",
                "MKMG", "Marketing management", "Managing marketing", SkillCategory.Domain, SkillType.Specialty, 1, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000142", "20000000-0000-0000-0000-000000000022",
                "MRCH", "Market research", "Researching markets", SkillCategory.Domain, SkillType.Specialty, 2, "3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000143", "20000000-0000-0000-0000-000000000022",
                "BRMG", "Brand management", "Managing brands", SkillCategory.Domain, SkillType.Specialty, 3, "4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000144", "20000000-0000-0000-0000-000000000022",
                "CTLO", "Customer engagement and loyalty", "Managing customer engagement", SkillCategory.Domain, SkillType.Specialty, 4, "3,4,5,6"),
            CreateSkill("30000000-0000-0000-0000-000000000145", "20000000-0000-0000-0000-000000000022",
                "MECM", "Marketing campaign management", "Managing campaigns", SkillCategory.Domain, SkillType.Specialty, 5, "2,3,4,5"),
            CreateSkill("30000000-0000-0000-0000-000000000146", "20000000-0000-0000-0000-000000000022",
                "DIGM", "Digital marketing", "Managing digital marketing", SkillCategory.Domain, SkillType.Specialty, 6, "2,3,4,5")
        };
    }

    private static Skill CreateSkill(string id, string subcategoryId, string code, string name, string description,
        SkillCategory category, SkillType skillType, int order, string levels = "1,2,3,4,5,6,7")
    {
        return new Skill
        {
            Id = Guid.Parse(id),
            SubcategoryId = Guid.Parse(subcategoryId),
            Code = code,
            Name = name,
            Description = description,
            Category = category,
            SkillType = skillType,
            DisplayOrder = order,
            IsActive = true,
            IsCurrent = true,
            IsCompanySpecific = false,
            EffectiveFrom = DateTime.UtcNow,
            // Applicable proficiency levels for this skill
            ApplicableLevels = levels
        };
    }
}

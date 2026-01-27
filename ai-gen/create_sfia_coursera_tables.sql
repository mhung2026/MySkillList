-- Create SFIA Skills with Coursera Courses tables
-- This schema stores scraped SFIA skills and their associated Coursera courses

-- Table for storing SFIA skills
CREATE TABLE IF NOT EXISTS public."SFIASkillCoursera" (
    "Id" SERIAL PRIMARY KEY,
    "SkillId" UUID NOT NULL,
    "SkillName" VARCHAR(255) NOT NULL,
    "SkillCode" VARCHAR(50) NOT NULL,
    "LevelCount" INTEGER,
    "CoursesFound" INTEGER,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_skill_id UNIQUE ("SkillId")
);

-- Table for storing Coursera courses associated with SFIA skills
CREATE TABLE IF NOT EXISTS public."CourseraCourse" (
    "Id" SERIAL PRIMARY KEY,
    "SkillId" UUID NOT NULL,
    "Url" TEXT NOT NULL,
    "Title" VARCHAR(500),
    "Instructor" TEXT[], -- Array of instructors
    "Organization" VARCHAR(255),
    "Description" TEXT,
    "Rating" DECIMAL(3,2),
    "ReviewsCount" INTEGER,
    "EnrollmentCount" INTEGER,
    "Duration" VARCHAR(100),
    "Level" VARCHAR(50),
    "Language" VARCHAR(100),
    "Subtitles" TEXT[], -- Array of subtitle languages
    "Skills" TEXT[], -- Array of skills/tags
    "Syllabus" TEXT[], -- Array of syllabus items
    "Prerequisites" TEXT[], -- Array of prerequisites
    "Price" VARCHAR(50),
    "CertificateAvailable" BOOLEAN,
    "ScrapedAt" TIMESTAMP,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_skill FOREIGN KEY ("SkillId") REFERENCES public."SFIASkillCoursera"("SkillId") ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_sfia_skill_code ON public."SFIASkillCoursera" ("SkillCode");
CREATE INDEX IF NOT EXISTS idx_sfia_skill_id ON public."SFIASkillCoursera" ("SkillId");
CREATE INDEX IF NOT EXISTS idx_coursera_skill_id ON public."CourseraCourse" ("SkillId");
CREATE INDEX IF NOT EXISTS idx_coursera_url ON public."CourseraCourse" ("Url");

-- Add comments for documentation
COMMENT ON TABLE public."SFIASkillCoursera" IS 'Stores SFIA skills with Coursera course metadata';
COMMENT ON TABLE public."CourseraCourse" IS 'Stores Coursera courses associated with SFIA skills';
COMMENT ON COLUMN public."SFIASkillCoursera"."SkillId" IS 'UUID identifier for the SFIA skill';
COMMENT ON COLUMN public."SFIASkillCoursera"."SkillCode" IS 'SFIA skill code (e.g., ACIN)';
COMMENT ON COLUMN public."SFIASkillCoursera"."CoursesFound" IS 'Number of Coursera courses found for this skill';
COMMENT ON COLUMN public."CourseraCourse"."ScrapedAt" IS 'Timestamp when the course data was scraped';

-- Migration: Add LearningRecommendations table
-- Date: 2026-01-27
-- Description: AI-generated learning recommendations for skill gaps

CREATE TABLE IF NOT EXISTS "LearningRecommendations" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "SkillGapId" UUID NOT NULL,
    "SkillId" UUID NOT NULL,
    "SkillName" VARCHAR(255) NOT NULL,
    "RecommendationType" VARCHAR(50) NOT NULL,
    "Title" VARCHAR(500) NOT NULL,
    "Description" TEXT,
    "Url" TEXT,
    "EstimatedHours" INT,
    "Rationale" TEXT NOT NULL,
    "DisplayOrder" INT NOT NULL DEFAULT 0,
    "IsCompleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CompletedAt" TIMESTAMP,
    "AiProvider" VARCHAR(100),
    "GeneratedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,

    CONSTRAINT "FK_LearningRecommendations_SkillGaps"
        FOREIGN KEY ("SkillGapId")
        REFERENCES "SkillGaps"("Id")
        ON DELETE CASCADE
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS "IX_LearningRecommendations_SkillGapId"
    ON "LearningRecommendations"("SkillGapId");

CREATE INDEX IF NOT EXISTS "IX_LearningRecommendations_SkillId"
    ON "LearningRecommendations"("SkillId");

CREATE INDEX IF NOT EXISTS "IX_LearningRecommendations_IsDeleted"
    ON "LearningRecommendations"("IsDeleted");

CREATE INDEX IF NOT EXISTS "IX_LearningRecommendations_IsCompleted"
    ON "LearningRecommendations"("IsCompleted");

-- Add comment
COMMENT ON TABLE "LearningRecommendations" IS 'AI-generated learning recommendations for skill gaps including Coursera courses';

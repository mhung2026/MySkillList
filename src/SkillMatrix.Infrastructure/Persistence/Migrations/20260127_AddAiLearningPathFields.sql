-- Add new fields to LearningPathItem for AI-generated learning paths
-- Migration: AddAiLearningPathFields
-- Date: 2026-01-27

ALTER TABLE "LearningPathItems"
ADD COLUMN IF NOT EXISTS "TargetLevelAfter" INT NULL,
ADD COLUMN IF NOT EXISTS "SuccessCriteria" TEXT NULL,
ADD COLUMN IF NOT EXISTS "ExternalUrl" TEXT NULL;

-- Add comments
COMMENT ON COLUMN "LearningPathItems"."TargetLevelAfter" IS 'Expected proficiency level after completing this item';
COMMENT ON COLUMN "LearningPathItems"."SuccessCriteria" IS 'How to measure success for this learning item';
COMMENT ON COLUMN "LearningPathItems"."ExternalUrl" IS 'Link to external resource (e.g., Coursera course)';

-- Add constraint for TargetLevelAfter
ALTER TABLE "LearningPathItems"
ADD CONSTRAINT "CK_LearningPathItems_TargetLevelAfter"
CHECK ("TargetLevelAfter" IS NULL OR ("TargetLevelAfter" >= 0 AND "TargetLevelAfter" <= 7));

-- Add ExternalUrl column to LearningPathItems table
-- This column stores links to external resources like Coursera courses

ALTER TABLE "LearningPathItems"
ADD COLUMN IF NOT EXISTS "ExternalUrl" TEXT NULL;

-- Add index for better query performance
CREATE INDEX IF NOT EXISTS "IX_LearningPathItems_ExternalUrl"
ON "LearningPathItems" ("ExternalUrl")
WHERE "ExternalUrl" IS NOT NULL;

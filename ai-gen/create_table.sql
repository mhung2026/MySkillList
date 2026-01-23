-- Create ConfigDocTemplate table
CREATE TABLE IF NOT EXISTS public."ConfigDocTemplate" (
    "Id" SERIAL PRIMARY KEY,
    "Code" VARCHAR(100) NOT NULL UNIQUE,
    "Name" VARCHAR(255),
    "KeyWordsTable" TEXT,
    "IsDisable" BOOLEAN DEFAULT FALSE,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "CreatedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create index for better performance
CREATE INDEX IF NOT EXISTS idx_configdoctemplate_code ON public."ConfigDocTemplate" ("Code");
CREATE INDEX IF NOT EXISTS idx_configdoctemplate_active ON public."ConfigDocTemplate" ("IsDisable", "IsDeleted");

-- Insert some sample data
INSERT INTO public."ConfigDocTemplate" ("Code", "Name", "KeyWordsTable", "IsDisable", "IsDeleted")
VALUES
    ('SAMPLE_TEMPLATE', 'Sample Document Template', '{"keywords": ["sample", "template", "test"]}', FALSE, FALSE),
    ('DEFAULT_DOC', 'Default Document', '{"keywords": ["default", "document"]}', FALSE, FALSE)
ON CONFLICT ("Code") DO NOTHING;
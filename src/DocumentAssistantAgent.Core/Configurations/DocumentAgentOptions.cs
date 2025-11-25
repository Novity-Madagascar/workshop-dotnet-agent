namespace DocumentAssistantAgent.Core.Configurations
{
    public class DocumentAgentOptions
    {
        #region Fields
        public const string SectionName = "DocumentAgent";
        #endregion

        #region Properties
        public string DefaultScanPath { get; set; } = string.Empty;

        public string IndexFilePath { get; set; } = "document_index.json";

        public int ExpirationCheckDays { get; set; } = 30;

        public string[] SupportedExtensions { get; set; } = [ ".txt", ".pdf" ];

        public bool EnableBackgroundScanning { get; set; } = true;

        public TimeSpan BackgroundScanInterval { get; set; } = TimeSpan.FromHours(6);

        public string ClassificationPrompt { get; set; } = """
            Analyze the following document and classify it:

            Filename: {0}
            Content preview: {1}

            Please classify this document into one of these categories:
            - Identity (passport, driver's license, birth certificate, etc.)
            - Professional (resume, contracts, work certificates, etc.)
            - Kids (children's documents, school reports, etc.)
            - School (academic certificates, transcripts, etc.)
            - Other (miscellaneous documents)

            Also determine the document type and extract any expiration date if present.

            Respond in JSON format:
            {{
                "Owner": "Owner's name if identifiable or 'Unknown'",
                "FileName": "the filename value",
                "FullPath": "the full path to the document",
                "Category": "Identity|Professional|Kids|Medical|School",
                "Type": "Passport|DriverLicense|BirthCertificate|Resume|Contract|Certificate|Report|Visa|Permit|Photo|Other",
                "ExpirationDate": "YYYY-MM-DD or null",
                "Keywords": ["keyword1", "keyword2"],
                "Summary": "brief description"
            }}
            """;

        public string RequirementPrompt { get; set; } = """
            Based on the user's request, determine the required documents for the following purpose:
            Purpose: {0}
            Provide a list of required documents in JSON format:
            [
                {{
                    "SearchTerm": "the document keyword value (e.g:permit, passport, photo)",
                    "Description": "Brief description of the document (in 3 words maxuimum)",
                    "Category": "Identity|Professional|Kids|School|Medical|Other",
                    "Type": "Passport|DriverLicense|BirthCertificate|Resume|Contract|Certificate|Report|Visa|Permit|Photo|Other"
                }},
                ...
            ]
            """;

        public string SystemPrompt { get; set; } = """
            You are a document management assistant. You can help users:
            1. Scan and classify documents in folders
            2. Find documents needed for specific purposes (like passport renewal, job applications)
            3. Check for expiring documents

            Available functions:
            - scan_folder: Scan a folder and classify documents
            - get_documents_for_purpose: Get documents needed for a purpose and create a zip file
            - check_expiring_documents: Check for documents expiring soon

            When users ask for document help, use the appropriate function and provide helpful responses.
            """;
        #endregion
    }
}

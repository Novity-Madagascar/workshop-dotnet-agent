namespace DocumentAssistantAgent.Core.Models
{
    /// <summary>
    /// Represents metadata and classification information for a document.
    /// </summary>
    public class DocumentInfo
    {
        #region Properties
        public string Owner { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string FullPath { get; set; } = string.Empty;

        public DocumentCategory Category { get; set; }

        public DocumentType Type { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public DateTime LastModified { get; set; }

        public string? Summary { get; set; }

        public List<string> Keywords { get; set; } = new();
        #endregion
    }
}

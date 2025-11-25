namespace DocumentAssistantAgent.Core.Models
{
    /// <summary>
    /// Represents classification information for a document, including its category, type, expiration date, keywords, and summary.
    /// </summary>
    public class DocumentClassification
    {
        #region Properties
        public string Owner { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public DateTime? ExpirationDate { get; set; }

        public List<string> Keywords { get; set; } = new();

        public string Summary { get; set; } = string.Empty;
        #endregion
    }
}

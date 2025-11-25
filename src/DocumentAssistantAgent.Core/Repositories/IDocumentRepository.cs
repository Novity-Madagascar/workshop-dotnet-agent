using DocumentAssistantAgent.Core.Models;

namespace DocumentAssistantAgent.Core.Repositories
{
    /// <summary>
    /// Interface for a repository that manages document metadata and classification.
    /// </summary>
    public interface IDocumentRepository
    {
        Task UpsertDocumentAsync(DocumentInfo document);

        Task<List<DocumentInfo>> GetAllDocumentsAsync(string owner);

        List<DocumentInfo> GetDocumentsByCategory(string owner, DocumentCategory category);

        List<DocumentInfo> GetDocumentsByType(string owner, DocumentType type);

        List<DocumentInfo> GetExpiringDocuments(string owner, int daysAhead = 30);

        List<DocumentInfo> SearchDocuments(string owner, string query);

        Task SaveChangesAsync();
    }
}

using DocumentAssistantAgent.Core.Models;

namespace DocumentAssistantAgent.Core.Services
{
    /// <summary>
    /// Interface for a service that classifies documents.
    /// </summary>
    public interface IDocumentClassifierService
    {
        /// <summary>
        /// Asynchronously classifies a document based on its content.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>DocumentInfo object</returns>
        Task<DocumentInfo> ClassifyDocumentAsync(string filePath);
    }
}

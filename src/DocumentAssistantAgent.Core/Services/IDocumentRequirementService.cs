using DocumentAssistantAgent.Core.Models;

namespace DocumentAssistantAgent.Core.Services
{
    public interface IDocumentRequirementService
    {
        /// <summary>
        /// Retrieves the list of required documents for a specific purpose.
        /// </summary>
        /// <param name="purpose"></param>
        /// <returns></returns>
        Task<List<DocumentRequirement>> GetRequiredDocumentsAsync(string owner, string purpose);
    }
}

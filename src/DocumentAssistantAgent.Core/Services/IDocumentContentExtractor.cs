namespace DocumentAssistantAgent.Core.Services
{
    public interface IDocumentContentExtractor
    {
        Task<string> ProceedAsync(string filePath);
    }
}

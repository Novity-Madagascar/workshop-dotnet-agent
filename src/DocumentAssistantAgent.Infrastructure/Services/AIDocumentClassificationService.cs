using DocumentAssistantAgent.Core.Configurations;
using DocumentAssistantAgent.Core.Models;
using DocumentAssistantAgent.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace DocumentAssistantAgent.Infrastructure.Services
{
    public class AIDocumentClassificationService : IDocumentClassifierService
    {
        #region Fields
        private readonly Kernel _kernel;
        private readonly ILogger<AIDocumentClassificationService> _logger;
        private readonly IDocumentContentExtractor _contentExtractor;
        private readonly DocumentAgentOptions _options;
        #endregion

        #region Constructors
        public AIDocumentClassificationService(
            Kernel kernel,
            ILogger<AIDocumentClassificationService> logger,
            IDocumentContentExtractor contentExtractor,
            IOptions<DocumentAgentOptions> options)
        {
            _kernel = kernel;
            _logger = logger;
            _contentExtractor = contentExtractor;
            _options = options.Value;
        }
        #endregion

        #region Methods
        public async Task<DocumentInfo> ClassifyDocumentAsync(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileInfo = new FileInfo(filePath);

            try
            {
                // Extract text content from the document
                string content = await _contentExtractor.ProceedAsync(filePath);

                // Use AI to classify the document
                var classificationPrompt = GetClassificationPrompt(fileName, content);
                var response = await _kernel.InvokePromptAsync(classificationPrompt);
                var classification = JsonSerializer.Deserialize<DocumentClassification>(response.ToString()) ??
                    throw new InvalidOperationException("Failed to deserialize document classification response.");

                return MapToDocumentInfo(filePath, fileName, fileInfo, classification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying document: {FileName}", fileName);

                return CreateFallbackDocumentInfo(filePath, fileName, fileInfo);
            }
        }
        #endregion

        #region Private Methods
        private string GetClassificationPrompt(string fileName, string content)
        {
            return string.Format(_options.ClassificationPrompt, fileName, content[..Math.Min(content.Length, 500)]);
        }

        private static DocumentInfo MapToDocumentInfo(string filePath, string fileName, FileInfo fileInfo, DocumentClassification classification)
        {
            return new DocumentInfo
            {
                Owner = classification.Owner,
                FileName = fileName,
                FullPath = filePath,
                Category = Enum.Parse<DocumentCategory>(classification.Category),
                Type = Enum.Parse<DocumentType>(classification.Type),
                ExpirationDate = classification.ExpirationDate,
                LastModified = fileInfo.LastWriteTime,
                Summary = classification.Summary,
                Keywords = classification.Keywords
            };
        }

        private static DocumentInfo CreateFallbackDocumentInfo(string filePath, string fileName, FileInfo fileInfo)
        {
            return new DocumentInfo
            {
                FileName = fileName,
                FullPath = filePath,
                Category = DocumentCategory.Other,
                Type = DocumentType.Other,
                LastModified = fileInfo.LastWriteTime
            };
        }
        #endregion
    }
}

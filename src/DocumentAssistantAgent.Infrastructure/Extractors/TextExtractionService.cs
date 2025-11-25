using DocumentAssistantAgent.Core.Services;
using Microsoft.Extensions.Logging;

namespace DocumentAssistantAgent.Infrastructure.Extractors
{
    public class TextExtractionService : DocumentContentExtractionBase
    {
        #region Fields
        private readonly ILogger<TextExtractionService> _logger;
        #endregion

        #region Properties
        public override string[] SupportedExtensions => [".txt", ".md", ".rtf"];
        #endregion

        #region Constructors
        public TextExtractionService(ILogger<TextExtractionService> logger) 
            : base()
        {
            _logger = logger;
        }

        #endregion

        #region Methods
        public override async Task<string> ExtractTextAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }
            try
            {

                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting text from file: {filePath}");
                throw new InvalidOperationException($"Failed to extract text from file: {filePath}", ex);
            }
        }
        #endregion
    }
}

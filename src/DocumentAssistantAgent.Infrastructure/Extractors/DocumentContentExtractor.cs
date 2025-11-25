using DocumentAssistantAgent.Core.Services;
using Microsoft.Extensions.Logging;

namespace DocumentAssistantAgent.Infrastructure.Extractors
{
    public class DocumentContentExtractor : IDocumentContentExtractor
    {
        #region Fields
        private readonly ILogger<DocumentContentExtractor> _logger;
        private readonly Dictionary<string, DocumentContentExtractionBase> _extractors;
        private readonly DocumentContentExtractionBase? _fallbackExtractor;
        #endregion

        #region Constructors
        public DocumentContentExtractor(
        IEnumerable<DocumentContentExtractionBase> extractors,
        ILogger<DocumentContentExtractor> logger)
        {
            _logger = logger;

            // Create a dictionary mapping extensions to extractors
            _extractors = [];

            foreach (var extractor in extractors.OrderByDescending(e => e.Priority))
            {
                foreach (var extension in extractor.SupportedExtensions)
                {
                    var normalizedExt = extension.ToLowerInvariant();

                    // Use the first (highest priority) extractor for each extension
                    if (!_extractors.ContainsKey(normalizedExt))
                    {
                        _extractors[normalizedExt] = extractor;
                        _logger.LogDebug("Registered {ExtractorType} for extension {Extension}",
                            extractor.GetType().Name, normalizedExt);
                    }
                    else
                    {
                        _logger.LogDebug("Extension {Extension} already handled by {ExtractorType}, skipping {SkippedType}",
                            normalizedExt, _extractors[normalizedExt].GetType().Name, extractor.GetType().Name);
                    }
                }
            }

            // Set fallback extractor (typically the text extractor)
            _fallbackExtractor = extractors.FirstOrDefault(e => e is TextExtractionService);

            _logger.LogInformation("Initialized DocumentContentExtractionService with {Count} extractors supporting {Extensions}",
                extractors.Count(), string.Join(", ", _extractors.Keys));
        }
        #endregion

        #region Methods
        public async Task<string> ProceedAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            // Try to find a specific extractor for this extension
            if (_extractors.TryGetValue(extension, out var extractor))
            {
                try
                {
                    _logger.LogDebug("Using {ExtractorType} for file: {FilePath}",
                        extractor.GetType().Name, filePath);
                    return await extractor.ExtractTextAsync(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to extract using {ExtractorType}, trying fallback for: {FilePath}",
                        extractor.GetType().Name, filePath);
                }
            }
            else
            {
                _logger.LogDebug("No specific extractor found for extension {Extension}, using fallback for: {FilePath}",
                    extension, filePath);
            }

            // Use fallback extractor or return filename
            if (_fallbackExtractor != null)
            {
                try
                {
                    return await _fallbackExtractor.ExtractTextAsync(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fallback extractor failed for: {FilePath}", filePath);
                }
            }

            // Ultimate fallback - return filename
            _logger.LogInformation("Using filename as content for unsupported file: {FilePath}", filePath);
            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// Gets all supported file extensions
        /// </summary>
        public string[] GetSupportedExtensions()
        {
            return [.. _extractors.Keys];
        }

        /// <summary>
        /// Checks if a file extension is supported
        /// </summary>
        public bool IsSupported(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return _extractors.ContainsKey(extension);
        }
        #endregion
    }
}

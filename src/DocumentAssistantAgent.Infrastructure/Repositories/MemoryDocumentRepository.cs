using DocumentAssistantAgent.Core.Configurations;
using DocumentAssistantAgent.Core.Models;
using DocumentAssistantAgent.Core.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DocumentAssistantAgent.Infrastructure.Repositories
{
    public class MemoryDocumentRepository : IDocumentRepository
    {
        #region Fields
        private readonly string _indexPath;
        private readonly DocumentAgentOptions _options;
        private readonly ILogger<MemoryDocumentRepository> _logger;
        private List<DocumentInfo> _documents;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        #endregion

        #region Constructors
        public MemoryDocumentRepository(ILogger<MemoryDocumentRepository> logger, IOptions<DocumentAgentOptions> options)
        {
            _options = options.Value ??
                throw new ArgumentNullException(nameof(options));
            _indexPath = _options.IndexFilePath;
            _logger = logger;
            _documents = LoadIndex();
        }
        #endregion

        #region Methods
        public async Task UpsertDocumentAsync(DocumentInfo document)
        {
            await _semaphore.WaitAsync();
            try
            {
                var existing = _documents.FirstOrDefault(d => d.FullPath == document.FullPath);
                if (existing != null)
                {
                    _documents.Remove(existing);
                }

                _documents.Add(document);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<DocumentInfo>> GetAllDocumentsAsync(string owner)
        {
            await _semaphore.WaitAsync();
            try
            {
                return [.. _documents];
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public List<DocumentInfo> GetDocumentsByCategory(string owner, DocumentCategory category)
        {
            return _documents.Where(d => d.Category == category).ToList();
        }

        public List<DocumentInfo> GetDocumentsByType(string owner, DocumentType type)
        {
            return _documents.Where(d => d.Type == type).ToList();
        }

        public List<DocumentInfo> GetExpiringDocuments(string owner, int daysAhead = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);

            return _documents
                .Where(d => d.ExpirationDate.HasValue && d.ExpirationDate.Value <= cutoffDate)
                .OrderBy(d => d.ExpirationDate)
                .ToList();
        }

        public List<DocumentInfo> SearchDocuments(string owner, string query)
        {
            var queryLower = query.ToLower();

            return _documents.Where(d =>
                d.FileName.Contains(queryLower, StringComparison.CurrentCultureIgnoreCase) ||
                d.Summary?.Contains(queryLower, StringComparison.CurrentCultureIgnoreCase) == true ||
                d.Keywords.Any(k => k.Contains(queryLower, StringComparison.CurrentCultureIgnoreCase))
            ).ToList();
        }

        public async Task SaveChangesAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(_documents, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_indexPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving document index");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        #endregion

        #region Private Methods
        private List<DocumentInfo> LoadIndex()
        {
            if (File.Exists(_indexPath))
            {
                try
                {
                    var json = File.ReadAllText(_indexPath);

                    return JsonSerializer.Deserialize<List<DocumentInfo>>(json) ?? new List<DocumentInfo>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading document index");
                }
            }
            else
            {
                _logger.LogInformation("Document index file not found, creating a new one.");

                try
                {
                    File.WriteAllText(_indexPath, JsonSerializer.Serialize(new List<DocumentInfo>()));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating document index file");
                }
            }

            return [];
        }
        #endregion
    }
}

using DocumentAssistantAgent.Core.Models;
using DocumentAssistantAgent.Core.Repositories;
using LiteDB;

namespace DocumentAssistantAgent.Infrastructure.Repositories
{
    public class LiteDbDocumentRepository : IDocumentRepository
    {
        #region Fields
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<DocumentInfo> _collection;
        #endregion

        #region Constructors
        public LiteDbDocumentRepository(string databasePath = "documents.db")
        {
            _db = new LiteDatabase(databasePath);
            _collection = _db.GetCollection<DocumentInfo>("documents");
        }
        #endregion

        #region Methods
        public async Task<List<DocumentInfo>> GetAllDocumentsAsync(string owner)
        {
            var results = _collection.Find(d => d.Owner.Equals(owner, StringComparison.CurrentCultureIgnoreCase));

            return await Task.FromResult(results.ToList());
        }

        public List<DocumentInfo> GetDocumentsByCategory(string owner, DocumentCategory category)
        {
            var results = _collection.Find(d => d.Owner.Equals(owner, StringComparison.CurrentCultureIgnoreCase) 
            && d.Category == category);

            return [.. results];
        }

        public List<DocumentInfo> GetDocumentsByType(string owner, DocumentType type)
        {
            var results = _collection.Find(d => d.Type == type);

            return [.. results];
        }

        public List<DocumentInfo> GetExpiringDocuments(string owner, int daysAhead = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);

            return _collection
                .Find(d => d.Owner.Equals(owner, StringComparison.CurrentCultureIgnoreCase) && 
                    d.ExpirationDate.HasValue && d.ExpirationDate.Value <= cutoffDate)
                .OrderBy(d => d.ExpirationDate)
                .ToList();
        }

        public Task SaveChangesAsync()
        {
            // LiteDB automatically saves changes, so no explicit save is needed.
            return Task.CompletedTask;
        }

        public List<DocumentInfo> SearchDocuments(string owner, string query)
        {
            var queryLower = query.ToLower();

            return _collection.Find(d =>
                d.Owner.Equals(owner, StringComparison.CurrentCultureIgnoreCase) &&
                (d.FileName.Contains(queryLower, StringComparison.CurrentCultureIgnoreCase) ||
                (d.Summary != null && d.Summary.Contains(queryLower, StringComparison.CurrentCultureIgnoreCase)) ||
                d.Keywords.Any(k => k.Contains(queryLower, StringComparison.CurrentCultureIgnoreCase)))
            ).ToList();
        }

        public async Task UpsertDocumentAsync(DocumentInfo document)
        {
            await Task.Run(() => _collection.Upsert(document.FullPath, document));
        }
        #endregion
    }
}

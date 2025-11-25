using DocumentAssistantAgent.Core.Models;
using DocumentAssistantAgent.Core.Repositories;
using DocumentAssistantAgent.Core.Services;
using DocumentAssistantAgent.Infrastructure.Utils;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace DocumentAssistantAgent.Client.Plugins
{
    /// <summary>
    /// Represents a plugin for the Document Assistant Agent that provides functionality
    /// </summary>
    public class DocumentAssistantPlugin
    {
        #region Fields
        private readonly IDocumentRepository _repository;
        private readonly IDocumentClassifierService _classifierService;
        private readonly IDocumentRequirementService _requirementService;
        private readonly ILogger<DocumentAssistantPlugin> _logger;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAssistantPlugin"/> class.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="classifierService"></param>
        /// <param name="requirementService"></param>
        /// <param name="logger"></param>
        public DocumentAssistantPlugin(
            IDocumentRepository repository,
            IDocumentClassifierService classifierService,
            IDocumentRequirementService requirementService,
            ILogger<DocumentAssistantPlugin> logger)
        {
            _repository = repository;
            _classifierService = classifierService;
            _requirementService = requirementService;
            _logger = logger;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Scans a folder for documents, classifies them, and stores the results in the repository.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>

        [KernelFunction("scan_folder")]
        [Description("Scan a folder and classify all documents in it")]
        public async Task<string> ScanFolderAsync(
            [Description("Path to the folder to scan")] string folderPath,
            [Description("Whether to scan subdirectories")] bool recursive = true)
        {
            if (!Directory.Exists(folderPath))
            {
                return $"Folder not found: {folderPath}";
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(folderPath, "*", searchOption)
                .Where(FileUtils.IsDocumentFile)
                .ToList();

            int processed = 0;
            foreach (var file in files)
            {
                try
                {
                    var documentInfo = await _classifierService.ClassifyDocumentAsync(file);
                    await _repository.UpsertDocumentAsync(documentInfo);
                    processed++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file: {File}", file);
                }
            }

            await _repository.SaveChangesAsync();

            return $"Processed {processed} out of {files.Count} files in {folderPath}";
        }

        /// <summary>
        /// Gets a list of documents needed for a specific purpose and creates a zip file with them.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="purpose"></param>
        /// <param name="zipPath"></param>
        /// <returns></returns>
        [KernelFunction("get_documents_for_purpose")]
        [Description("Get list of documents needed for a specific purpose and create a zip file")]
        public async Task<string> GetDocumentsForPurposeAsync(
            [Description("Owner name (e.g., 'John Doe', 'Jane Doe')")] string owner,
            [Description("Purpose description (e.g., 'renew passport', 'job application')")] string purpose,
            [Description("Output zip file path")] string zipPath = "documents.zip")
        {
            var requirements = await _requirementService.GetRequiredDocumentsAsync(owner, purpose.ToLower());

            var foundDocuments = new List<DocumentInfo>();
            var missingDocuments = new List<string>();

            foreach (var requirement in requirements)
            {
                var docs = _repository.SearchDocuments(owner, requirement.SearchTerm);
                if (string.IsNullOrWhiteSpace(requirement.Category))
                {
                    docs = [.. docs.Where(d => d.Category.ToString() == requirement.Category)];
                }
                if (string.IsNullOrWhiteSpace(requirement.Type))
                {
                    docs = [.. docs.Where(d => d.Type.ToString() == requirement.Type)];
                }

                if (docs.Count != 0)
                {
                    // Get the most recent document
                    foundDocuments.Add(docs.OrderByDescending(d => d.LastModified).First());
                }
                else
                {
                    missingDocuments.Add(requirement.Description);
                }
            }

            if (foundDocuments.Count != 0)
            {
                await ZipUtils.CreateZipFileAsync(foundDocuments, zipPath);
            }

            return FormatDocumentResult(purpose, foundDocuments, missingDocuments);
        }

        /// <summary>
        /// Checks for documents that are expiring soon and returns a summary.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="daysAhead"></param>
        /// <returns></returns>
        [KernelFunction("check_expiring_documents")]
        [Description("Check for documents that are expiring soon")]
        public string CheckExpiringDocuments(
            [Description("Owner name (e.g., 'John Doe', 'Jane Doe')")] string owner,
            [Description("Days ahead to check")] int daysAhead = 30)
        {
            var expiringDocs = _repository.GetExpiringDocuments(owner, daysAhead);

            if (expiringDocs.Count == 0)
            {
                return $"No documents expiring in the next {daysAhead} days.";
            }

            var result = $"Found {expiringDocs.Count} document(s) expiring in the next {daysAhead} days:\n";
            foreach (var doc in expiringDocs)
            {
                var daysUntilExpiry = (doc.ExpirationDate!.Value - DateTime.Now).Days;
                result += $"- {doc.FileName} expires on {doc.ExpirationDate:yyyy-MM-dd} ({daysUntilExpiry} days)\n";
            }

            return result;
        }
        #endregion

        #region Private Methods
        private static string FormatDocumentResult(string purpose, List<DocumentInfo> foundDocuments, List<string> missingDocuments)
        {
            var result = $"Created zip file with {foundDocuments.Count} documents for: {purpose}\n";
            result += $"Found documents:\n{string.Join("\n", foundDocuments.Select(d => $"- {d.FileName}"))}\n";

            if (missingDocuments.Count != 0)
            {
                result += $"\nMissing documents:\n{string.Join("\n", missingDocuments.Select(m => $"- {m}"))}";
            }

            return result;
        }
        #endregion
    }
}

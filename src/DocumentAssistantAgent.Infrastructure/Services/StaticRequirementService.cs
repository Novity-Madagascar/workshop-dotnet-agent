using DocumentAssistantAgent.Core.Models;
using DocumentAssistantAgent.Core.Services;
using Microsoft.Extensions.Logging;

namespace DocumentAssistantAgent.Infrastructure.Services
{
    public class StaticRequirementService : IDocumentRequirementService
    {
        #region Fields
        private readonly ILogger<StaticRequirementService> _logger;
        private readonly Dictionary<string, List<DocumentRequirement>> _customRequirements = new();
        #endregion

        #region Constructors
        public StaticRequirementService(
            ILogger<StaticRequirementService> logger)
        {
            _logger = logger;
        }
        #endregion

        #region Methods
        public async Task<List<DocumentRequirement>> GetRequiredDocumentsAsync(string owner, string purpose)
        {
            // Check custom requirements first
            if (_customRequirements.ContainsKey(purpose))
            {
                return await Task.FromResult(_customRequirements[purpose]);
            }

            // Default requirements based on common purposes
            return purpose switch
            {
                var p when p.Contains("passport") || p.Contains("renew passport") => new()
            {
                new("passport", "Current passport", DocumentCategory.Identity.ToString(), DocumentType.Passport.ToString()),
                new("birth certificate", "Birth certificate", DocumentCategory.Identity.ToString(), DocumentType.BirthCertificate.ToString()),
                new("photo", "Passport photo", DocumentCategory.Identity.ToString())
            },
                var p when p.Contains("job") || p.Contains("employment") => new()
            {
                new("resume", "Resume/CV", DocumentCategory.Professional.ToString(), DocumentType.Resume.ToString()),
                new("certificate", "Educational certificates", DocumentCategory.School.ToString(), DocumentType.Certificate.ToString()),
                new("id", "Identity document", DocumentCategory.Identity.ToString())
            },
                var p when p.Contains("school") || p.Contains("university") => new()
            {
                new("transcript", "Academic transcript", DocumentCategory.School.ToString()),
                new("certificate", "Educational certificates", DocumentCategory.School.ToString(), DocumentType.Certificate.ToString()),
                new("birth certificate", "Birth certificate", DocumentCategory.Identity.ToString(), DocumentType.BirthCertificate.ToString())
            },
                var p when p.Contains("visa") => new()
            {
                new("passport", "Valid passport", DocumentCategory.Identity.ToString(), DocumentType.Passport.ToString()),
                new("photo", "Visa photo", DocumentCategory.Identity.ToString()),
                new("financial", "Financial documents", DocumentCategory.Professional.ToString())
            },
                _ => []
            };
        }
        #endregion
    }
}

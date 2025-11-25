using DocumentAssistantAgent.Core.Configurations;
using DocumentAssistantAgent.Core.Models;
using DocumentAssistantAgent.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace DocumentAssistantAgent.Infrastructure.Services
{
    public class AIRequirementService : IDocumentRequirementService
    {
        #region Fields
        private readonly Kernel _kernel;
        private readonly ILogger<AIRequirementService> _logger;
        private DocumentAgentOptions _options;
        #endregion

        #region Constructors
        public AIRequirementService(
            Kernel kernel,
            ILogger<AIRequirementService> logger,
            IOptions<DocumentAgentOptions> options)
        {
            _kernel = kernel;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }
        #endregion

        #region Methods
        public async Task<List<DocumentRequirement>> GetRequiredDocumentsAsync(string owner, string purpose)
        {
            try
            {
                // Use AI to get the list of required documents for a purpose
                var requirementPrompt = GetRequirementPrompt(purpose);
                var response = await _kernel.InvokePromptAsync(requirementPrompt);
                var requirements = JsonSerializer.Deserialize<List<DocumentRequirement>>(response.ToString()) ??
                    throw new InvalidOperationException("Failed to deserialize document requirements response.");

                return requirements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document requirements for purpose: {Purpose}", purpose);

                return [];
            }
        }
        #endregion

        #region Private Methods
        private string GetRequirementPrompt(string purpose)
        {
            return string.Format(_options.RequirementPrompt, purpose);
        }
        #endregion
    }
}

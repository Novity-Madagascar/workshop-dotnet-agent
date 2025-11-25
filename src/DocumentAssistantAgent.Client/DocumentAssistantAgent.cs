using DocumentAssistantAgent.Client.Plugins;
using DocumentAssistantAgent.Core.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace DocumentAssistantAgent.Client
{
    /// <summary>
    /// Main entry point for the Document Assistant Agent application.
    /// </summary>
    public class DocumentAssistantAgent : BackgroundService
    {
        #region Fields
        private readonly DocumentAgentOptions _options;
        private readonly Kernel _kernel;
        private readonly DocumentAssistantPlugin _documentPlugin;
        private readonly ILogger<DocumentAssistantAgent> _logger;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAssistantAgent"/> class.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="kernel"></param>
        /// <param name="documentPlugin"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DocumentAssistantAgent(
            IOptions<DocumentAgentOptions> options,
            Kernel kernel,
            DocumentAssistantPlugin documentPlugin, 
            ILogger<DocumentAssistantAgent> logger)
        {
            _logger = logger;

            _options = options.Value
                ?? throw new ArgumentNullException(nameof(options));
            _kernel = kernel 
                ?? throw new ArgumentNullException(nameof(kernel));
            _documentPlugin = documentPlugin 
                ?? throw new ArgumentNullException(nameof(documentPlugin));

            // Register the plugin
            _kernel.Plugins.AddFromObject(_documentPlugin);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the background service to periodically check for expiring documents and process user requests.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Document Assistant Agent started");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                //try
                //{
                //    var expiringDocs = _documentPlugin.CheckExpiringDocuments();
                //    if (!expiringDocs.Contains("No documents"))
                //    {
                //        _logger.LogWarning("Document expiration alert: {Message}", expiringDocs);
                //        // Here you could send notifications via email, desktop notification, etc.
                //    }

                //    await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // Check daily
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogError(ex, "Error in background document check");

                //    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Retry in 1 hour
                //}
            }
        }

        /// <summary>
        /// Processes user requests by invoking the appropriate plugin functions based on the input.
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns></returns>
        public async Task<string> ProcessUserRequestAsync(string userInput)
        {
            try
            {
                var chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();

                var chatHistory = new ChatHistory(_options.SystemPrompt);
                chatHistory.AddUserMessage(userInput);

                var response = await chatCompletion.GetChatMessageContentAsync(
                    chatHistory,
                    new OpenAIPromptExecutionSettings
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    },
                    _kernel);

                return response.Content ?? "I couldn't process your request.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user request: {Input}", userInput);
                return "I encountered an error processing your request. Please try again.";
            }
        }
        #endregion
    }
}

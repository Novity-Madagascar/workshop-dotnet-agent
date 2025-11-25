using DocumentAssistantAgent.Client.Plugins;
using DocumentAssistantAgent.Core.Configurations;
using DocumentAssistantAgent.Core.Repositories;
using DocumentAssistantAgent.Core.Services;
using DocumentAssistantAgent.Infrastructure.Extractors;
using DocumentAssistantAgent.Infrastructure.Repositories;
using DocumentAssistantAgent.Infrastructure.Services;
using Microsoft.SemanticKernel;

namespace DocumentAssistantAgent.Client.Extensions
{
    /// <summary>
    /// Extension methods for configuring services in the Document Assistant Agent application.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the Document Management Agent services and configurations in the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddDocumentManagementAgent(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure options
            services.Configure<DocumentAgentOptions>(
                configuration.GetSection(DocumentAgentOptions.SectionName));

            ConfigureAIService(services, configuration);

            // Add content extraction services
            services.AddDocumentContentExtraction();

            // Register application services
            services.AddSingleton<IDocumentClassifierService, AIDocumentClassificationService>();
            //services.AddSingleton<IDocumentRepository, MemoryDocumentRepository>();
            services.AddSingleton<IDocumentRepository, LiteDbDocumentRepository>();
            //services.AddSingleton<IDocumentRequirementService, StaticRequirementService>();
            services.AddSingleton<IDocumentRequirementService, AIRequirementService>();
            services.AddSingleton<INotificationService, ConsoleNotificationService>();
            services.AddSingleton<DocumentAssistantPlugin>();
            services.AddSingleton<DocumentAssistantAgent>();

            // Register the main agent as a hosted service
            services.AddHostedService<DocumentAssistantAgent>();

            return services;
        }

        private static void ConfigureAIService(IServiceCollection services, IConfiguration configuration)
        {
            // Configure Semantic Kernel
            var kernelBuilder = services.AddKernel();

            var openAIKey = configuration["OpenAI:ApiKey"];
            var openAIModel = configuration["OpenAI:ModelId"] ?? "gpt-4";

            if (!string.IsNullOrEmpty(openAIKey))
            {
                kernelBuilder.AddOpenAIChatCompletion(openAIModel, openAIKey);
                return;
            }
            else
            {
                // Try Azure OpenAI configuration
                var azureEndpoint = configuration["AzureOpenAI:Endpoint"];
                var azureApiKey = configuration["AzureOpenAI:ApiKey"];
                var azureDeployment = configuration["AzureOpenAI:DeploymentName"];

                if (!string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureApiKey))
                {
                    kernelBuilder.AddAzureOpenAIChatCompletion(azureDeployment!, azureEndpoint, azureApiKey);
                    return;
                }
            }

            throw new InvalidOperationException(
                "No AI service configuration found. Please configure OpenAI or Azure OpenAI settings.");
        }

        /// <summary>
        /// Registers the document content extraction services in the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDocumentContentExtraction(this IServiceCollection services)
        {
            // Register all content extraction implementations
            services.AddSingleton<DocumentContentExtractionBase, TextExtractionService>();
            services.AddSingleton<DocumentContentExtractionBase, PdfExtractionService>();

            // Register the factory service
            services.AddSingleton<IDocumentContentExtractor, DocumentContentExtractor>();

            return services;
        }
    }
}

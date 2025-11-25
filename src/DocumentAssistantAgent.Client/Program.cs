using DocumentAssistantAgent.Client.Extensions;

namespace DocumentAssistantAgent.Client
{
    /// <summary>
    /// Main entry point for the Document Assistant Agent application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the Document Assistant Agent application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var services = builder.Services;

            services.AddDocumentManagementAgent(builder.Configuration);

            services.AddLogging();

            var host = builder.Build();

            // Example usage
            var agent = host.Services.GetRequiredService<DocumentAssistantAgent>();

            // Scan documents
            Console.WriteLine("Document Assistant Agent Starting...");
            Console.WriteLine("Available commands:");
            Console.WriteLine("- 'Scan my Documents folder and classify all files'");
            Console.WriteLine("- 'I need to renew my passport, what documents do I need?'");
            Console.WriteLine("- 'Check if any of my documents are expiring soon'");
            Console.WriteLine("- 'exit' to quit");

            // Interactive mode
            while (true)
            {
                Console.Write("\n> ");
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
                    break;

                try
                {
                    var response = await agent.ProcessUserRequestAsync(input);
                    Console.WriteLine($"\nAgent> {response}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            await host.RunAsync();
        }
    }
}
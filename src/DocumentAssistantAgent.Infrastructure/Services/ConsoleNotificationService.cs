using DocumentAssistantAgent.Core.Configurations;
using DocumentAssistantAgent.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocumentAssistantAgent.Infrastructure.Services
{
    public class ConsoleNotificationService : INotificationService
    {
        #region Fields
        private readonly DocumentAgentOptions _options;
        private readonly ILogger<ConsoleNotificationService> _logger;
        #endregion

        #region Constructors
        public ConsoleNotificationService(
            IOptions<DocumentAgentOptions> options,
            ILogger<ConsoleNotificationService> logger)
        {
            _options = options.Value
                ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
        }
        #endregion

        #region Methods
        public async Task SendExpirationAlertAsync(string documentName, DateTime expirationDate)
        {
            var daysUntilExpiry = (expirationDate - DateTime.Now).Days;
            var urgencyIcon = daysUntilExpiry <= 7 ? "🚨" : daysUntilExpiry <= _options.ExpirationCheckDays ? "⚠️" : "📅";

            await SendNotificationAsync(
                $"{urgencyIcon} Document Expiring",
                $"{documentName} expires on {expirationDate:yyyy-MM-dd} ({daysUntilExpiry} days remaining)");
        }

        public async Task SendNotificationAsync(string title, string message)
        {
            Console.WriteLine($"\n🔔 {title}");
            Console.WriteLine($"   {message}");
            _logger.LogInformation("Notification sent: {Title} - {Message}", title, message);

            await Task.CompletedTask;
        }
        #endregion
    }
}

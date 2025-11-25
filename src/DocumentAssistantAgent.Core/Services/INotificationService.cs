namespace DocumentAssistantAgent.Core.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification with the specified title and message.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendNotificationAsync(string title, string message);

        /// <summary>
        /// Sends an alert when a document is about to expire.
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="expirationDate"></param>
        /// <returns></returns>
        Task SendExpirationAlertAsync(string documentName, DateTime expirationDate);
    }
}

namespace DocumentAssistantAgent.Infrastructure.Utils
{
    public static class FileUtils
    {
        private static readonly string[] DocumentExtensions =
        {
            ".pdf", ".doc", ".docx", ".txt", ".md", ".rtf",
            ".jpg", ".jpeg", ".png", ".tiff", ".bmp", ".gif"
        };

        public static bool IsDocumentFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return DocumentExtensions.Contains(extension);
        }

        public static bool IsImageFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return new[] { ".jpg", ".jpeg", ".png", ".tiff", ".bmp", ".gif" }.Contains(extension);
        }

        public static bool IsTextFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return new[] { ".txt", ".md", ".rtf" }.Contains(extension);
        }
    }
}

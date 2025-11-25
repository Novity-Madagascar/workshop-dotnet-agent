namespace DocumentAssistantAgent.Core.Services
{
    /// <summary>
    /// Interface for a service that extracts text content from documents.
    /// </summary>
    public abstract class DocumentContentExtractionBase
    {
        #region Fields
        #endregion

        #region Properties
        /// <summary>
        /// The file type that this service can handle, e.g., "pdf", "docx", etc.
        /// </summary>
        public abstract string[] SupportedExtensions { get; }

        public virtual int Priority => 0; // Default priority, can be overridden by derived classes
        #endregion

        #region Constructors
        protected DocumentContentExtractionBase()
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Asynchronously extracts text content from a document file at the specified path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public abstract Task<string> ExtractTextAsync(string filePath);

        /// <summary>
        /// Checks if the file extension is supported by this extractor
        /// </summary>
        public virtual bool CanExtract(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return SupportedExtensions.Contains(extension);
        }
        #endregion
    }
}

namespace DocumentAssistantAgent.Core.Models
{
    /// <summary>
    /// Represents the category of a document.
    /// </summary>
    public enum DocumentCategory
    {
        Identity,
        Professional,
        Kids,
        School,
        Medical,
        Other
    }

    /// <summary>
    /// Represents the type of a document. 
    /// </summary>
    public enum DocumentType
    {
        Passport,
        DriverLicense,
        BirthCertificate,
        Resume,
        Contract,
        Certificate,
        Report,
        Visa,
        Permit,
        Photo,
        Other
    }
}

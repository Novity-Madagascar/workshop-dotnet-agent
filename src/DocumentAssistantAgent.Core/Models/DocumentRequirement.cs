namespace DocumentAssistantAgent.Core.Models
{
    /// <summary>
    /// Represents a requirement for a document, including search term, description, category, and type.
    /// </summary>
    /// <param name="SearchTerm"></param>
    /// <param name="Description"></param>
    /// <param name="Category"></param>
    /// <param name="Type"></param>
    public record DocumentRequirement
    (
        string SearchTerm,
        string Description,
        string? Category = null,
        string? Type = null
    );
}

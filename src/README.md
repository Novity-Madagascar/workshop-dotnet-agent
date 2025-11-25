## Technical Documentation: Document Assistant Agent - An Introduction to AI Agent Creation with .NET

This document outlines the architecture and implementation of the Document Assistant Agent, a practical example of building AI-powered agents using .NET and Microsoft Semantic Kernel. The solution demonstrates how to integrate advanced AI capabilities like document classification and intelligent requirement determination into a robust application.

### 1. Overall Architecture and AI Components

The Document Assistant Agent is designed to automate document management tasks, leveraging Artificial Intelligence to classify documents, identify requirements, and facilitate document retrieval. The core of the system revolves around a .NET application structured into several projects, embodying an agent-oriented approach where distinct components collaborate to achieve complex goals:

*   **DocumentAssistantAgent.Client**: The entry point of the application, responsible for orchestrating the various services and interacting with the user. This acts as the agent's interface and control center.
*   **DocumentAssistantAgent.Core**: Contains the foundational elements, including models (e.g., `DocumentInfo`, `DocumentClassification`, `DocumentRequirement`), interfaces for repositories (e.g., `IDocumentRepository`), and services (e.g., `IDocumentClassifierService`, `IDocumentRequirementService`). This layer defines the agent's capabilities and data structures.
*   **DocumentAssistantAgent.Infrastructure**: Implements the core interfaces defined in `DocumentAssistantAgent.Core`. This is where the AI integration primarily resides, specifically within the `Plugins` and `Services` directories. This layer provides the agent's "senses" (e.g., document content extraction) and "actions" (e.g., AI classification, requirement determination).

The AI components are built using **Microsoft Semantic Kernel**, which acts as the orchestrator for AI interactions. It allows the application to define "skills" or "plugins" that encapsulate AI capabilities, making them callable as functions within the application logic. This framework is crucial for building flexible and extensible AI agents in .NET, enabling them to perform complex tasks by chaining together AI and conventional code.

### 2. `DocumentAssistantPlugin.cs` Implementation

The `DocumentAssistantPlugin.cs` file defines a Semantic Kernel plugin that exposes several AI-powered functions to the agent. These functions are decorated with `[KernelFunction]` and `[Description]` attributes, making them discoverable and usable by the Semantic Kernel.

Key Kernel Functions:

*   `ScanFolderAsync`: This function scans a specified folder (and optionally subdirectories) for documents. For each document, it leverages the `IDocumentClassifierService` (implemented by `AIDocumentClassificationService`) to classify the document's category, type, and extract relevant information. The classified document information is then stored in the document repository.
*   `GetDocumentsForPurposeAsync`: Given a specific purpose (e.g., "renew passport"), this function uses the `IDocumentRequirementService` (implemented by `AIRequirementService`) to determine the required documents. It then searches the local document repository for these documents and, if found, creates a zip file containing them. It also reports any missing documents.
*   `CheckExpiringDocuments`: This function queries the document repository to find documents that are expiring within a specified number of days.

These functions demonstrate how the AI agent can perform complex, multi-step operations by combining AI capabilities (classification, requirement determination) with traditional application logic (file system operations, database interactions).

### 3. `AIDocumentClassificationService.cs`

The `AIDocumentClassificationService.cs` is responsible for classifying documents using an AI model. It implements the `IDocumentClassifierService` interface.

*   **AI Integration**: The service utilizes a `Kernel` instance (from Semantic Kernel) to invoke AI prompts. The core of its functionality lies in the `ClassifyDocumentAsync` method.
*   **Prompt Engineering**: Before sending content to the AI model, the service constructs a `classificationPrompt` using a predefined template (`_options.ClassificationPrompt`). This prompt includes the document's file name and a truncated version of its extracted text content. This is a crucial aspect of prompt engineering, guiding the AI to provide a structured classification.
*   **Content Extraction**: It relies on an `IDocumentContentExtractor` to get the text content from various document types (e.g., PDF, plain text).
*   **Deserialization**: The AI model's response, which is expected to be in JSON format, is deserialized into a `DocumentClassification` object. This structured output allows the application to easily process and store the AI's classification results.
*   **Error Handling**: Includes a fallback mechanism to create a basic `DocumentInfo` object if AI classification fails, ensuring robustness.

### 4. `AIRequirementService.cs`

The `AIRequirementService.cs` is responsible for determining document requirements for a given purpose using an AI model. It implements the `IDocumentRequirementService` interface.

*   **AI Integration**: Similar to the classification service, this service uses a `Kernel` instance to invoke AI prompts. The key method is `GetRequiredDocumentsAsync`.
*   **Prompt Engineering**: It constructs a `requirementPrompt` using `_options.RequirementPrompt` and the specified `purpose`. This prompt guides the AI to list the types of documents required for that purpose.
*   **Deserialization**: The AI's response, expected to be a JSON array of `DocumentRequirement` objects, is deserialized into a `List<DocumentRequirement>`. This allows the application to understand and act upon the AI's determination of required documents.
*   **Error Handling**: Provides an empty list of requirements if the AI call fails.

### 5. AI Integration Points and Benefits

The Document Assistant Agent heavily relies on AI at several critical junctures, providing significant benefits:

*   **Intelligent Document Classification**: Instead of rigid rule-based classification, the AI can understand the content of documents and categorize them dynamically. This makes the system adaptable to new document types and reduces the need for manual configuration.
*   **Dynamic Requirement Determination**: The AI's ability to interpret a "purpose" (e.g., "job application") and translate it into a list of required documents is a powerful feature. It eliminates the need for pre-defined, exhaustive lists of requirements for every possible scenario, making the system highly flexible.
*   **Semantic Understanding**: By using AI, the system moves beyond keyword matching to a more semantic understanding of document content and user intent. This leads to more accurate classifications and more relevant document retrieval.
*   **Extensibility with Semantic Kernel**: The use of Microsoft Semantic Kernel provides a structured way to integrate AI capabilities. New AI functions can be easily added as Kernel Functions, and different AI models can be swapped out with minimal changes to the core application logic.
*   **Automation and Efficiency**: The AI-powered features automate tasks that would otherwise require significant manual effort, such as sorting documents or identifying what's needed for a specific process. This increases efficiency and reduces human error.

In summary, the AI Agent implementation within this solution transforms a traditional document management system into an intelligent, adaptive, and highly efficient assistant, capable of understanding and acting upon complex document-related tasks.
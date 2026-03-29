using System;
using System.IO;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IPythonAiService
    {
        /// <summary>
        /// Sends a document to the Python AI service for extraction.
        /// </summary>
        /// <param name="fileStream">The document stream</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="docType">"cv" or "jd"</param>
        /// <returns>JSON string containing the extracted structure</returns>
        Task<string> ExtractDocumentToJsonAsync(Stream fileStream, string fileName, string docType);
    }
}

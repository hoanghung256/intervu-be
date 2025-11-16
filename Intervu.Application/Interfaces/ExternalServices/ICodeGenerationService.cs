using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface ICodeGenerationService
    {
        string Language { get; }

        string GenerateTemplate(string functionName, object[] testCases);

        string GenerateTestHarness(string userCode, string functionName, JsonElement testCase);
    }
}

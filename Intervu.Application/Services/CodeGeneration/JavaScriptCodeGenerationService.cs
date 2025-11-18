using Intervu.Application.Interfaces.ExternalServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Intervu.Application.Services.CodeGeneration
{
    public class JavaScriptCodeGenerationService : ICodeGenerationService
    {
        private readonly ILogger<JavaScriptCodeGenerationService> _logger;
        public string Language => "javascript";

        public JavaScriptCodeGenerationService(ILogger<JavaScriptCodeGenerationService> logger)
        {
            _logger = logger;
        }

        public string GenerateTemplate(string functionName, object[] testCases)
        {
            try
            {
                if (testCases.Length == 0) return null;

                var firstTestCase = (JsonElement)testCases[0];
                var inputs = firstTestCase.GetProperty("inputs").EnumerateArray().ToList();
                var paramStrings = inputs.Select(input =>
                {
                    var name = input.GetProperty("name").GetString();
                    var type = InferJSDocType(input.GetProperty("value").GetString());
                    return $"@param {{{type}}} {name}";
                });

                // Determine return type
                var firstExpectedOutput = firstTestCase.GetProperty("expectedOutputs").EnumerateArray().FirstOrDefault().GetString() ?? "";
                var returnType = InferJSDocType(firstExpectedOutput);
                var returnString = $"@return {{{returnType}}}";

                var paramNames = string.Join(", ", inputs.Select(input => input.GetProperty("name").GetString()));

                if (string.IsNullOrEmpty(functionName)) return null;
                var camelFunctionName = char.ToLower(functionName[0]) + functionName.Substring(1);

                return
    $@"/**
  * {string.Join("\n * ", paramStrings)}
  * {returnString}
  */
 var {camelFunctionName} = ({paramNames}) => {{
     // Write your code here
 }};";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate JavaScript template for function '{FunctionName}'.", functionName);
                return null;
            }
        }

        private string InferJSDocType(string value)
        {
            if (string.IsNullOrEmpty(value)) return "any";

            value = value.Trim();

            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                string innerJson = value.Substring(1, value.Length - 2).Trim();
                if (string.IsNullOrEmpty(innerJson)) return "any[]"; // Empty array

                string firstElement = innerJson.Split(',')[0].Trim();
                string innerType = InferJSDocType(firstElement);
                return $"{innerType}[]";
            }

            if (value.StartsWith("\"") && value.EndsWith("\"")) return "string";

            if (bool.TryParse(value, out _)) return "boolean";

            if (double.TryParse(value, out _)) return "number";

            return "number"; // Default to number for other scalar types
        }

        public string GenerateTestHarness(string userCode, string functionName, JsonElement testCase)
        {
            var inputs = testCase.GetProperty("inputs").EnumerateArray().ToList();
            var harnessBody = new StringBuilder();

            if (string.IsNullOrEmpty(functionName)) return null;
            var camelFunctionName = char.ToLower(functionName[0]) + functionName.Substring(1);

            // 1. Declare and initialize variables from test case inputs
            foreach (var input in inputs)
            {
                var name = input.GetProperty("name").GetString();
                // In JS, we can directly use the JSON value string
                var value = input.GetProperty("value").GetString();
                string jsValue;

                if (value.StartsWith("[") && value.EndsWith("]"))
                {
                    jsValue = value; // Use the raw JSON array string
                }
                else
                {
                    jsValue = $"\"{value}\""; // Wrap scalar values in quotes
                }
                harnessBody.AppendLine($"const {name} = {value};");
            }

            // 2. Call the solution function and print the result
            var paramNames = string.Join(", ", inputs.Select(p => p.GetProperty("name").GetString()));
            harnessBody.AppendLine($"const result = {camelFunctionName}({paramNames});");

            // 3. Use console.log and JSON.stringify for consistent output format
            harnessBody.AppendLine("console.log(JSON.stringify(result));");

            // 4. Combine user code with the harness
            return
    $@"{userCode}
 
 // --- Test Harness ---
 {harnessBody}";
        }
    }
}

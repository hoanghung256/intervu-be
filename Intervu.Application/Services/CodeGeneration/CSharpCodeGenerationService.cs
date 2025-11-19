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
    public class CSharpCodeGenerationService : ICodeGenerationService
    {
        private readonly ILogger<CSharpCodeGenerationService> _logger;
        public string Language => "csharp";

        public CSharpCodeGenerationService(ILogger<CSharpCodeGenerationService> logger)
        {
            _logger = logger;
        }

        public string GenerateTemplate(string functionName, object[] testCases)
        {
            try
            {
                if (testCases.Length == 0) return null;

                var firstTestCase = (JsonElement)testCases[0];
                var inputs = firstTestCase.GetProperty("inputs").EnumerateArray()
                    .Select(input => new
                    {
                        Name = input.GetProperty("name").GetString(),
                        Type = InferCSharpType(input.GetProperty("value").GetString())
                    }).ToList();

                if (string.IsNullOrEmpty(functionName)) return null;
                var pascalFunctionName = char.ToUpper(functionName[0]) + functionName.Substring(1);

                var firstExpectedOutput = firstTestCase.GetProperty("expectedOutputs").EnumerateArray().FirstOrDefault().GetString() ?? "";
                var returnType = InferCSharpType(firstExpectedOutput);
                var paramsString = string.Join(", ", inputs.Select(p => $"{p.Type} {p.Name}"));

                return
    $@"class Solution {{
     public {returnType} {pascalFunctionName}({paramsString}) {{
         // Write your code here
         
     }}
 }}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate C# template for function '{FunctionName}'.", functionName);
                return null;
            }
        }

        public string GenerateTestHarness(string userCode, string functionName, JsonElement testCase)
        {
            var inputs = testCase.GetProperty("inputs").EnumerateArray().ToList();
            var mainBody = new StringBuilder();

            if (string.IsNullOrEmpty(functionName)) return null;
            var pascalFunctionName = char.ToUpper(functionName[0]) + functionName.Substring(1);

            foreach (var input in inputs)
            {
                var name = input.GetProperty("name").GetString();
                var value = input.GetProperty("value").GetString();
                var type = InferCSharpType(value);

                if (type.EndsWith("[]"))
                {
                    mainBody.AppendLine($"        var {name} = new {type} {{{value.Trim('[', ']')}}};");
                }
                else if (type == "string")
                {
                    mainBody.AppendLine($"        var {name} = \"{value.Trim('"').Replace("\"", "\"\"")}\";");
                }
                else
                {
                    mainBody.AppendLine($"        var {name} = {value};");
                }
            }

            var paramNames = string.Join(", ", inputs.Select(p => p.GetProperty("name").GetString()));
            mainBody.AppendLine("        var sol = new Solution();");
            mainBody.AppendLine($"        var result = sol.{pascalFunctionName}({paramNames});");
            mainBody.AppendLine("        Console.WriteLine(Serialize(result));");

            return
    $@"using System;
 using System.Collections.Generic;
 using System.Linq;
 
 {userCode}
 
 class Program
 {{
     // Helper function to serialize results to a JSON-like string without external dependencies
     public static string Serialize(object obj)
     {{
         if (obj == null) return ""null"";
         if (obj is bool b) return b.ToString().ToLower();
         if (obj is string s) return $""\""{{s}}\"""";
         if (obj.GetType().IsPrimitive || obj is decimal) return obj.ToString();
 
         if (obj is System.Collections.IEnumerable enumerable)
         {{
             var items = enumerable.Cast<object>().Select(Serialize);
             return $""[{{string.Join("","", items)}}]"";
         }}
 
         return obj.ToString();
     }}
     public static void Main(string[] args)
     {{
 {mainBody}
     }}
 }}";
        }

        private string InferCSharpType(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "null") return "object";
            value = value.Trim();
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                return InferArrayType(value);
            }
            if (value.StartsWith("\"") && value.EndsWith("\"")) return "string";
            if (bool.TryParse(value, out _)) return "bool";
            if (long.TryParse(value, out _)) return "int";
            if (double.TryParse(value, out _)) return "double";
            return "object";
        }

        private string InferArrayType(string jsonArray)
        {
            string inner = jsonArray.Substring(1, jsonArray.Length - 2).Trim();

            if (string.IsNullOrEmpty(inner))
                return "int[]"; // Default for empty array

            if (inner.StartsWith("["))
            {
                string innerType = InferCSharpType(inner.Split(',')[0].Trim() + "]"); // Check first element of outer array
                return innerType + "[]";
            }

            var parts = inner.Split(',').Select(p => p.Trim()).ToList();

            if (parts.Any(p => p.Contains(".")))
                return "double[]";

            if (parts.All(p => long.TryParse(p, out _)))
                return "int[]";

            return "object[]";
        }
    }
}

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
    public class JavaCodeGenerationService : ICodeGenerationService
    {
        private readonly ILogger<JavaCodeGenerationService> _logger;
        public string Language => "java";

        public JavaCodeGenerationService(ILogger<JavaCodeGenerationService> logger)
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
                        Type = InferJavaType(input.GetProperty("value").GetString())
                    }).ToList();

                if (string.IsNullOrEmpty(functionName)) return null;
                var camelFunctionName = char.ToLower(functionName[0]) + functionName.Substring(1);

                var firstExpectedOutput = firstTestCase.GetProperty("expectedOutputs").EnumerateArray().FirstOrDefault().GetString() ?? "";
                var returnType = InferJavaType(firstExpectedOutput);
                var paramsString = string.Join(", ", inputs.Select(p => $"{p.Type} {p.Name}"));

                return
$@"class Solution {{
    public {returnType} {camelFunctionName}({paramsString}) {{
        // Write your code here
        
    }}
}}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Java template for function '{FunctionName}'.", functionName);
                return null;
            }
        }

        public string GenerateTestHarness(string userCode, string functionName, JsonElement testCase)
        {
            var inputs = testCase.GetProperty("inputs").EnumerateArray().ToList();
            var mainBody = new StringBuilder();

            if (string.IsNullOrEmpty(functionName)) return null;
            var camelFunctionName = char.ToLower(functionName[0]) + functionName.Substring(1);

            foreach (var input in inputs)
            {
                var name = input.GetProperty("name").GetString();
                var value = input.GetProperty("value").GetString();
                var type = InferJavaType(value);

                // handle arrays
                if (type.EndsWith("[]"))
                {
                    string javaValue = ConvertJsonToJavaArray(value, type);
                    mainBody.AppendLine($"        {type} {name} = {javaValue};");
                }
                else
                {
                    // scalar
                    string javaValue = ConvertJsonToJavaScalar(value, type);
                    mainBody.AppendLine($"        {type} {name} = {javaValue};");
                }
            }

            var paramNames = string.Join(", ", inputs.Select(p => p.GetProperty("name").GetString()));
            var firstExpectedOutput = testCase.GetProperty("expectedOutputs").EnumerateArray().FirstOrDefault().GetString() ?? "";
            var returnType = InferJavaType(firstExpectedOutput);

            mainBody.AppendLine("        Solution sol = new Solution();");
            mainBody.AppendLine($"        {returnType} result = sol.{camelFunctionName}({paramNames});");

            if (returnType.EndsWith("[][]"))
                mainBody.AppendLine("        System.out.println(java.util.Arrays.deepToString(result));");
            else if (returnType.EndsWith("[]"))
                mainBody.AppendLine("        System.out.println(java.util.Arrays.toString(result));");
            else
                mainBody.AppendLine("        System.out.println(result);");

            return
        $@"import java.util.*;
           import java.io.*;
           import java.math.*;

{userCode}

class Main {{
    public static void main(String[] args) {{
{mainBody}
    }}
}}";
        }

        private string ConvertJsonToJavaScalar(string value, string type)
        {
            value = value.Trim();

            if (type == "String")
                return "\"" + value.Trim('"') + "\"";

            if (type == "char")
                return "'" + value.Trim('\'', '"') + "'";

            return value; // int, long, double, boolean
        }

        private string ConvertJsonToJavaArray(string value, string type)
        {
            // Convert JSON array syntax [a,b,c] → {a,b,c}
            string inner = value.Trim();
            inner = inner.TrimStart('[').TrimEnd(']');

            // multi-dimensional array
            if (type.EndsWith("[][]"))
            {
                // [[1,2],[3,4]] → new int[][]{{1,2},{3,4}}
                string converted = value
                    .Replace("[", "{")
                    .Replace("]", "}");

                return $"new {type}{converted}";
            }

            // 1D arrays: int[], double[], boolean[], String[], char[]
            if (type == "String[]")
            {
                // escape quotes if needed
                return $"new String[]{{{inner}}}";
            }

            if (type == "char[]")
            {
                return $"new char[]{{{inner}}}";
            }

            if (type == "boolean[]" || type == "double[]" || type == "int[]")
            {
                return $"new {type}{{{inner}}}";
            }

            return $"new Object[]{{{inner}}}";
        }


        private string InferJavaType(string value)
        {
            if (value is null) return "Object";

            value = value.Trim();

            // null or empty
            if (value == "" || value == "null") return "Object";

            // ----------------------------
            // ARRAY HANDLING
            // ----------------------------
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                return InferArrayType(value);
            }

            // ----------------------------
            // SCALAR HANDLING
            // ----------------------------
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return "String";

            if (value.Length == 3 && value.StartsWith("'") && value.EndsWith("'"))
                return "char";

            if (bool.TryParse(value, out _))
                return "boolean";

            if (long.TryParse(value, out _))
                return "int";   // LeetCode uses int for scalars unless value exceeds range

            if (double.TryParse(value, out _))
                return "double";

            return "Object";
        }

        private string InferArrayType(string jsonArray)
        {
            string inner = jsonArray.Substring(1, jsonArray.Length - 2).Trim();

            if (inner.Length == 0)
                return "int[]";

            // multi-dimensional array
            if (inner.StartsWith("["))
            {
                string innerType = InferJavaType(inner);
                return innerType + "[]";
            }

            // split shallow
            var parts = inner.Split(',')
                             .Select(p => p.Trim())
                             .ToList();

            // ------------ Char detection (strict) ----------------
            // Must be exactly quoted single-character strings
            bool isStrictChar = parts.All(p =>
                // "a"
                (p.StartsWith("\"") && p.EndsWith("\"") && p.Length == 3)
                ||
                // 'a'
                (p.StartsWith("'") && p.EndsWith("'") && p.Length == 3)
            );

            if (isStrictChar) return "char[]";

            // ------------ String detection -----------------------
            bool isString = parts.Any(p => p.StartsWith("\"") && p.EndsWith("\""));
            if (isString) return "String[]";

            // ------------ Boolean detection ----------------------
            bool isBoolean = parts.All(p => bool.TryParse(p, out _));
            if (isBoolean) return "boolean[]";

            // ------------ Numeric detection ----------------------
            bool allLong = parts.All(p => long.TryParse(p, out _));
            bool allDouble = parts.All(p => double.TryParse(p, out _));

            if (allDouble && !allLong)
                return "double[]";

            if (allLong)
                return "int[]";   // LeetCode always uses int[]

            return "Object[]";
        }
    }
}

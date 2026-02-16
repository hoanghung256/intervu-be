using Intervu.API.Test.Reporting;
using System;
using TechTalk.SpecFlow.xUnit.SpecFlowPlugin;
using Xunit;

// This attribute tells xUnit to create a single instance of ReportFixture
// and share it across all tests in the assembly.
[assembly: AssemblyFixture(typeof(ReportFixture))]

namespace Intervu.API.Test.Reporting
{
    /// <summary>
    /// An xUnit Assembly Fixture to manage the lifecycle of the Extent Report.
    /// It ensures that the report is flushed (saved) once all tests in the assembly have completed.
    /// </summary>
    public class ReportFixture : IDisposable
    {
        public void Dispose()
        {
            // This code runs once after all tests in the assembly are finished.
            ExtentService.Instance.Flush();
        }
    }
}
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace Intervu.API.Test.Reporting
{
    public class ExtentService
    {
        private static ExtentReports _extent;
        private static readonly object _lock = new object();

        public static ExtentReports Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_extent == null)
                    {
                        var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "ExtentReport.html");
                        var htmlReporter = new ExtentSparkReporter(reportPath);
                        _extent = new ExtentReports();
                        _extent.AttachReporter(htmlReporter);
                    }
                    return _extent;
                }
            }
        }
    }
}

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
                        var reportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
                        if (!Directory.Exists(reportDirectory))
                        {
                            Directory.CreateDirectory(reportDirectory);
                        }

                        var reportPath = Path.Combine(reportDirectory, "ExtentReport.html");
                        var htmlReporter = new ExtentSparkReporter(reportPath);
                        _extent = new ExtentReports();
                        _extent.AttachReporter(htmlReporter);

                        // Ensure flush on process exit as a safety measure
                        AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                            lock(_lock) {
                                _extent?.Flush();
                            }
                        };
                    }
                    return _extent;
                }
            }
        }

        public void Flush()
        {
            lock (_lock)
            {
                _extent?.Flush();
            }
        }
    }
}

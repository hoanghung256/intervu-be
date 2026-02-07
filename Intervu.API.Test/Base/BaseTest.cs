using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Model;
using Intervu.API.Test.Reporting;
using Xunit;
using Xunit.Abstractions;

namespace Intervu.API.Test.Base
{
    public class BaseTest : IAsyncLifetime
    {
        protected ExtentTest _test;
        protected readonly ITestOutputHelper _output;
        private readonly Stopwatch _stopwatch;
        
        // Allows static helpers to access the current running test instance
        public static AsyncLocal<BaseTest> Current { get; } = new();
        
        // Default timeout for steps to detect infinite loops
        protected virtual TimeSpan StepTimeout => TimeSpan.FromSeconds(60);

        // Delegate to capture screenshot (to be set by derived classes like BaseAutomationTest)
        public Func<Task<byte[]>>? ScreenshotProvider { get; set; }

        public BaseTest(ITestOutputHelper output)
        {
            Current.Value = this;
            _output = output;
            _stopwatch = new Stopwatch();

            string testName = this.GetType().Name;
            try
            {
                var type = output.GetType();
                var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
                if (testMember != null)
                {
                    var test = (ITest)testMember.GetValue(output);
                    testName = test.TestCase.TestMethod.Method.Name;
                }
            }
            catch { }

            _test = ExtentService.Instance.CreateTest(testName);
            try
            {
                var type = output.GetType();
                var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
                if (testMember != null)
                {
                    var test = (ITest)testMember.GetValue(output)!;
                    if (test.TestCase.Traits.TryGetValue("Category", out var categories))
                    {
                        foreach (var category in categories)
                        {
                            _test.AssignCategory(category);
                        }
                    }
                    
                    if (test.TestCase.Traits.TryGetValue("Name", out var names))
                    {
                        foreach (var name in names)
                        {
                            _test.AssignAuthor(name);
                        }
                    }
                }
            }
            catch { }
        }

        public void LogInfo(string message)
        {
            _test.Log(Status.Info, message);
            _output.WriteLine($"[INFO] {message}");
        }

        public async Task LogPass(string message)
        {
            _test.Log(Status.Pass, message);
            _output.WriteLine($"[PASS] {message}");
            await Task.CompletedTask;
        }

        public async Task LogFail(string message, Exception ex = null)
        {
            Media? mediaModel = null;
            if (ScreenshotProvider != null)
            {
                try
                {
                    // Asynchronously wait for the screenshot task to complete
                    var bytes = await ScreenshotProvider();
                    var base64 = Convert.ToBase64String(bytes);
                    mediaModel = MediaEntityBuilder.CreateScreenCaptureFromBase64String(base64).Build();
                }
                catch (Exception sEx)
                {
                    _output.WriteLine($"[WARN] Failed to capture screenshot: {sEx.Message}");
                }
            }

            if (ex != null)
            {
                _test.Log(Status.Fail, $"{message} - {ex.Message}", mediaModel);
                //_test.Log(Status.Fail, ex);
                _output.WriteLine($"[FAIL] {message} - {ex.Message}");
            }
            else
            {
                _test.Log(Status.Fail, message, mediaModel);
                _output.WriteLine($"[FAIL] {message}");
            }
        }
        
        protected virtual async Task OnTimeout(string stepName)
        {
            var msg = $"[TIMEOUT] Step '{stepName}' exceeded the time limit of {StepTimeout.TotalSeconds} seconds. Possible infinite loop detected.";
            await LogFail(msg);
            // You could add additional logic here, like sending an alert or capturing thread dumps if possible.
        }

        // Original methods
        public void LogStep(string stepName, Action action)
        {
            ExecuteStep(stepName, action);
        }

        public T LogStep<T>(string stepName, Func<T> action)
        {
            return ExecuteStep(stepName, action);
        }

        public async Task LogStepAsync(string stepName, Func<Task> action)
        {
            await ExecuteStepAsync(stepName, action);
        }

        public async Task<T> LogStepAsync<T>(string stepName, Func<Task<T>> action)
        {
            return await ExecuteStepAsync(stepName, action);
        }

        // Overloads with CallerMemberName
        public void LogStep(Action action, [CallerMemberName] string stepName = "")
        {
            ExecuteStep(stepName, action);
        }

        public T LogStep<T>(Func<T> action, [CallerMemberName] string stepName = "")
        {
            return ExecuteStep(stepName, action);
        }

        public async Task LogStepAsync(Func<Task> action, [CallerMemberName] string stepName = "")
        {
            await ExecuteStepAsync(stepName, action);
        }

        public async Task<T> LogStepAsync<T>(Func<Task<T>> action, [CallerMemberName] string stepName = "")
        {
            return await ExecuteStepAsync(stepName, action);
        }

        // Private helpers to avoid duplication
        private void ExecuteStep(string stepName, Action action)
        {
            _stopwatch.Restart();
            try
            {
                // LogInfo($"Starting step: {stepName}");
                
                var task = Task.Run(action);
                if (!task.Wait(StepTimeout))
                {
                    OnTimeout(stepName).GetAwaiter().GetResult();
                    throw new TimeoutException($"Step {stepName} timed out.");
                }
                
                _stopwatch.Stop();
                LogPass($"Step passed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();
                if (ex is not TimeoutException) // Already logged in OnTimeout
                {
                    LogFail($"Step failed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms", ex).GetAwaiter().GetResult();
                }
                throw;
            }
        }

        private T ExecuteStep<T>(string stepName, Func<T> action)
        {
            _stopwatch.Restart();
            try
            {
                // LogInfo($"Starting step: {stepName}");
                
                var task = Task.Run(action);
                if (!task.Wait(StepTimeout))
                {
                    OnTimeout(stepName).GetAwaiter().GetResult();
                    throw new TimeoutException($"Step {stepName} timed out.");
                }
                
                var result = task.Result;
                _stopwatch.Stop();
                LogPass($"Step passed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms").GetAwaiter().GetResult();
                return result;
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();
                if (ex is not TimeoutException)
                {
                    LogFail($"Step failed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms", ex).GetAwaiter().GetResult();
                }
                throw;
            }
        }

        private async Task ExecuteStepAsync(string stepName, Func<Task> action)
        {
            _stopwatch.Restart();
            try
            {
                // LogInfo($"Starting step: {stepName}");
                
                var task = action();
                var timeoutTask = Task.Delay(StepTimeout);
                
                var completedTask = await Task.WhenAny(task, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    await OnTimeout(stepName);
                    throw new TimeoutException($"Step {stepName} timed out.");
                }
                
                await task; // Propagate exceptions if any
                
                _stopwatch.Stop();
                await LogPass($"Step passed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();
                if (ex is not TimeoutException)
                {
                    await LogFail($"Step failed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms", ex);
                }
                throw;
            }
        }

        private async Task<T> ExecuteStepAsync<T>(string stepName, Func<Task<T>> action)
        {
            _stopwatch.Restart();
            try
            {
                // LogInfo($"Starting step: {stepName}");
                
                var task = action();
                var timeoutTask = Task.Delay(StepTimeout);
                
                var completedTask = await Task.WhenAny(task, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    await OnTimeout(stepName);
                    throw new TimeoutException($"Step {stepName} timed out.");
                }
                
                var result = await task;
                
                _stopwatch.Stop();
                await LogPass($"Step passed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();
                if (ex is not TimeoutException)
                {
                    await LogFail($"Step failed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms", ex);
                }
                throw;
            }
        }

        public virtual Task InitializeAsync() => Task.CompletedTask;

        public virtual Task DisposeAsync() => Task.CompletedTask;
    }
}

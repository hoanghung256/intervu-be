using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Model;
using Intervu.API.Test.Reporting;
using Xunit;
using Xunit.Abstractions;

namespace Intervu.API.Test.Base
{
    public class BaseTest : IAsyncLifetime
    {
        protected ExtentTest? _test;
        protected readonly ITestOutputHelper _output;
        private readonly Stopwatch _stopwatch;

        public static AsyncLocal<BaseTest?> Current { get; } = new();
        protected virtual TimeSpan StepTimeout => TimeSpan.FromSeconds(60);
        public Func<Task<byte[]>>? ScreenshotProvider { get; set; }

        // Default credentials for seeded users
        protected const string DEFAULT_PASSWORD = "123";
        protected const string CANDIDATE_PASSWORD = "abc@12345";
        protected const string ADMIN_EMAIL = "admin@example.com";
        protected const string COACH_EMAIL = "coach@example.com";

        public BaseTest(ITestOutputHelper output)
        {
            _output = output;
            _stopwatch = new Stopwatch();
            Current.Value = this;

            string testName = this.GetType().Name;

            try
            {
                // Attempt to get the actual test method name from xUnit
                var type = output.GetType();
                var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic)
                                ?? type.GetField("_test", BindingFlags.Instance | BindingFlags.NonPublic);

                if (testMember != null)
                {
                    var test = testMember.GetValue(output) as ITest;
                    if (test?.TestCase?.TestMethod?.Method?.Name != null)
                    {
                        testName = test.TestCase.TestMethod.Method.Name;
                    }

                    _test = ExtentService.Instance.CreateTest(testName);

                    if (test?.TestCase?.Traits != null)
                    {
                        if (test.TestCase.Traits.TryGetValue("Category", out var categories))
                            foreach (var category in categories) _test.AssignCategory(category);

                        if (test.TestCase.Traits.TryGetValue("Name", out var names))
                            foreach (var name in names) _test.AssignAuthor(name);
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"[WARN] Reflection failed in BaseTest constructor: {ex.Message}");
            }
            finally
            {
                // Ensure _test is NEVER null even if reflection fails
                if (_test == null)
                {
                    _test = ExtentService.Instance.CreateTest(testName);
                }
            }

            // Subscribe to catch exceptions that escape the Fact method without needing a wrapper
            AppDomain.CurrentDomain.FirstChanceException += HandleFirstChanceException;
        }

        private void HandleFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
        {
            // Only log if this exception is happening in the current test's execution context
            if (Current.Value == this && _test != null)
            {
                // Filter out common "handled" exceptions to avoid noise
                if (e.Exception is TaskCanceledException || e.Exception is OperationCanceledException || e.Exception is SocketException || e.Exception.Message.Contains("Hangfire.PostgreSql.Scripts.Install")) return;

                // If the test hasn't already been marked as failed, this might be the crash reason
                if (_test.Model.Status != Status.Fail)
                {
                    // We don't mark as Fail yet because it might be caught by a try-catch in the test logic,
                    // but we log it as an Info/Warning so we don't lose the data if it becomes a "System Exception"
                    _output.WriteLine($"[DEBUG] Exception observed: {e.Exception.GetType().Name}");
                    LogFail($"System exception occurred: {e.Exception.GetType().Name} - {e.Exception.Message}", e.Exception);
                }
            }
        }

        public void LogInfo(string message)
        {
            _test?.Log(Status.Info, message);
            _output.WriteLine($"[INFO] {message}");
        }

        public async Task LogPass(string message)
        {
            _test?.Log(Status.Pass, message);
            _output.WriteLine($"[PASS] {message}");
            await Task.CompletedTask;
        }

        public async Task LogFail(string message, Exception? ex = null)
        {
            try
            {
                Media? mediaModel = null;
                if (ScreenshotProvider != null)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        var screenshotTask = ScreenshotProvider();
                        var completedTask = await Task.WhenAny(screenshotTask, Task.Delay(5000, cts.Token));

                        if (completedTask == screenshotTask)
                        {
                            var bytes = await screenshotTask;
                            mediaModel = MediaEntityBuilder.CreateScreenCaptureFromBase64String(Convert.ToBase64String(bytes)).Build();
                        }
                    }
                    catch { /* Ignore screenshot errors during failure */ }
                }

                if (ex != null)
                {
                    while (ex is AggregateException ae && ae.InnerException != null) ex = ae.InnerException;
                    var detailedMessage = $"{message}<br/><b>Exception:</b> {ex.Message}<br/>";
                        //+ "<b>StackTrace:</b><br/><pre>{ex.StackTrace}</pre>";
                    _test?.Log(Status.Fail, detailedMessage, mediaModel);
                    _output.WriteLine($"[FAIL] {message} - {ex.Message}");
                }
                else
                {
                    _test?.Log(Status.Fail, message, mediaModel);
                    _output.WriteLine($"[FAIL] {message}");
                }
            }
            catch (Exception fatalEx)
            {
                _output.WriteLine($"[CRITICAL] LogFail failed: {fatalEx.Message}");
            }
        }

        protected virtual async Task OnTimeout(string stepName)
        {
            await LogFail($"[TIMEOUT] Step '{stepName}' exceeded {StepTimeout.TotalSeconds}s.");
        }

        // --- LogStep Methods ---
        public void LogStep(string stepName, Action action) => ExecuteStep(stepName, action);
        public T LogStep<T>(string stepName, Func<T> action) => ExecuteStep(stepName, action);
        public async Task LogStepAsync(string stepName, Func<Task> action) => await ExecuteStepAsync(stepName, action);
        public async Task<T> LogStepAsync<T>(string stepName, Func<Task<T>> action) => await ExecuteStepAsync(stepName, action);

        public void LogStep(Action action, [CallerMemberName] string stepName = "") => ExecuteStep(stepName, action);
        public T LogStep<T>(Func<T> action, [CallerMemberName] string stepName = "") => ExecuteStep(stepName, action);
        public async Task LogStepAsync(Func<Task> action, [CallerMemberName] string stepName = "") => await ExecuteStepAsync(stepName, action);
        public async Task<T> LogStepAsync<T>(Func<Task<T>> action, [CallerMemberName] string stepName = "") => await ExecuteStepAsync(stepName, action);

        private void ExecuteStep(string stepName, Action action)
        {
            _stopwatch.Restart();
            try
            {
                var task = Task.Run(action);
                if (!task.Wait(StepTimeout)) throw new TimeoutException();
                _stopwatch.Stop();
                LogPass($"Step passed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();
                LogFail($"Step failed: {stepName}", ex).GetAwaiter().GetResult();
                throw;
            }
        }

        private T ExecuteStep<T>(string stepName, Func<T> action)
        {
            _stopwatch.Restart();
            try
            {
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
                    LogFail($"Step failed: {stepName}", ex).GetAwaiter().GetResult();
                throw;
            }
        }

        private async Task ExecuteStepAsync(string stepName, Func<Task> action)
        {
            _stopwatch.Restart();
            try
            {
                var task = action();
                if (await Task.WhenAny(task, Task.Delay(StepTimeout)) != task) throw new TimeoutException();
                await task;
                _stopwatch.Stop();
                await LogPass($"Step passed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();
                await LogFail($"Step failed: {stepName}", ex);
                throw;
            }
        }

        private async Task<T> ExecuteStepAsync<T>(string stepName, Func<Task<T>> action)
        {
            _stopwatch.Restart();
            try
            {
                var task = action();
                if (await Task.WhenAny(task, Task.Delay(StepTimeout)) != task) throw new TimeoutException();
                var result = await task;
                _stopwatch.Stop();
                await LogPass($"Step passed: {stepName}. Duration: {_stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();
                await LogFail($"Step failed: {stepName}", ex);
                throw;
            }
        }

        public virtual Task InitializeAsync() => Task.CompletedTask;
        public virtual async Task DisposeAsync()
        {
            AppDomain.CurrentDomain.FirstChanceException -= HandleFirstChanceException;
            await Task.CompletedTask;
        }
    }
}

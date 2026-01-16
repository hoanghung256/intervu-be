using AventStack.ExtentReports;
using FluentAssertions.Execution;
using Intervu.API.Test.Base;
using Intervu.API.Test.Controls;
using Microsoft.Playwright;
using Xunit;

namespace Intervu.API.Test.Utils
{
    public static class AssertHelper
    {
        /*/// <summary>
        /// Executes a block of assertions. If any fail, execution continues.
        /// All failures are reported at the end of the block.
        /// Note: Only works with FluentAssertions (e.g., .Should().Be()).
        /// </summary>
        /// <param name="assertions">The action containing assertions.</param>
        public static void SoftAssert(Action assertions)
        {
            using (new AssertionScope())
            {
                assertions();
            }
        }

        /// <summary>
        /// Executes a block of async assertions softly.
        /// </summary>
        public static async Task SoftAssertAsync(Func<Task> assertions)
        {
            using (new AssertionScope())
            {
                await assertions();
            }
        }

        /// <summary>
        /// Executes assertions normally (Hard Assert). 
        /// Stops at the first failure.
        /// </summary>
        public static void HardAssert(Action assertions)
        {
            assertions();
        }
        
        /// <summary>
        /// Executes a block of async assertions normally (Hard Assert).
        /// </summary>
        public static async Task HardAssertAsync(Func<Task> assertions)
        {
            await assertions();
        }*/

        // --- Custom Assertion Extensions for BaseTest ---

        public static async Task AssertTrue(bool condition, string userMessage)
        {
            var test = BaseTest.Current.Value;
            try
            {
                Assert.True(condition);
                if (test != null) await test.LogPass($"Assertion passed: {userMessage}");
            }
            catch (Exception ex)
            {
                if (test != null) await test.LogFail($"Assertion failed: {userMessage}", ex);
                throw;
            }
        }

        public static async Task AssertFalse(bool condition, string userMessage)
        {
            var test = BaseTest.Current.Value;
            try
            {
                Assert.False(condition);
                if (test != null) await test.LogPass($"Assertion passed: {userMessage}");
            }
            catch (Exception ex)
            {
                if (test != null) await test.LogFail($"Assertion failed: {userMessage}", ex);
                throw;
            }
        }

        public static async Task AssertNull(object? @object, string userMessage)
        {
            var test = BaseTest.Current.Value;
            try
            {
                Assert.Null(@object);
                if (test != null) await test.LogPass($"Assertion passed: {userMessage}");
            }
            catch (Exception ex)
            {
                if (test != null) await test.LogFail($"Assertion failed: {userMessage}", ex);
                throw;
            }
        }

        public static async Task AssertNotNull(object? @object, string userMessage)
        {
            var test = BaseTest.Current.Value;
            try
            {
                Assert.NotNull(@object);
                if (test != null) await test.LogPass($"Assertion passed: {userMessage}");
            }
            catch (Exception ex)
            {
                if (test != null) await test.LogFail($"Assertion failed: {userMessage}", ex);
                throw;
            }
        }
        
        public static async Task AssertNotEmpty(IEnumerable<object> @object, string userMessage)
        {
            var test = BaseTest.Current.Value;
            try
            {
                Assert.NotEmpty(@object);
                if (test != null) await test.LogPass($"Assertion passed: {userMessage}");
            }
            catch (Exception ex)
            {
                if (test != null) await test.LogFail($"Assertion failed: {userMessage}", ex);
                throw;
            }
        }

        public static async Task AssertContains(string expectedSubstring, string actualString, string userMessage, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            var test = BaseTest.Current.Value;
            try
            {
                Assert.Contains(expectedSubstring, actualString, comparisonType);
                if (test != null) await test.LogPass($"Assertion passed: {userMessage}");
            }
            catch (Exception ex)
            {
                if (test != null) await test.LogFail($"Assertion failed: {userMessage}", ex);
                throw;
            }
        }

        public static async Task AssertEqual<T>(T expected, T actual, string userMessage)
        {
            var test = BaseTest.Current.Value;
            try
            {
                Assert.Equal(expected, actual);
                if (test != null) await test.LogPass($"Assertion passed: {userMessage}");
            }
            catch (Exception ex)
            {
                if (test != null) await test.LogFail($"Assertion failed: {userMessage}", ex);
                throw;
            }
        }

        public static async Task AssertToastMessageAsync(Toast toast, string expectedMessage, string userMessage)
        {
            var test = BaseTest.Current.Value;
            try
            {
                await toast.WaitForMessageAsync(expectedMessage);
                if (test != null) await test.LogPass($"Assertion passed: {userMessage}");
            }
            catch (Exception ex)
            {
                if (test != null) await test.LogFail($"Assertion failed: {userMessage}", ex);
                throw;
            }
        }
    }
}
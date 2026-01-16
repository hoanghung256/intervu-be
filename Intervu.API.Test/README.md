# Intervu.API.Test

This project contains the test suite for the Intervu API, including Unit Tests, Smoke Tests, and E2E Frontend Automation Tests.

## Project Structure

- **Base**: Contains the `BaseTest` class which provides common functionality like logging, reporting, and timeout handling.
- **Controllers**: Contains unit tests for API controllers.
- **UnitTests**: Contains unit tests for services and business logic.
- **SmokeTests**: Contains smoke tests to verify the basic health and connectivity of the API.
- **E2E**: Contains End-to-End automation tests using Playwright for the frontend.
- **Reporting**: Contains the ExtentReports configuration for generating HTML test reports.
- **Utils**: Contains utility classes for testing.

## Running Tests

### Prerequisites

- .NET 8.0 SDK
- Docker (for running dependencies if needed)

### Command Line Execution

Use `dotnet test` to execute the test suite. You can filter tests by their namespace (folder structure) or name to run specific subsets.

#### 1. Run All Tests

```bash
dotnet test
```

To run specific tests (e.g., Automation Tests):

```bash
dotnet test --filter "Category=Automation"
```

### CI/CD Integration

This project is designed to run in a CI/CD pipeline (e.g., GitHub Actions). The tests generate an HTML report in the `TestResults` directory.

## Test Framework Features

- **ExtentReports**: Generates detailed HTML reports with logs for each step.
- **Timeout Handling**: Each test step has a built-in timeout (default 30s) to prevent infinite loops.
- **FluentAssertions**: Used for more readable assertions.
- **Moq**: Used for mocking dependencies in unit tests.
- **Playwright**: Used for frontend automation.

## Adding New Tests

1. Inherit from `BaseTest`.
2. Use `LogStep` or `LogStepAsync` to wrap your test steps. This ensures they are logged and timed.
3. Use `_output` (ITestOutputHelper) for console logging if needed.

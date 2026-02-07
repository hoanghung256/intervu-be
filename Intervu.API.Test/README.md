# Intervu.API.Test

This project contains the test suite for the Intervu API, including Unit Tests, Smoke Tests, and E2E Frontend Automation Tests.

## Project Structure

- **Base**: Contains the `BaseTest` class which provides common functionality like logging, reporting, and timeout handling.
- **UnitTests**: Contains unit tests for services and business logic.
- **E2E**: Contains End-to-End automation tests using Playwright for the frontend.
- **Reporting**: Contains the ExtentReports configuration for generating HTML test reports.
- **Utils**: Contains utility classes for testing.

## Running Tests

### Prerequisites

- .NET 8.0 SDK

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
- **Timeout Handling**: Each test step has a built-in timeout (default 60s) to prevent infinite loops.
- **Playwright**: Used for frontend automation.

## Writing Tests Guide

### 1. Naming Conventions

To maintain consistency, please adhere to the following naming conventions:

| Item | Convention | Example |
|------|------------|---------|
| **Test Classes** | `[Feature]Tests` | `SkillsControllerTests`, `LoginTests` |
| **API Test Methods** | `[Method]_[Scenario]` | `GetAllSkills_ReturnsSuccessAndData` |
| **E2E Test Methods** | `Should_[Behavior]_[Condition]` | `Should_Display_Error_Message_For_Invalid_Credentials` |
| **Page Actions** | `[Verb][Noun]Async` | `NavigateAsync`, `LoginAsync`, `ClickSubmitAsync` |
| **Page Verifications** | `Verify[Element/State]Async` | `VerifyToastMessageAsync`, `IsDashboardVisibleAsync` |

### 2. API Tests (Integration)

Located in the `ApiTests` folder. These tests verify the backend endpoints directly.

#### How to Implement a New API Test

1.  **Class Definition**: Inherit from `BaseTest` and implement `IClassFixture<BaseApiTest<Program>>`.
2.  **Constructor**: Initialize `ApiHelper` with `factory.CreateClient()`.
3.  **Test Method**:
    *   Add `[Fact]` and `[Trait("Category", "API")]`.
    *   Use `LogInfo` to document steps.
    *   Use `_api` methods (`GetAsync`, `PostAsync`, etc.) for requests.
    *   Use `LogDeserializeJson<T>` to parse responses.
4.  **Verification**: Use `AssertHelper` to validate data.

#### Example (`SkillsControllerTest.cs`)

```csharp
public class SkillsControllerTest : BaseTest, IClassFixture<BaseApiTest<Program>>
{
    private readonly ApiHelper _api;

    public SkillsControllerTest(BaseApiTest<Program> factory, ITestOutputHelper output) : base(output)
    {
        _api = new ApiHelper(factory.CreateClient());
    }

    [Fact]
    [Trait("Category", "API")]
    public async Task GetAllSkills_ReturnsSuccessAndData()
    {            
        LogInfo("Getting all skills.");
        var response = await _api.GetAsync("/api/v1/skills?page=1&pageSize=10");
        
        LogInfo("Verify response.");
        var result = await _api.LogDeserializeJson<PagedResult<SkillDto>>(response);
        
        // Verification
        await AssertHelper.AssertNotNull(result.Data?.Items, "Data items are not null");
        await AssertHelper.AssertNotEmpty(result.Data?.Items!, "Data items are not empty");
    }
}
```

### 2. E2E Automation Tests (Frontend)

Located in the `E2ETests` folder. These tests use the **Page Object Model** pattern and inherit from `BaseAutomationTest`.

**Pattern:**
1. Inherit from `BaseAutomationTest`.
2. Instantiate Page Objects inherit from `BasePage` and pass `Page` as an argument to the constructor.
3. Use `LogInfo` to describe user actions.

#### **Example Page (`LoginPage.cs`)**

```csharp
public class LoginPage : BasePage
{
    //Controls for elements on the page
    private readonly TextField _emailInput;
    private readonly TextField _passwordInput;
    private readonly Button _loginButton;

    public LoginPage(IPage page) : base(page)
    {
        _emailInput = new TextField(Page, "input[name='email']");
        _passwordInput = new TextField(Page, "input[name='password']");
        _loginButton = new Button(Page, "button[type='submit']");
    }

    // Action: Naming convention [Verb][Noun]Async
    public async Task LoginAsync(string email, string password)
    {
        await _emailInput.FillAsync(email);
        await _passwordInput.FillAsync(password);
        await _loginButton.ClickAsync();
    }

    // Verification: Naming convention Verify[State]Async
    public Task VerifyToastMessageAsync(string message)
    {
        return AssertHelper.AssertToastMessageAsync(_toastMessage, message, "Toast message is displayed with correct text.");
    }
}
```

#### How to Implement a New E2E Test Case +

1.  **Class Definition**: 
    * Inherit from `BaseAutomationTest`. 
2.  **Test Method**:
    * Add `[Fact]` and `[Trait("Category", "Automation")]`.
    * Instantiate Page Objects passing Page. 
3.  **Flow**:
    * Arrange: Setup data or page objects.
    * Act: Call Action methods on Page Objects.
    * Assert: Call Verification methods on Page Objects or use `AssertHelper` for URL/Data checks.

#### **Example Automation Test (`LoginTests.cs`)**

```csharp
public class LoginTests : BaseAutomationTest
{
    public LoginTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "Automation")]
    public async Task Should_Login_Successfully()
    {
        
        // Setup: Create a Page Object instance
        var loginPage = new LoginPage(Page);
        
        // Action: Perform actions using the Page Object
        LogInfo("Navigating to login.");
        await loginPage.NavigateAsync();
        
        // Action: Login
        LogInfo("Performing login.");
        await loginPage.LoginAsync("user@example.com", "password");

        // Verification: Verify the outcome
        LogInfo("Verifying that a success message is displayed.");
        await loginPage.VerifyToastMessageAsync("Successful");
        
        // Verification: Assert
        LogInfo("Verifying redirect.");
        await Page.WaitForURLAsync("**/home");
        await AssertHelper.AssertContains("/home", Page.Url, "Redirected to home");
    }
}
```

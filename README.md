# INTERVU - Guidelines & Conventions

Welcome to the Intervu project! This document provides general guidelines and conventions to ensure code quality and consistency throughout the development process.

## Commit Message Convention

To maintain a clean, readable, and easy-to-track project history, we will adhere to the **Conventional Commits** specification. This practice not only helps team members understand the nature of changes but also enables automated changelog generation and semantic versioning.

### Structure of a Commit Message

Each commit message consists of three main parts: a **Header**, an optional **Body**, and an optional **Footer**.

```
<type>(<scope>): <subject>
<BLANK LINE>
<body>
<BLANK LINE>
<footer>
```

---

### 1. Header (Required)

The header is the most important part and follows a strict format: `<type>(<scope>): <subject>`

#### **`<type>`**

This prefix describes the kind of change you've made. The types used in this project are:

| Type | Description |
| :--- | :--- |
| **feat** | A new feature for the user. |
| **fix** | A bug fix in the codebase. |
| **docs** | Changes that only affect documentation. |
| **style** | Changes that do not affect the meaning of the code (white-space, formatting, etc.). |
| **refactor**| A code change that neither fixes a bug nor adds a feature. |
| **perf** | A code change that improves performance. |
| **test** | Adding missing tests or correcting existing tests. |
| **build** | Changes that affect the build system or external dependencies (e.g., npm, nuget, gradle). |
| **cicd** | Changes to our CI/CD configuration files and scripts. |
| **chore** | Other changes that don't modify source or test files (e.g., updating `.gitignore`). |
| **revert** | Reverts a previous commit. |

#### **`<scope>` (Optional)**

The `scope` provides additional contextual information about the commit. It could be the name of a module, component, or feature.

* `feat(api): add endpoint to get user profile`
* `fix(auth): correct password validation logic`
* `docs(readme): update setup instructions`

#### **`<subject>`**

* Use the imperative, present tense: "add", "change", "fix" not "added", "changed", or "fixed".
* Don't capitalize the first letter.
* Don't add a dot (.) at the end.
* Keep it short and concise, preferably under 50 characters.

---

### 2. Body (Optional)

* Use the body to provide more detailed explanations for the change.
* Separate the body from the header with a blank line.
* Explain the **"what"** and **"why"** of the change, not the "how".

---

### 3. Footer (Optional)

* Use the footer to reference issues from an issue tracker (e.g., Jira, GitHub Issues).
* Use it to indicate Breaking Changes.
* Separate the footer from the body with a blank line.

**Breaking Change:** Any commit that introduces a breaking change must start the footer with `BREAKING CHANGE:`, followed by a detailed description.

### Examples

**A simple commit (header only):**
```
fix(api): correct user data serialization
```

**A commit for a new feature:**
```
feat: allow users to upload profile pictures
```

**A commit with a detailed explanation (body):**
```
perf(db): improve query performance for fetching products

The previous query was using a LEFT JOIN which was inefficient
for large datasets. This commit changes it to use a subquery
which significantly reduces the query time.
```

**A commit with an issue reference and a Breaking Change:**
```
feat(auth): switch to JWT for authentication

This replaces the old session-based authentication system. All API
endpoints now require a Bearer token in the Authorization header.

Closes #123

BREAKING CHANGE: The authentication method has been changed.
Clients must now obtain a JWT and send it with every request.
```

---

By following this convention, we can work more effectively and keep the project in a healthy state. Thank you for your cooperation!
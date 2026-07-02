# User Management API

This repository contains a simple, robust User Management API built with ASP.NET Core (.NET 10). It was developed as part of the "Building a Simple API with Copilot" project.

## Project Features & Grading Criteria

This project fulfills all the requirements of the grading scheme:

### 1. CRUD Endpoints
The API includes complete RESTful CRUD operations for managing users. The following endpoints are implemented:
* **GET** `/api/users` - Retrieves a list of all users.
* **POST** `/api/users` - Creates a new user.
* **PUT** `/api/users/{id}` - Updates an existing user's information.
* **DELETE** `/api/users/{id}` - Removes a user from the system.

### 2. Data Validation
The API ensures that only valid data is processed. The `POST` and `PUT` endpoints include validation logic to check that required fields (such as `Name` and `Email`) are not null or empty before allowing the creation or modification of a user record. If invalid data is submitted, the API returns a `400 Bad Request`.

### 3. Custom Middleware
The application implements custom **Authentication Middleware**. This middleware intercepts incoming HTTP requests and checks for a valid Authorization header (`Bearer techhive-secret-token`). If the token is missing or invalid, the middleware short-circuits the request and returns a `401 Unauthorized` response, ensuring the endpoints are secure.

### 4. Debugging with Microsoft Copilot
Microsoft Copilot was utilized extensively throughout the development process, particularly for debugging:
* **Resolving Version Conflicts:** When integrating Swagger/OpenAPI in .NET 10, I encountered `CS0234` and `CS1061` errors due to namespace changes and a known vulnerability (CVE-2026-49451) in older `Microsoft.OpenApi` packages. Copilot helped me understand the root cause of the version mismatch.
* **Refactoring Testing Strategy:** Copilot guided me through safely removing the conflicting packages and transitioning to using a `requests.http` file to test the API and the authentication middleware directly, bypassing the Swagger UI issues entirely.
* **Code Generation:** Copilot assisted in scaffolding the Minimal API endpoints and drafting the initial middleware logic, which I then refined.

### 5. GitHub Repository
The project is successfully deployed to this public GitHub repository for review.

---

## How to Run and Test

1. Clone this repository to your local machine.
2. Open the project folder in VS Code or your preferred IDE.
3. Run the application using the .NET CLI:
   ```bash
   dotnet run

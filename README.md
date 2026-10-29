Optimizations Implemented

Main Changes

Replaced EF entity responses with DTOs to avoid exposing sensitive user fields.
Implemented password hashing for new passwords using PasswordHasher<T> (with a salt and configurable computational cost), along with a gradual migration of existing SHA-256 hashes upon the user's next successful login.
Added input validation, username/email duplicate checks, and fixed password update logic.
Used AsNoTracking() for read operations, added CancellationToken support to asynchronous actions, and standardized HTTP responses (404, 409, 204).
Fixed the URL called by project-service when communicating with user-service: GET /api/user/user/{id}.
Replaced repeated HttpClient instantiation with a typed client registered through IHttpClientFactory, including a timeout limit.
Removed hardcoded SQL credentials from configuration files and added a root-level Compose configuration that uses an environment variable to provide the database password.
Aligned Entity Framework Core packages with version 8 to match net8.0 and removed identified unused dependencies.

Running with Docker Compose

Copy .env.example to .env.
Replace SA_PASSWORD with a strong password that meets SQL Server requirements.
From the repository root, run docker compose up --build.
The APIs are available at http://localhost:5218 (users) and http://localhost:5247 (projects).

Note:
Existing EF Core migrations have been preserved. If the database has not yet been created, apply the migrations using the appropriate EF Core commands before using the endpoints. Build and runtime verification could not be performed in this environment because the .NET SDK was unavailable.

Remaining Tasks Before Production Deployment

Add unique SQL indexes for UserName and Email to prevent duplicate registrations during concurrent requests.
Implement proper authentication and authorization (e.g., JWT), as the current login endpoint does not create a session or issue a token.
Restrict CORS to the list of explicitly authorized origins.
Add automated tests and establish a migration management strategy for application startup and deployment.

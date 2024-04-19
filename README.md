# CityInfo API

The CityInfo API provides access to cities and their points of interest.

## Requirements

- .NET 8 SDK
- SQLite
- dotnet-ef (Entity Framework Core tools for database migrations)
- HTTP REPL (httpsrepl) for testing the API

## Installation

1. Clone the repository.
2. Navigate to the project directory: `cd CityInfo.API`
3. Restore dependencies: `dotnet restore`

## Configuration

Modify the `appsettings.json` file to configure the database connection string and authentication settings.

## Database Migration
Run database migration to create the SQLite database: `dotnet ef database update`

## Running the API

1. Start the API: `dotnet run --launch-profile https`
2. Access the API documentation and test the endpoints using Swagger UI: https://localhost:7222/swagger

## Testing with HTTP REPL

1. Start the HTTP REPL: `httprepl https://localhost:7222/ --openapi https://localhost:7222/swagger/2.0/swagger.json`
2. Use commands like ls, cd, get, put, post, and delete to interact with the API endpoints.
# Movie Information Provider - Backend API

A simple, well-structured **.NET 10 REST API** application for searching movie information from external public movie APIs with built-in caching and statistics tracking.

---

## Table of Contents

- [Overview](#overview)
- [Main Goals](#main-goals)
- [Technologies](#technologies)
- [External APIs](#external-apis)
- [API Endpoints](#api-endpoints)
- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [API Documentation](#api-documentation)
- [Troubleshooting](#troubleshooting)

---

## Overview

Movie Information Provider is a REST API that aggregates movie data from multiple external sources:
- **OMDb API** (Open Movie Database)
- **TMDb API** (The Movie Database)

The API returns movie search results in a **unified response format**, regardless of which external provider is used.

---

## Main Goals

### 1?? Primary Goal: Fast Response Times

Provide **fast movie search responses** by implementing an efficient **caching strategy**.

- In-memory cache with configurable TTL (15 minutes default)
- Reduces external API calls
- Improves response times for repeated searches
- Cache keys based on API provider, movie title, and pagination parameters

### 2?? Secondary Goal: Analytics & Insights

Store **search statistics** for future analytical purposes.

- Track search queries, API providers, and result counts
- Record cache hit/miss ratios
- Monitor response times for performance analysis
- Provide autocomplete suggestions based on search history

---

## Technologies

| Technology | Purpose |
|------------|---------|
| **.NET 10** | Web API framework |
| **Entity Framework Core** | ORM for database operations |
| **Microsoft SQL Server** | Relational database (LocalDB for development) |
| **Swagger/OpenAPI** | API documentation and testing |
| **IMemoryCache** | In-memory caching for performance |
| **HttpClient** | External API communication |
| **Clean Architecture** | Separation of concerns (Application/Domain/Infrastructure) |

---

## External APIs
<img width="1250" height="304" alt="image" src="https://github.com/user-attachments/assets/b60db245-7517-43a6-b40b-c8c90daf09c5" />

### OMDb (Open Movie Database)

**Base URL:** `http://www.omdbapi.com`

**Endpoints used:**

1. **Search movies:**
   ```
   GET /?s={movieTitle}&type=movie&page={page}&apikey={apiKey}
   ```

2. **Get movie details:**
   ```
   GET /?i={imdbID}&apikey={apiKey}
   ```

**Pagination:** OMDb returns **10 items per page**.

**Director data:** Retrieved from the detail endpoint.

---

### TMDb (The Movie Database)

**Base URL:** `https://api.themoviedb.org/3`

**Endpoints used:**

1. **Search movies:**
   ```
   GET /search/movie?api_key={apiKey}&query={movieTitle}&include_adult=true&page={page}
   ```

2. **Get movie credits (directors):**
   ```
   GET /movie/{movieId}/credits?api_key={apiKey}
   ```

**Director data:** Extracted from the `crew` array where `job == "Director"`.

---

## API Endpoints

### 1. Search Movies

**Endpoint:**
```
GET /movies/{movieTitle}
```

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `api` | string | ? Yes | - | API provider: `omdb` or `tmdb` |
| `page` | integer | ? No | 1 | Page number (1-based) |
| `pageSize` | integer | ? No | 10 | Results per page |

**Example Request:**
```http
GET /movies/batman?api=omdb&page=1&pageSize=10
```
<img width="1235" height="1274" alt="image" src="https://github.com/user-attachments/assets/0e09333c-7891-4fde-9051-5a5b4ca88120" />

**Example Response:**
```json
{
  "movies": [
    {
      "title": "Batman Begins",
      "year": "2005",
      "director": ["Christopher Nolan"]
    },
    {
      "title": "The Dark Knight",
      "year": "2008",
      "director": ["Christopher Nolan"]
    }
  ],
  "currentPage": 1,
  "pageSize": 10,
  "totalResults": 150,
  "totalPages": 15
}
```

**Response Schema:**

```json
{
  "movies": [
    {
      "title": "string",
      "year": "string",
      "director": ["string"]
    }
  ],
  "currentPage": "integer",
  "pageSize": "integer",
  "totalResults": "integer",
  "totalPages": "integer"
}
```

---

### 2. Autocomplete Suggestions

**Endpoint:**
```
GET /movies/autocomplete
```
<img width="1236" height="1095" alt="image" src="https://github.com/user-attachments/assets/ff3af6be-01df-4da3-af4a-3e8e750ec466" />

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `query` | string | ? Yes | - | Search term prefix (min 1 char) |
| `limit` | integer | ? No | 10 | Max suggestions (1-50) |

**Example Request:**
```http
GET /movies/autocomplete?query=bat&limit=10
```

**Example Response:**
```json
[
  "batman",
  "batman begins",
  "battle royale",
  "batwoman"
]
```

**How it works:**
- Suggestions are based on **previous search history** stored in the database
- Results are ordered by **popularity** (search count)
- Case-insensitive prefix matching
- Cached for performance

---

## Features

### ? Core Features

- **Dual API Support** - OMDb and TMDb integration with unified response format
- **Caching** - In-memory cache with 15-minute TTL to reduce API calls
- **Pagination** - Configurable page size with total page calculation
- **Search Statistics** - Database storage for analytics and trending searches
- **Autocomplete** - Smart suggestions from search history, ordered by popularity
- **Type Safety** - `MovieApiProvider` enum validation prevents invalid API selection

### ??? Reliability Features

- **Global Error Handling** - Structured error responses with appropriate HTTP status codes
- **Request Logging** - HTTP request/response timing with log level based on status codes
- **Cancellation Token Support** - Graceful cancellation handling for all async operations
- **Validation** - Request validation with detailed error messages

### ??? Architecture Features

- **Clean Architecture** - Clear separation: Application › Domain › Infrastructure
- **Dependency Injection** - Full DI container setup with proper lifetimes
- **Repository Pattern** - Data access abstraction
- **Strategy Pattern** - `IMovieApiClient` for pluggable API providers

---

## Architecture

```
MovieInformationProvider/
+¦¦ Controllers/                      # API endpoints
-   L¦¦ MoviesController.cs           # Movie search and autocomplete
-
+¦¦ Application/                      # Business logic layer
-   +¦¦ Movies/
-   -   +¦¦ Services/
-   -   -   L¦¦ MovieService.cs       # Main orchestration service
-   -   +¦¦ DTOs/
-   -   -   +¦¦ MovieDto.cs           # Movie response model
-   -   -   +¦¦ MovieSearchRequestDto.cs
-   -   -   L¦¦ MovieSearchResponseDto.cs
-   -   +¦¦ Interfaces/
-   -   -   +¦¦ IMovieService.cs
-   -   -   L¦¦ IMovieApiClient.cs    # API client abstraction
-   -   L¦¦ Validators/
-   -       L¦¦ MovieSearchRequestValidator.cs
-   -
-   +¦¦ SearchStatistics/
-   -   +¦¦ Services/
-   -   -   L¦¦ SearchStatisticsService.cs
-   -   +¦¦ DTOs/
-   -   -   +¦¦ SaveSearchStatisticDto.cs
-   -   -   L¦¦ SearchSuggestionDto.cs
-   -   L¦¦ Interfaces/
-   -       L¦¦ ISearchStatisticsService.cs
-   -
-   L¦¦ Common/
-       +¦¦ Caching/
-       -   L¦¦ ICacheService.cs      # Cache abstraction
-       L¦¦ Exceptions/
-           L¦¦ ValidationException.cs
-
+¦¦ Domain/                           # Domain entities and enums
-   +¦¦ Entities/
-   -   L¦¦ SearchStatistic.cs        # EF Core entity
-   +¦¦ Enums/
-   -   L¦¦ MovieApiProvider.cs       # Omdb = 1, Tmdb = 2
-   L¦¦ Common/
-       +¦¦ BaseEntity.cs             # Base class with Id and CreatedAtUtc
-       L¦¦ ErrorResponse.cs          # Structured error model
-
+¦¦ Infrastructure/                   # External concerns
-   +¦¦ Apis/
-   -   +¦¦ Omdb/
-   -   -   +¦¦ OmdbMovieApiClient.cs
-   -   -   L¦¦ OmdbSearchResponseDto.cs
-   -   L¦¦ Tmdb/
-   -       +¦¦ TmdbMovieApiClient.cs
-   -       L¦¦ TmdbSearchResponseDto.cs
-   +¦¦ Caching/
-   -   L¦¦ MemoryCacheService.cs     # IMemoryCache wrapper
-   L¦¦ Data/
-       +¦¦ AppDbContext.cs           # EF Core context
-       L¦¦ Migrations/               # Database migrations
-
L¦¦ Middleware/                       # ASP.NET Core middleware
    +¦¦ ExceptionHandlingMiddleware.cs # Global error handler
    L¦¦ RequestLoggingMiddleware.cs    # Request/response logging
```

---

## Getting Started

### Prerequisites

- **.NET 10 SDK** or later
- **SQL Server** or **LocalDB**
- **OMDb API Key** ([Get it here](http://www.omdbapi.com/apikey.aspx))
- **TMDb API Key** ([Get it here](https://www.themoviedb.org/settings/api))

---

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/mzoli95/movie-search-demo.git
   cd movie-search-demo
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Configure API keys:**

   Edit `appsettings.json`:
   ```json
   {
     "ExternalApis": {
       "Omdb": {
         "BaseUrl": "http://www.omdbapi.com",
         "ApiKey": "YOUR_OMDB_API_KEY"
       },
       "Tmdb": {
         "BaseUrl": "https://api.themoviedb.org/3",
         "ApiKey": "YOUR_TMDB_API_KEY"
       }
     },
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MovieInfoDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Run database migrations:**
   ```bash
   dotnet ef database update
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

6. **Open Swagger UI:**
   ```
   https://localhost:44376/swagger
   ```

---

## Configuration

### Cache Settings

Modify cache TTL in `MovieService.cs`:

```csharp
private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);
```

### Database Connection

Update connection string in `appsettings.json` for production:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=MovieInfoDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

### CORS Policy

Modify CORS origins in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200") // Add your origins here
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

---

## Database Setup

### Entity Framework Core Migrations

**Create a new migration:**
```bash
dotnet ef migrations add MigrationName
```

**Update database:**
```bash
dotnet ef database update
```

**Remove last migration:**
```bash
dotnet ef migrations remove
```

### Database Schema

**SearchStatistics Table:**

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key (auto-increment) |
| Api | int | MovieApiProvider enum (1=Omdb, 2=Tmdb) |
| Query | nvarchar(max) | Search query string |
| ResultCount | int | Number of results returned |
| ServedFromCache | bit | Cache hit indicator |
| DurationMs | int | Response time in milliseconds |
| CreatedAtUtc | datetime2 | Timestamp (UTC) |

---

## API Documentation

### Swagger UI

Interactive API documentation available at:
```
https://localhost:44376/swagger
```

Features:
- Try out endpoints directly from browser
- View request/response schemas
- See example values
- Download OpenAPI specification

### Response Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 400 | Bad Request (invalid parameters) |
| 404 | Not Found |
| 500 | Internal Server Error |
| 502 | Bad Gateway (external API error) |

### Error Response Format

```json
{
  "message": "Error description",
  "statusCode": 400,
  "traceId": "00-abc123...",
  "validationErrors": {
    "Api": ["Invalid API. Allowed: omdb, tmdb"]
  },
  "stackTrace": "..." // Development only
}
```

---

## Troubleshooting

### Issue: OMDb/TMDb returns no results

**Solution:**
- Verify API keys are correct in `appsettings.json`
- Check external API rate limits
- Inspect logs for HTTP errors

---

### Issue: Database connection fails

**Solution:**
```bash
# Verify LocalDB is running
sqllocaldb info mssqllocaldb

# Start LocalDB instance
sqllocaldb start mssqllocaldb

# Run migrations again
dotnet ef database update
```

---

### Issue: CORS errors from frontend

**Solution:**
- Add frontend URL to CORS policy in `Program.cs`
- Ensure `app.UseCors("AngularClient")` is called before `app.MapControllers()`

---

### Issue: Cache not working

**Solution:**
- Check `IMemoryCache` is registered in DI container
- Verify `ICacheService` is singleton
- Ensure cache keys are consistent (check `CacheKeys` class)

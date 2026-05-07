# Movie Information Provider

A simple, well-structured .NET REST API application for searching movie information from external public movie APIs.

The application currently supports:

- OMDb API
- TMDb API

The API returns movie search results in a unified response format, regardless of which external provider is used.

---

## Main goal

The primary goal of the application is to provide fast movie search responses by using caching.
<img width="1232" height="970" alt="image" src="https://github.com/user-attachments/assets/af7bc417-d68a-4082-b7e3-3c4f09af02f5" />
<img width="1217" height="712" alt="image" src="https://github.com/user-attachments/assets/c5f4c3d5-3163-4a36-9c48-8770bfd164b4" />

The secondary goal is to store search statistics for future analytical purposes.
<img width="559" height="356" alt="image" src="https://github.com/user-attachments/assets/11ee69d2-d46d-4032-9f54-dec5d28b6d7f" />

---

## Technologies

- .NET 10 Web API
- Entity Framework Core
- Microsoft SQL Server
- Swagger
- IMemoryCache
- HttpClient
- Angular 21
- Minimal NGRX
- Tailwind CSS

---

## External APIs

### OMDb

Used endpoints:

```txt
http://www.omdbapi.com/?s={movieTitle}&apikey={apiKey}
http://www.omdbapi.com/?i={imdbID}&apikey={apiKey}
```

The search endpoint is used to find matching movies.
The details endpoint is used to retrieve additional information, especially the director.

### TMDb

Used endpoints:
```txt
https://api.themoviedb.org/3/search/movie?api_key={apiKey}&query={movieTitle}&include_adult=true
https://api.themoviedb.org/3/movie/{movieId}/credits?api_key={apiKey}
```
The search endpoint is used to find matching movies.
The credits endpoint is used to retrieve the director information.

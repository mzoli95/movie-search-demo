using Microsoft.EntityFrameworkCore;
using MovieInformationProvider.Application.Common.Caching;
using MovieInformationProvider.Application.Movies.Interfaces;
using MovieInformationProvider.Application.Movies.Services;
using MovieInformationProvider.Application.SearchStatistics.Interfaces;
using MovieInformationProvider.Application.SearchStatistics.Services;
using MovieInformationProvider.Infrastructure.Apis.Omdb;
using MovieInformationProvider.Infrastructure.Apis.Tmdb;
using MovieInformationProvider.Infrastructure.Caching;
using MovieInformationProvider.Infrastructure.Data;
using MovieInformationProvider.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ISearchStatisticService, SearchStatisticService>();

builder.Services.AddHttpClient<IMovieApiClient, OmdbMovieApiClient>();
builder.Services.AddHttpClient<IMovieApiClient, TmdbMovieApiClient>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AngularClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
using Microsoft.EntityFrameworkCore;
using MovieInformationProvider.Data;
using MovieInformationProvider.Services;
using MovieInformationProvider.Services.ApiClients;
using MovieInformationProvider.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ISearchStatisticService, SearchStatisticService>();

builder.Services.AddHttpClient<OmdbMovieApiClient>();
builder.Services.AddHttpClient<TmdbMovieApiClient>();

builder.Services.AddScoped<IMovieApiClient, OmdbMovieApiClient>();
builder.Services.AddScoped<IMovieApiClient, TmdbMovieApiClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
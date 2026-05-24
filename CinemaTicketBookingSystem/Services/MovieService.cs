using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CinemaTicketBookingSystem.Database.AppDbContextModels;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public class MovieService : IMovieService
{
    private readonly AppDbContext _dbContext;

    public MovieService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Get all movies (with an option to include deleted ones)
    public async Task<IEnumerable<MovieResponseDto>> GetAllMoviesAsync(bool includeDeleted = false)
    {
        IQueryable<Movie> query = _dbContext.Movies;
        if (!includeDeleted)
        {
            query = query.Where(m => !m.IsDeleted);
        }
        var movies = await query.ToListAsync();
        return movies.Select(MapToResponseDto);
    }


    // Get a single movie by ID
    public async Task<MovieResponseDto?> GetMovieByIdAsync(int id)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.MovieId == id);
        if (movie == null || movie.IsDeleted)
        {
            return null;
        }
        return MapToResponseDto(movie);
    }


    // Add a new movie
    public async Task<MovieResponseDto> CreateMovieAsync(CreateMovieDto dto)
    {
        var movie = new Movie
        {
            Title = dto.Title,
            DurationMinutes = dto.DurationMinutes,
            Genre = dto.Genre,
            IsDeleted = false
        };

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        return MapToResponseDto(movie);
    }


    // Update movie by id
    public async Task<MovieResponseDto?> UpdateMovieAsync(int id, UpdateMovieDto dto)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.MovieId == id);
        if (movie == null || movie.IsDeleted)
        {
            return null;
        }

        movie.Title = dto.Title;
        movie.DurationMinutes = dto.DurationMinutes;
        movie.Genre = dto.Genre;

        await _dbContext.SaveChangesAsync();

        return MapToResponseDto(movie);
    }


    // Delete movie by id (soft delete)
    public async Task<bool> DeleteMovieAsync(int id)
    {
        var movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.MovieId == id);
        if (movie == null || movie.IsDeleted)
        {
            return false;
        }

        movie.IsDeleted = true;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static MovieResponseDto MapToResponseDto(Movie movie)
    {
        return new MovieResponseDto
        {
            MovieId = movie.MovieId,
            Title = movie.Title,
            DurationMinutes = movie.DurationMinutes,
            Genre = movie.Genre,
            IsDeleted = movie.IsDeleted
        };
    }
}

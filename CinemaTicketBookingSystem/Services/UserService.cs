using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CinemaTicketBookingSystem.Database.AppDbContextModels;
using CinemaTicketBookingSystem.WebApi.DTOs;

namespace CinemaTicketBookingSystem.WebApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Get all users
    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _dbContext.Users
                    .Where(u => !u.IsDeleted)
                    .ToListAsync();

        return users.Select(MapToResponseDto);
    }


    // Get a single user by ID
    public async Task<UserResponseDto?> GetUserByIdAsync(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
        {
            return null;
        }
        return MapToResponseDto(user);
    }


    // Create user 
    public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
    {
        
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
            {
                throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            var phoneExists = await _dbContext.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber);
            if (phoneExists)
            {
                throw new InvalidOperationException($"A user with phone number '{dto.PhoneNumber}' already exists.");
            }
        }

        var user = new User
        {
            FullName = dto.FullName,
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
            PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return MapToResponseDto(user);
    }


    // Update user
    public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
        {
            return null;
        }

        
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != id);
            if (emailExists)
            {
                throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
            }
        }

        
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            var phoneExists = await _dbContext.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber && u.UserId != id);
            if (phoneExists)
            {
                throw new InvalidOperationException($"A user with phone number '{dto.PhoneNumber}' already exists.");
            }
        }

        user.FullName = dto.FullName;
        user.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber;

        await _dbContext.SaveChangesAsync();

        return MapToResponseDto(user);
    }


    // Patch user
    public async Task<UserResponseDto?> PatchUserAsync(int id, PatchUserDto dto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
        {
            return null;
        }

        
        if (dto.FullName != null)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
            {
                throw new InvalidOperationException("Full name cannot be empty.");
            }
            user.FullName = dto.FullName;
        }

        
        string? newEmail = dto.Email != null ? (string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email) : user.Email;
        string? newPhone = dto.PhoneNumber != null ? (string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber) : user.PhoneNumber;

        if (string.IsNullOrWhiteSpace(newEmail) && string.IsNullOrWhiteSpace(newPhone))
        {
            throw new InvalidOperationException("At least email or phone number must be provided.");
        }

       
        if (dto.Email != null && !string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != id);
            if (emailExists)
            {
                throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
            }
        }

        
        if (dto.PhoneNumber != null && !string.IsNullOrWhiteSpace(dto.PhoneNumber) && dto.PhoneNumber != user.PhoneNumber)
        {
            var phoneExists = await _dbContext.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber && u.UserId != id);
            if (phoneExists)
            {
                throw new InvalidOperationException($"A user with phone number '{dto.PhoneNumber}' already exists.");
            }
        }

        
        if (dto.Email != null)
        {
            user.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
        }
        if (dto.PhoneNumber != null)
        {
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber;
        }

        await _dbContext.SaveChangesAsync();

        return MapToResponseDto(user);
    }

    // Delete user
    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);

        if (user == null)
        {
            return false;
        }

        user.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }


    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }
}

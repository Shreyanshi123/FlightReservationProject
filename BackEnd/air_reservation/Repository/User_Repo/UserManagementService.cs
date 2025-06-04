﻿using air_reservation.Data_Access_Layer;
using air_reservation.Models.Reservation_Model_;
using air_reservation.Models.Users_Model_;
using Microsoft.EntityFrameworkCore;

namespace air_reservation.Repository.User_Repo
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ApplicationDbContext _context;

        public UserManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserListDTO>> GetUsersAsync(UserSearchDTO searchDto)
        {
            var query = _context.Users.AsQueryable();

            // Apply search filters
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                var searchTerm = searchDto.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm));
            }

            if (searchDto.Role.HasValue)
            {
                query = query.Where(u => u.Role == searchDto.Role.Value);
            }

            if (searchDto.CreatedAfter.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= searchDto.CreatedAfter.Value);
            }

            if (searchDto.CreatedBefore.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= searchDto.CreatedBefore.Value);
            }

            // Apply sorting
            query = searchDto.SortBy.ToLower() switch
            {
                "email" => searchDto.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "firstname" => searchDto.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                "lastname" => searchDto.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                "role" => searchDto.SortDescending ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
                _ => searchDto.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var users = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Include(u => u.Reservations)
                .ToListAsync();

            var userDtos = new List<UserListDTO>();

            foreach (var user in users)
            {
                var lastLogin = await _context.LoginLogs
                    .Where(l => l.Email == user.Email && l.IsSuccessful)
                    .OrderByDescending(l => l.LoginTime)
                    .Select(l => l.LoginTime)
                    .FirstOrDefaultAsync();

                userDtos.Add(new UserListDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    ReservationCount = user.Reservations.Count,
                    LastLoginDate = lastLogin == DateTime.MinValue ? null : lastLogin
                });
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

            return new PagedResult<UserListDTO>
            {
                Items = userDtos,
                TotalCount = totalCount,
                CurrentPage = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = totalPages,
                HasNextPage = searchDto.Page < totalPages,
                HasPreviousPage = searchDto.Page > 1
            };
        }

        public async Task<UserDetailsDTO> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Reservations)
                    .ThenInclude(r => r.Payment)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var lastLogin = await _context.LoginLogs
                .Where(l => l.Email == user.Email && l.IsSuccessful)
                .OrderByDescending(l => l.LoginTime)
                .Select(l => l.LoginTime)
                .FirstOrDefaultAsync();

            var totalSpent = user.Reservations
                .Where(r => r.Status == ReservationStatus.Confirmed)
                .Sum(r => r.TotalAmount);

            return new UserDetailsDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                TotalReservations = user.Reservations.Count,
                TotalSpent = totalSpent,
                LastLoginDate = lastLogin == DateTime.MinValue ? null : lastLogin
            };
        }



        public async Task<UserDetailsDTO> UpdateUserAsync(int userId, UpdateUserDTO updateUserDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            // Check if email is being changed and if new email already exists
            if (user.Email != updateUserDto.Email &&
                await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != userId))
            {
                throw new InvalidOperationException("Email already exists");
            }

            user.Email = updateUserDto.Email;
            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.PhoneNumber = updateUserDto.PhoneNumber;
            user.Role = updateUserDto.Role;

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Reservations)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            // Check if user has active reservations
            var hasActiveReservations = user.Reservations.Any(r =>
                r.Status == ReservationStatus.Confirmed ||
                r.Status == ReservationStatus.Pending);

            if (hasActiveReservations)
            {
                throw new InvalidOperationException("Cannot delete user with active reservations");
            }

            // Don't allow deletion of the last admin
            if (user.Role == UserRole.Admin)
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
                if (adminCount <= 1)
                {
                    throw new InvalidOperationException("Cannot delete the last admin user");
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangeUserPasswordAsync(int userId, ChangePasswordDTO changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserListDTO>> GetRecentUsersAsync(int count = 10)
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(count)
                .Include(u => u.Reservations)
                .ToListAsync();

            var userDtos = new List<UserListDTO>();

            foreach (var user in users)
            {
                var lastLogin = await _context.LoginLogs
                    .Where(l => l.Email == user.Email && l.IsSuccessful)
                    .OrderByDescending(l => l.LoginTime)
                    .Select(l => l.LoginTime)
                    .FirstOrDefaultAsync();

                userDtos.Add(new UserListDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    ReservationCount = user.Reservations.Count,
                    LastLoginDate = lastLogin == DateTime.MinValue ? null : lastLogin
                });
            }

            return userDtos;
        }

        public async Task<Dictionary<string, object>> GetUserStatisticsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
            var totalRegularUsers = await _context.Users.CountAsync(u => u.Role == UserRole.User);

            var usersThisMonth = await _context.Users
                .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1));

            var usersThisWeek = await _context.Users
                .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7));

            var activeUsers = await _context.LoginLogs
                .Where(l => l.IsSuccessful && l.LoginTime >= DateTime.UtcNow.AddDays(-30))
                .Select(l => l.Email)
                .Distinct()
                .CountAsync();

            var topSpenders = await _context.Users
                .Include(u => u.Reservations)
                .ThenInclude(r => r.Payment)
                .Where(u => u.Reservations.Any(r => r.Status == ReservationStatus.Confirmed))
                .Select(u => new
                {
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    TotalSpent = u.Reservations
                        .Where(r => r.Status == ReservationStatus.Confirmed)
                        .Sum(r => r.TotalAmount)
                })
                .OrderByDescending(u => u.TotalSpent)
                .Take(5)
                .ToListAsync();

            return new Dictionary<string, object>
            {
                { "totalUsers", totalUsers },
                { "totalAdmins", totalAdmins },
                { "totalRegularUsers", totalRegularUsers },
                { "newUsersThisMonth", usersThisMonth },
                { "newUsersThisWeek", usersThisWeek },
                { "activeUsersLast30Days", activeUsers },
                { "topSpenders", topSpenders }
            };
        }

        public async Task<UserDetailsDTO> UpdateUserProfileAsync(int userId, UpdateUserProfileDTO updateUserProfileDto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            // Check if email is being changed and if new email already exists
            if (user.Email != updateUserProfileDto.Email &&
                await _context.Users.AnyAsync(u => u.Email == updateUserProfileDto.Email && u.Id != userId))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Update only the allowed fields (excluding Role)
            user.Email = updateUserProfileDto.Email;
            user.FirstName = updateUserProfileDto.FirstName;
            user.LastName = updateUserProfileDto.LastName;
            user.PhoneNumber = updateUserProfileDto.PhoneNumber;

            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }


    }
}

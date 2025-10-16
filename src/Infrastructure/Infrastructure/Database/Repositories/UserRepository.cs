using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

public class UserRepository(HiveDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await context.Users
            .Include(x=>x.UserRoles)
            .FirstOrDefaultAsync(e=> e.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await context.Users
            .Include(x=>x.UserRoles)
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await context.Users.FirstOrDefaultAsync(e => e.Username == username);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await context.Users.ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await context.Users.FirstOrDefaultAsync(e => e.Id == id);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await context.Users.AnyAsync(e => e.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await context.Users.AnyAsync(e => e.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await context.Users.AnyAsync(e => e.Username == username);
    }

    public Task<bool> UpdateUserInfoAsync(Guid commandUserId, string commandFirstName, string commandLastName,
        CancellationToken cancellationToken)
    {
        var user = context.Users.FirstOrDefault(u => u.Id == commandUserId);
        if (user == null)
        {
            return Task.FromResult(false);
        }

        user.FirstName = commandFirstName;
        user.LastName = commandLastName;

        context.Users.Update(user);
        return context.SaveChangesAsync(cancellationToken)
            .ContinueWith(t => t.IsCompletedSuccessfully, cancellationToken);
    }
}
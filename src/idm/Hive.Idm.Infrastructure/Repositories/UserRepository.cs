using Domain.Entities;
using Domain.Interfaces;
using Hive.Idm.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hive.Idm.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    IdmDbContext _idmDbContext;
    
    public UserRepository(IdmDbContext idmDbContext)
    {
        _idmDbContext = idmDbContext;
    }
    
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _idmDbContext.Users
            .Include(x=>x.UserRoles)
            .FirstOrDefaultAsync(e=> e.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _idmDbContext.Users
            .Include(x=>x.UserRoles)
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _idmDbContext.Users.FirstOrDefaultAsync(e => e.Username == username);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _idmDbContext.Users.ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        await _idmDbContext.Users.AddAsync(user);
        await _idmDbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _idmDbContext.Users.Update(user);
        await _idmDbContext.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _idmDbContext.Users.FirstOrDefaultAsync(e => e.Id == id);
        if (user != null)
        {
            _idmDbContext.Users.Remove(user);
            await _idmDbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _idmDbContext.Users.AnyAsync(e => e.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _idmDbContext.Users.AnyAsync(e => e.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _idmDbContext.Users.AnyAsync(e => e.Username == username);
    }
}
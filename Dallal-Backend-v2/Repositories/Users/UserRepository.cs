using Dallal_Backend_v2.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Repositories.Users;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByFirebaseUidAsync(string firebaseUid)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByFirebaseUidAsync(string firebaseUid)
    {
        return await _dbSet.AnyAsync(u => u.FirebaseUid == firebaseUid);
    }

    public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
    {
        return await _dbSet.AnyAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<User?> GetUserByEmailWithRolesAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Broker)
            .Include(u => u.Buyer)
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.Broker)
            .Include(u => u.Buyer)
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        await _dbSet.AddAsync(user);
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        _dbSet.Update(user);
    }

    public async Task UpdateUserProfileAsync(User user)
    {
        var existingUser = await _dbSet.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existingUser != null)
        {
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.ProfileImage = user.ProfileImage;
            existingUser.UpdatedAt = DateTime.UtcNow;
            
            }
    }

    public async Task<User?> GetUserByIdWithRolesAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.Broker)
            .Include(u => u.Buyer)
            .Include(u => u.Admin)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
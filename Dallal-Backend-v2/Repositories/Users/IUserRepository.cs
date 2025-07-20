using Dallal_Backend_v2.Entities.Users;

namespace Dallal_Backend_v2.Repositories.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByFirebaseUidAsync(string firebaseUid);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByFirebaseUidAsync(string firebaseUid);
    Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetUserByEmailWithRolesAsync(string email);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task UpdateUserProfileAsync(User user);
    Task<User?> GetUserByIdWithRolesAsync(Guid id);
}
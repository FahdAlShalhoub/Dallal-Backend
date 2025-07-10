using Dallal_Backend_v2.Entities.Users;

namespace Dallal_Backend_v2.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByFirebaseUidAsync(string firebaseUid);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByFirebaseUidAsync(string firebaseUid);
    Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
}
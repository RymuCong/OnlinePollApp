using Microsoft.EntityFrameworkCore;

namespace T3H.Poll.Application.Users.Services;

public class UserService : CrudService<User>, IUserService
{
    public UserService(IRepository<User, Guid> userRepository, Dispatcher dispatcher)
        : base(userRepository, dispatcher)
    {
    }
    
    public async Task<IEnumerable<string>> GetPermissionsAsync(Guid userId, string key)
    {
        var query = base._repository.GetQueryableSet();

        // Lấy role permissions
        var rolePermissions = await query
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles
                .SelectMany(ur => ur.Role.Claims
                    .Where(rc => rc.Type == key)
                    .Select(rc => new { Value = rc.Value })))
            .ToListAsync();

        // Lấy user permissions
        var userPermissions = await query
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Claims
                .Where(uc => uc.Type == key)
                .Select(uc => new { Value = uc.Value }))
            .ToListAsync();

        // Kết hợp và chuyển đổi sang Claims ở memory
        var permissions = rolePermissions
            .Union(userPermissions)
            .Select(p => p.Value);

        return permissions;
    }
}

namespace T3H.Poll.Application.Users.Services;

public interface IUserService : ICrudService<User>
{
    Task<IEnumerable<string>> GetPermissionsAsync(Guid userId, string key);
}

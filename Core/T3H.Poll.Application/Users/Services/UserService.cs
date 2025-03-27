namespace T3H.Poll.Application.Users.Services;

public class UserService : CrudService<User>, IUserService
{
    public UserService(IRepository<User, Guid> userRepository, Dispatcher dispatcher)
        : base(userRepository, dispatcher)
    {
    }
}

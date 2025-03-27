using T3H.Poll.Application.Common.DTOs;
namespace T3H.Poll.Application.Users.Queries;

public class GetUsersByNameQuery : IQuery<ResultModel<List<SearchUserResponse>>>
{
    public string UserName { get; set; }
    public bool AsNoTracking { get; set; }
}

internal class GetUsersByNameQueryHandler : IQueryHandler<GetUsersByNameQuery, ResultModel<List<SearchUserResponse>>>
{
    private readonly IRepository<User, Guid> _userRepository;

    public GetUsersByNameQueryHandler(IRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResultModel<List<SearchUserResponse>>> HandleAsync(GetUsersByNameQuery query, CancellationToken cancellationToken = default)
    {
        var result =  await _userRepository.ToListAsync(_userRepository.GetQueryableSet().Where(x => x.FullName.ToLower().Contains(query.UserName.ToLower()))
            .Select(u => new SearchUserResponse
            {
                UserName = u.UserName,
                Avatar = u.ColorAvatar,
                UserId = u.Id
            }));

        return ResultModel<List<SearchUserResponse>>.Create(result);
    }
}

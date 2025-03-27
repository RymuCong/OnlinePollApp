namespace T3H.Poll.Application.CommonSetting.Queries;
public class GetCommonSettingsByTypeQuery : IQuery<List<CommonSettingDto>>
{
    public GetCommonSettingsByTypeQuery()
    {
    }
}

public class GetCommonSettingsByTypeHandler : IQueryHandler<GetCommonSettingsByTypeQuery, List<CommonSettingDto>>
{
    private readonly ICommonSettingRepository _commonSettingRepository;

    public GetCommonSettingsByTypeHandler(ICommonSettingRepository commonSettingRepository)
    {
        _commonSettingRepository = commonSettingRepository;
    }

    public async Task<List<CommonSettingDto>> HandleAsync(GetCommonSettingsByTypeQuery query, CancellationToken cancellationToken)
    {
        var settings = await _commonSettingRepository.GetBySettingTypeAsync(SettingType.Industry);

        return settings.Select(s => new CommonSettingDto
        {
            Id = s.Id,
            Value = s.Value
        }).ToList();
    }
}

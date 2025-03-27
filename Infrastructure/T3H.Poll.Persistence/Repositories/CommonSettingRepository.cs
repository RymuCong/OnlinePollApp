namespace T3H.Poll.Persistence.Repositories;
    public class CommonSettingRepository : Repository<CommonSetting, Guid>, ICommonSettingRepository
{
    public CommonSettingRepository(CrmDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<List<CommonSetting>> GetBySettingTypeAsync(SettingType settingType)
    {
        return await GetQueryableSet()
            .Where(s => s.SettingType == settingType)
            .Select(s => new CommonSetting
            {
                Id = s.Id,
                Value = s.Value
            })
            .AsNoTracking()
            .ToListAsync();
    }

}

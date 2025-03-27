namespace T3H.Poll.Domain.Repositories
{
    public interface ICommonSettingRepository : IRepository<CommonSetting, Guid>
    {
        Task<List<CommonSetting>> GetBySettingTypeAsync(SettingType settingType);
    }
}

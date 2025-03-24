namespace FDS.CRM.Domain.Repositories
{
    public interface ICommonSettingRepository : IRepository<CommonSetting, Guid>
    {
        Task<List<CommonSetting>> GetBySettingTypeAsync(SettingType settingType);
    }
}

namespace T3H.Poll.Domain.Entities;

public class CommonSetting : Entity<Guid>, IAggregateRoot
{
    [StringLength(100)]
    public string Name { get; set; }
    [StringLength(100)]
    public string Value { get; set; }
    [StringLength(500)]
    public string Description { get; set; }

    public bool IsSensitive { get; set; }
    public SettingType SettingType { get; set; }

}



namespace T3H.Poll.Domain.Entities
{
    public abstract class Entity<TKey> : IHasKey<TKey>, ITrackable
    {
        public TKey Id { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public DateTimeOffset CreatedDateTime { get; set; }

        public DateTimeOffset? UpdatedDateTime { get; set; }

        public bool IsDeleted { get; set; }
        [StringLength(100)]
        public string? UserNameCreated { get; set; }
        [StringLength(100)]
        public string? UserNameUpdated { get; set; }
        [StringLength(1000)]
        public string? Filter { get; set; }
        [StringLength(1000)]
        public string? ExtraField1 { get; set; }
        [StringLength(1000)]
        public string? ExtraField2 { get; set; }
        [StringLength(1000)]
        public string? ExtraField3 { get; set; }

        //public string? UserNameCreated { get; set; } = HttpContextCustom.Current?.User?.Claims.Where(c => c.Type == "email").FirstOrDefault()?.Value;
        //// UserNameCreated
        //public string? UserNameUpdated { get; set; } = HttpContextCustom.Current?.User?.Claims.Where(c => c.Type == "unique_name").FirstOrDefault()?.Value;
    }
}

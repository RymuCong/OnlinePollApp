namespace T3H.Poll.Domain.Entities;

public class FileUpload : Entity<Guid>, IAggregateRoot
{
    public Guid ReferenceId { get; set; }  // Id của phần tham chiếu tới như contact, company
    [StringLength(150)]
    public string FileName { get; set; }
    [StringLength(250)]
    public string FileUrl { get; set; }
}

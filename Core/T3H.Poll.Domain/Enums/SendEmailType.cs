using System.ComponentModel;

namespace T3H.Poll.Domain.Enums;

public enum SendEmailType
{
    [Description("Email")]
    Email,

    [Description("Tải về")]
    Download,

    [Description("Bưu điện")]
    PostOffice
}

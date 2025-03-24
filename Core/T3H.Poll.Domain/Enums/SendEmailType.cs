using System.ComponentModel;

namespace FDS.CRM.Domain.Enums;

public enum SendEmailType
{
    [Description("Email")]
    Email,

    [Description("Tải về")]
    Download,

    [Description("Bưu điện")]
    PostOffice
}

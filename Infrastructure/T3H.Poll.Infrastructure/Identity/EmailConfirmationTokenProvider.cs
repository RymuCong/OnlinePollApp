using Microsoft.AspNetCore.DataProtection;

namespace FDS.CRM.Infrastructure.Identity;

public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
    where TUser : class
{
    public EmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
        IOptions<EmailConfirmationTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}

public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
{
}
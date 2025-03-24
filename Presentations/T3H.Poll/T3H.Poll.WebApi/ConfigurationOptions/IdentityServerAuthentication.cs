using CryptographyHelper.Certificates;

namespace FDS.CRM.WebApi.ConfigurationOptions;

public class IdentityServerAuthentication
{
    public string Provider { get; set; }

    public string Authority { get; set; }

    public string ApiName { get; set; }

    public bool RequireHttpsMetadata { get; set; }

    public OpenIddictOptions OpenIddict { get; set; }
}

public class OpenIddictOptions
{
    public string IssuerUri { get; set; }

    public CertificateOption TokenDecryptionCertificate { get; set; }

    public CertificateOption IssuerSigningCertificate { get; set; }
}

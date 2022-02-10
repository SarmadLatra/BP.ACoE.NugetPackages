using System.Security.Cryptography.X509Certificates;
using BP.ACoE.ChatBotHelper.Helpers;
using BP.ACoE.ChatBotHelper.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BP.ACoE.ChatBotHelper.Extensions
{
    public interface ISalesForceCertificateHelper
    {
        X509Certificate2? Certificate { get; set; }
    }

    public class SalesForceCertificateHelper : ISalesForceCertificateHelper
    {
        public X509Certificate2? Certificate { get; set; }

        public SalesForceCertificateHelper(X509Certificate2? certificate)
        {
            Certificate = certificate;
        }
    }


    public static class SalesForceCertificateExtensions
    {
        public static IServiceCollection AddSalesForceCertificate(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureKeyVaultSettings>(configuration.GetSection(AzureKeyVaultSettings.AzureKeyVaultSettingsName));
            services.Configure<SalesForceCertificateSettings>(configuration.GetSection(SalesForceCertificateSettings.SalesForceCertificateSettingsName));

            return TryAddSalesForceCertificate(services, configuration);
        }

        private static IServiceCollection TryAddSalesForceCertificate(IServiceCollection services, IConfiguration configuration)
        {
            // get the certificate object from configuration
            var serviceProvider = services.BuildServiceProvider();
            var salesForceSettings = serviceProvider.GetRequiredService<IOptions<SalesForceCertificateSettings>>().Value;
            salesForceSettings.Validate();

            //var certName = configuration.GetValue<string>("SalesForceCertName");
            //var certPath = configuration.GetValue<string>("SalesForceCertPath");
            //var certPassword = configuration.GetValue<string>("SalesForceCertPassword");
            var certFullPath = Path.Combine(salesForceSettings.SalesForceCertPath!, salesForceSettings.SalesForceCertName!);
            var cert = File.Exists(certFullPath) ? new X509Certificate2(File.ReadAllBytes(certFullPath), salesForceSettings.SalesForceCertPassword, X509KeyStorageFlags.UserKeySet
                    | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable) : null;

            // when every thing is good
            if (CertificateHelper.ValidateCertificate(cert))
            {
                services.AddSingleton<ISalesForceCertificateHelper>(new SalesForceCertificateHelper(cert));
            }
            else
            {
                // when we have Get a new certificate
                try
                {
                    var azureSettings = serviceProvider.GetRequiredService<IOptions<AzureKeyVaultSettings>>().Value;
                    cert = CertificateHelper.DownloadAndSaveNewPfx(salesForceSettings, azureSettings).GetAwaiter().GetResult();
                    if (CertificateHelper.ValidateCertificate(cert))
                    {
                        services.AddSingleton<ISalesForceCertificateHelper>(new SalesForceCertificateHelper(cert));
                    }
                    else
                    {
                        throw new InvalidOperationException("Unable to download Sales Force certificate from KeyVault or unable to save the certificate");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }

            return services;
        }
    }
}

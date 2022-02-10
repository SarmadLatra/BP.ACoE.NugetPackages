

namespace BP.ACoE.ChatBotHelper.Settings
{
    public class AzureKeyVaultSettings
    {
        public const string AzureKeyVaultSettingsName = "AzureKeyVaultCertificate";
        public string? KeyVaultCertUri { get; set; }
        public string? AzureAuthUrl { get; set; }
        public string? AzureKeyVaultScope { get; set; }
        public string? AppClientId { get; set; }
        public string? AppClientSecret { get; set; }
        public string? CertVersion { get; set; } = "?api-version=7.0";
    }

    public class SalesForceCertificateSettings
    {
        public const string SalesForceCertificateSettingsName = "SalesForceCertificate";
        public string? SalesForceCertPath { get; set; }
        public string? SalesForceCertName { get; set; }
        public string? SalesForceCertPassword { get; set; }
    }


    public static class SettingsValidationExtensions
    {
        public static void Validate(this AzureKeyVaultSettings azureKeyVaultSettings)
        {
            if (string.IsNullOrEmpty(azureKeyVaultSettings.AppClientId))
            {
                throw new ArgumentNullException(nameof(azureKeyVaultSettings.AppClientId), $" can't be null");
            }
            if (string.IsNullOrEmpty(azureKeyVaultSettings.AppClientSecret))
            {
                throw new ArgumentNullException(nameof(azureKeyVaultSettings.AppClientSecret), $" can't be null");
            }
            if (string.IsNullOrEmpty(azureKeyVaultSettings.AzureAuthUrl))
            {
                throw new ArgumentNullException(nameof(azureKeyVaultSettings.AzureAuthUrl), $" can't be null");
            }
            if (string.IsNullOrEmpty(azureKeyVaultSettings.AzureKeyVaultScope))
            {
                throw new ArgumentNullException(nameof(azureKeyVaultSettings.AzureKeyVaultScope), $" can't be null");
            }
            if (string.IsNullOrEmpty(azureKeyVaultSettings.KeyVaultCertUri))
            {
                throw new ArgumentNullException(nameof(azureKeyVaultSettings.KeyVaultCertUri), $" can't be null");
            }
        }

        public static void Validate(
            this SalesForceCertificateSettings salesForceCertificateSettings)
        {
            if (string.IsNullOrEmpty(salesForceCertificateSettings.SalesForceCertName))
            {
                throw new ArgumentNullException(nameof(salesForceCertificateSettings.SalesForceCertName), "should not be empty");
            }

            if (string.IsNullOrEmpty(salesForceCertificateSettings.SalesForceCertPath))
            {
                throw new ArgumentNullException(nameof(salesForceCertificateSettings.SalesForceCertPath), $" can't be null");
            }
        }
    }

}

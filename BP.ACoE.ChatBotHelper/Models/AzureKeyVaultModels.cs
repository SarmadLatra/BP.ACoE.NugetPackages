using Newtonsoft.Json;

namespace BP.ACoE.ChatBotHelper.Models
{
    public class AccessTokenResponse
    {
        [JsonProperty("token_type")]
        public string? TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("ext_expires_in")]
        public long ExtExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }
    }

    public  class PrivateKeyResponse
    {
        [JsonProperty("value")]
        public string? Value { get; set; }

        [JsonProperty("contentType")]
        public string? ContentType { get; set; }

        [JsonProperty("id")]
        public Uri? Id { get; set; }

        [JsonProperty("managed")]
        public bool Managed { get; set; }

        [JsonProperty("attributes")]
        public Attributes? Attributes { get; set; }

        [JsonProperty("kid")]
        public Uri? Kid { get; set; }
    }

    public class PublicKeyResponse
    {
        [JsonProperty("key")]
        public Key? Key { get; set; }

        [JsonProperty("attributes")]
        public Attributes? Attributes { get; set; }

        [JsonProperty("managed")]
        public bool Managed { get; set; }
    }

    public class Attributes
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("nbf")]
        public long Nbf { get; set; }

        [JsonProperty("exp")]
        public long Exp { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }

        [JsonProperty("recoveryLevel")]
        public string? RecoveryLevel { get; set; }
    }

    public class Key
    {
        [JsonProperty("kid")]
        public Uri? Kid { get; set; }

        [JsonProperty("kty")]
        public string? Kty { get; set; }

        [JsonProperty("key_ops")]
        public string[]? KeyOps { get; set; }

        [JsonProperty("n")]
        public string? N { get; set; }

        [JsonProperty("e")]
        public string? E { get; set; }
    }

    public  class CertificateResponse
    {
        [JsonProperty("id")]
        public Uri? Id { get; set; }

        [JsonProperty("kid")]
        public Uri? Kid { get; set; }

        [JsonProperty("sid")]
        public Uri? Sid { get; set; }

        [JsonProperty("x5t")]
        public string? X5T { get; set; }

        [JsonProperty("cer")]
        public string? Cer { get; set; }

        [JsonProperty("attributes")]
        public Attributes? Attributes { get; set; }

        [JsonProperty("policy")]
        public Policy? Policy { get; set; }
    }

    public  class Policy
    {
        [JsonProperty("id")]
        public Uri? Id { get; set; }

        [JsonProperty("key_props")]
        public KeyProps? KeyProps { get; set; }

        [JsonProperty("secret_props")]
        public SecretProps? SecretProps { get; set; }

        [JsonProperty("x509_props")]
        public X509Props? X509Props { get; set; }

        [JsonProperty("lifetime_actions")]
        public LifetimeAction[]? LifetimeActions { get; set; }

        [JsonProperty("issuer")]
        public Issuer? Issuer { get; set; }

        [JsonProperty("attributes")]
        public PolicyAttributes? Attributes { get; set; }
    }

    public  class PolicyAttributes
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }
    }

    public  class Issuer
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public  class KeyProps
    {
        [JsonProperty("exportable")]
        public bool Exportable { get; set; }

        [JsonProperty("kty")]
        public string? Kty { get; set; }

        [JsonProperty("key_size")]
        public long KeySize { get; set; }

        [JsonProperty("reuse_key")]
        public bool ReuseKey { get; set; }
    }

    public  class LifetimeAction
    {
        [JsonProperty("trigger")]
        public Trigger? Trigger { get; set; }

        [JsonProperty("action")]
        public Action? Action { get; set; }
    }

    public  class Action
    {
        [JsonProperty("action_type")]
        public string? ActionType { get; set; }
    }

    public  class Trigger
    {
        [JsonProperty("lifetime_percentage")]
        public long LifetimePercentage { get; set; }
    }

    public  class SecretProps
    {
        [JsonProperty("contentType")]
        public string? ContentType { get; set; }
    }

    public  class X509Props
    {
        [JsonProperty("subject")]
        public string? Subject { get; set; }

        [JsonProperty("sans")]
        public Sans? Sans { get; set; }

        [JsonProperty("ekus")]
        public string[]? Ekus { get; set; }

        [JsonProperty("key_usage")]
        public string[]? KeyUsage { get; set; }

        [JsonProperty("validity_months")]
        public long ValidityMonths { get; set; }

        [JsonProperty("basic_constraints")]
        public BasicConstraints? BasicConstraints { get; set; }
    }

    public class BasicConstraints
    {
        [JsonProperty("ca")]
        public bool Ca { get; set; }
    }

    public class Sans
    {
        [JsonProperty("emails")]
        public object[]? Emails { get; set; }

        [JsonProperty("dns_names")]
        public string[]? DnsNames { get; set; }

        [JsonProperty("upns")]
        public object[]? Upns { get; set; }
    }
}

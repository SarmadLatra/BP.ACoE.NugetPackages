using System.Security.Cryptography;
using System.Text;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using Microsoft.Extensions.Options;
using Serilog;

namespace BP.ACoE.ChatBotHelper.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly EncryptionSettings _encryptionSettings;
        private readonly ILogger _logger;

        private const string ClassName = "EncryptionService--";

        public EncryptionService(IOptions<EncryptionSettings> encryptionSettings, ILogger logger)
        {
            _encryptionSettings = encryptionSettings.Value;
            _logger = logger.ForContext<EncryptionService>();
        }

        public string Encrypt(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            const string methodName = "Encrypt--";
            _logger.Information($"{ClassName}{methodName} encryption process started");
            var encryptionKey = _encryptionSettings.EncryptionKey;
            var clearBytes = Encoding.Unicode.GetBytes(text);
            using var aes = Aes.Create();
            var pdb = new Rfc2898DeriveBytes(encryptionKey!, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

            aes.Key = pdb.GetBytes(32);
            aes.IV = pdb.GetBytes(16);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.Close();
            var encryptedEmailId = Convert.ToBase64String(ms.ToArray());
            _logger.Information($"{ClassName}{methodName} encryption process completed");
            return encryptedEmailId;
        }

        public string Decrypt(string text)
        {
            const string methodName = "Decrypt--";
            _logger.Information($"{ClassName}{methodName} decryption process started");
            var encryptionKey = _encryptionSettings.EncryptionKey;
            text = text.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(text);
            using var aes = Aes.Create();
            var pdb = new Rfc2898DeriveBytes(encryptionKey!, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

            aes.Key = pdb.GetBytes(32);
            aes.IV = pdb.GetBytes(16);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherBytes, 0, cipherBytes.Length);
            cs.Close();
            var decryptedEmailId = Encoding.Unicode.GetString(ms.ToArray());

            _logger.Information($"{ClassName}{methodName} decryption process completed");
            return decryptedEmailId;

        }
    }
}

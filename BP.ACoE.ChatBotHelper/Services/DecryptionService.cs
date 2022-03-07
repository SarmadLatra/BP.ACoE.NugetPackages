using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using BPMeAUChatBot.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BPMeAUChatBot.API.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private const string ClassName = "DecryptionService--";

        public EncryptionService(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger.ForContext<EncryptionService>();
        }

        public string Encrypt(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            const string methodName = "Encrypt--";
            _logger.Information($"{ClassName}{methodName} encryption process started");
            var encryptionKey = _configuration.GetValue<string>("EncryptionKey");
            var clearBytes = Encoding.Unicode.GetBytes(text);
            using var aes = Aes.Create();
            var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            if (aes != null)
            {
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
            _logger.Error("Unable to create encryption service object");
            throw new CryptographicException("Unable to create encryption service object");
        }

        public string Decrypt(string text)
        {
            const string methodName = "Decrypt--";
            _logger.Information($"{ClassName}{methodName} decryption process started");
            var encryptionKey = _configuration.GetValue<string>("EncryptionKey");
            text = text.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(text);
            using var aes = Aes.Create();
            var pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            if (aes != null)
            {
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
            _logger.Error("Unable to create encryption service object");
            throw new CryptographicException("Unable to create encryption service object");
        }
    }
}

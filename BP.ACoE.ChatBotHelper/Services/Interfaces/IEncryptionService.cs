namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string text);

        string Decrypt(string text);
    }
}


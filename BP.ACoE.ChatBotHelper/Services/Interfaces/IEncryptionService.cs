namespace BPMeAUChatBot.API.Services.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string text);

        string Decrypt(string text);
    }
}


using BP.ACoE.ChatBotHelper.Models;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface ISalesForceAuthService
    {
        Task<SalesForceAuthResponse?> GetSalesForceAccessToken();
        Task<SalesForceAuthResponse?> DoSalesForceAuth();
        Task<string?> GenerateJwtWithCertificateFile(object jwtPayload);
    }
}
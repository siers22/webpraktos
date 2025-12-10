namespace PRAKTOSWEBAPI.Services
{
    public interface IEmailService
    {
        void SendEmail(string to, string login, string password);
    }
}
namespace Pishgaman_Project.Models
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }

}

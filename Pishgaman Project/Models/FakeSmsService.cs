namespace Pishgaman_Project.Models
{
    public class FakeSmsService : ISmsService
    {
        public Task SendSmsAsync(string phoneNumber, string message)
        {
            // Simulate sending an SMS
            Console.WriteLine($"Fake SMS sent to {phoneNumber}: {message}");
            return Task.CompletedTask;
        }
    }

}

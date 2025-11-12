namespace slick.Application.Services.Interfaces
{
    public class SmsSettings
    {
        public string ProviderName { get; set; } = string.Empty; // or others
        public string ApiUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
    }
}

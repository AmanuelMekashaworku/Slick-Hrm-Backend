namespace slick.Application.Services.Interfaces
{
    public class SMTPSettings
    {
        public required string SmtpServer { get; set; }
        public required string SmtpUsername { get; set; }
        public required string SmtpPassword { get; set; }
        public required int SmtpPort { get; set; }
    }
}

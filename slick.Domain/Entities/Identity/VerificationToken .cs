namespace slick.Domain.Entities.Identity
{
    public class VerificationToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }  // Expiry date of the token
    }
}

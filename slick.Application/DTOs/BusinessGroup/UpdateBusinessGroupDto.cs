namespace slick.Application.DTOs.BusinessGroup
{
    public class UpdateBusinessGroupDto : BusinessGroupBaseDto
    {
        public Guid ID { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}

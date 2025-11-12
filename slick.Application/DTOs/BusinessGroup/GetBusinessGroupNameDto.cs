namespace slick.Application.DTOs.BusinessGroup
{
    public class GetBusinessGroupNameDto
    {
        public Guid ID { get; set; }
        public string BusinessGroupName { get; set; } = string.Empty;
    }
}
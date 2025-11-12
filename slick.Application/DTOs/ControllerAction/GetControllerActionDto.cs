namespace slick.Application.DTOs.ControllerAction
{
    public class GetControllerActionDto : ControllerActionBaseDto
    {
        public Guid ID { get; set; }
        public required string DisplayController { get; set; }
        public required string ActionName { get; set; }
    }
}


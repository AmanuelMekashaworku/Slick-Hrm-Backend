namespace slick.Application.DTOs.TaskController
{
    public class GetTaskControllerNameDto
    {
        public Guid ID { get; set; }
        public string DisplayController { get; set; } = string.Empty;
    }
}
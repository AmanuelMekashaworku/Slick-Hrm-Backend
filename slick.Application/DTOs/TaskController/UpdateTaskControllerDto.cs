namespace slick.Application.DTOs.TaskController
{
    public class UpdateTaskControllerDto : TaskControllerBaseDto
    {
        public Guid ID { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}

using user_service.Models.DTOs;

namespace project_service.Models.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public UserDto? User { get; set; } // Inclure un DTO utilisateur
    }
}

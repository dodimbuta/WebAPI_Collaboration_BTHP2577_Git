using Microsoft.Build.Evaluation;

namespace user_service.Models.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        //public required string PasswordHash { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        //public ICollection<Project>? Projects { get; set; }

    }

    //public class ProjectDto
    //{
    //    public int Id { get; set; }
    //    public string ProjectName { get; set; } = string.Empty;
    //}
}

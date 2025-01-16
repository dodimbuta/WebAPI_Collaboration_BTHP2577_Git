using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using user_service.Models.Entities;

namespace project_service.Models.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public required string ProjectName { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }
        //[NotMapped]
        //[JsonIgnore]
        //public User User { get; set; } = null!;// Navigation Property
    }
}

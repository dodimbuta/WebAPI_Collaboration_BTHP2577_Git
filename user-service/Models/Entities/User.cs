using Microsoft.Build.Evaluation;
using System.ComponentModel.DataAnnotations;

namespace user_service.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string PasswordHash { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string RoleProject { get; set; } = string.Empty;
        //[Timestamp]
        // public required byte[] RowVersion { get; set; }
        //public List<User> Projects { get; set; } = new List<User>();// Navigation Property
        //public ICollection<Project>? Projects { get; set; }

    }

}

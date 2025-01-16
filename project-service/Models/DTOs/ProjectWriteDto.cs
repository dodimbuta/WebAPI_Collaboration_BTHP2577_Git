using System.ComponentModel.DataAnnotations;

namespace project_service.Models.DTOs;

public sealed class ProjectWriteDto
{
    [Required, StringLength(150, MinimumLength = 1)]
    public string ProjectName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCompleted { get; set; }
    public int? UserId { get; set; }
}

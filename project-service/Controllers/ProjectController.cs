using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.Models.DTOs;
using project_service.Models.Entities;
using project_service.Services;

namespace project_service.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ProjectController(ProjectDbContext context, UserServiceClient userServiceClient) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetProjects(CancellationToken cancellationToken)
    {
        var projects = await context.Projects.AsNoTracking()
            .OrderBy(project => project.Id)
            .ToListAsync(cancellationToken);

        var results = new List<ProjectDto>(projects.Count);
        foreach (var project in projects)
        {
            var user = project.UserId.HasValue
                ? await userServiceClient.GetUserByIdAsync(project.UserId.Value, cancellationToken)
                : null;
            results.Add(ToDto(project, user));
        }

        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id, CancellationToken cancellationToken)
    {
        var project = await context.Projects.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (project is null)
            return NotFound(new { message = $"Project with ID {id} was not found." });

        var user = project.UserId.HasValue
            ? await userServiceClient.GetUserByIdAsync(project.UserId.Value, cancellationToken)
            : null;
        return Ok(ToDto(project, user));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(
        [FromBody] ProjectWriteDto request, CancellationToken cancellationToken)
    {
        if (request.EndDate < request.StartDate)
            return BadRequest(new { message = "EndDate must be on or after StartDate." });

        var user = await FindUserAsync(request.UserId, cancellationToken);
        if (request.UserId.HasValue && user is null)
            return BadRequest(new { message = $"User with ID {request.UserId.Value} does not exist." });

        var project = new Project
        {
            ProjectName = request.ProjectName.Trim(),
            Description = request.Description.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsCompleted = request.IsCompleted,
            UserId = request.UserId
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, ToDto(project, user));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProject(
        int id, [FromBody] ProjectWriteDto request, CancellationToken cancellationToken)
    {
        if (request.EndDate < request.StartDate)
            return BadRequest(new { message = "EndDate must be on or after StartDate." });

        var project = await context.Projects.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (project is null)
            return NotFound(new { message = $"Project with ID {id} was not found." });

        var user = await FindUserAsync(request.UserId, cancellationToken);
        if (request.UserId.HasValue && user is null)
            return BadRequest(new { message = $"User with ID {request.UserId.Value} does not exist." });

        project.ProjectName = request.ProjectName.Trim();
        project.Description = request.Description.Trim();
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.IsCompleted = request.IsCompleted;
        project.UserId = request.UserId;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProject(int id, CancellationToken cancellationToken)
    {
        var project = await context.Projects.FindAsync(new object[] { id }, cancellationToken);
        if (project is null)
            return NotFound(new { message = $"Project with ID {id} was not found." });

        context.Projects.Remove(project);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Task<user_service.Models.DTOs.UserDto?> FindUserAsync(int? userId, CancellationToken cancellationToken) =>
        userId.HasValue
            ? userServiceClient.GetUserByIdAsync(userId.Value, cancellationToken)
            : Task.FromResult<user_service.Models.DTOs.UserDto?>(null);

    private static ProjectDto ToDto(Project project, user_service.Models.DTOs.UserDto? user) => new()
    {
        Id = project.Id,
        ProjectName = project.ProjectName,
        Description = project.Description,
        StartDate = project.StartDate,
        EndDate = project.EndDate,
        IsCompleted = project.IsCompleted,
        User = user
    };
}

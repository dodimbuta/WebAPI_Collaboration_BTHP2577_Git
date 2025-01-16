using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.Models.DTOs;
using project_service.Models.Entities;
using System.Net.Http;
using System.Text.Json;
using user_service.Models.DTOs;
using user_service.Models.Entities;
using static project_service.Controllers.ProjectController;

namespace project_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectDbContext _context;
        private readonly UserServiceClient _userServiceClient;

        public ProjectController(ProjectDbContext context, UserServiceClient userServiceClient)
        {
            _context = context;
            _userServiceClient = userServiceClient;
        }

        // GET: api/Project - Returns a list of all projects.
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            // Récupérer tous les projets depuis la base de données
            var projects = await _context.Projects.ToListAsync();

            // Liste pour stocker les projets avec les informations utilisateur
            var projectDtos = new List<ProjectDto>();

            foreach (var project in projects)
            {
                UserDto? userDto = null;

                // Si un UserId est défini, récupérer les informations utilisateur depuis le microservice utilisateur
                if (project.UserId.HasValue)
                {
                    var user = await _userServiceClient.GetUserByIdAsync(project.UserId.Value);
                    if (user != null)
                    {
                        userDto = new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            FullName = user.FullName
                        };
                    }
                }

                // Ajouter le projet au DTO avec les données utilisateur si disponibles
                projectDtos.Add(new ProjectDto
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName,
                    Description = project.Description,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    IsCompleted = project.IsCompleted,
                    User = userDto
                });
            }

            // Retourner la liste des projets sous forme de DTOs
            return Ok(projectDtos);
        }


        // GET: api/Project/{id} - Returns a specific project based on its Id.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            User? user = null;
            if (project.UserId.HasValue)
            {
                user = await _userServiceClient.GetUserByIdAsync(project.UserId.Value);
            }

            var projectDto = new
            {
                project.Id,
                project.ProjectName,
                project.Description,
                project.StartDate,
                project.EndDate,
                project.IsCompleted,
                User = user // Ajout des informations utilisateur si disponibles
            };

            return Ok(projectDto);
        }


        // POST: api/Project - Creates a new project.
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project project)
        {
            if (project.UserId.HasValue)
            {
                var user = await _userServiceClient.GetUserByIdAsync(project.UserId.Value);
                if (user == null)
                {
                    return BadRequest($"User with ID {project.UserId.Value} does not exist.");
                }
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        // PUT: api/Project/{id} - Updates an existing project.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] Project project)
        {
            if (id != project.Id)
            {
                return BadRequest("Project ID in the URL does not match the ID in the request body.");
            }

            if (project.UserId.HasValue)
            {
                var user = await _userServiceClient.GetUserByIdAsync(project.UserId.Value);
                if (user == null)
                {
                    return BadRequest($"User with ID {project.UserId.Value} does not exist.");
                }
            }

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound($"Project with ID {id} not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Project/{id} - Deletes a project based on its Id.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method to check if a project exists by ID.
        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }

    // Service for interacting with the UserService API.
    public class UserServiceClient
    {
        private readonly HttpClient _httpClient;

        public UserServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration["MicroserviceUrls:UserService"]);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"users/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(content);
            }
            return null;
        }
    }

}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.Models.Entities;
using System.Net.Http;
using System.Text.Json;
using user_service.Models.Entities;

namespace project_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectContext _context;
        public ProjectController(ProjectContext context)
        {
            _context = context;
        }

        //GET: api/Project - Returns a list of all projects.
        [HttpGet]
        //Uses ToListAsync() to get data asynchronously.
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            return await _context.Projects.ToListAsync();
        }
        
        //GET: api/Project/{id} - Returns a specific project based on its Id.
        [HttpGet("{id}")]
        //Returns 404 Not Found if the project does not exist.
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }

        //POST: api/Project - Creates a new project
        [HttpPost]
        //Returns a 201 Created HTTP code with the created project.
        public async Task<ActionResult<Project>> CreateProject(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        //PUT: api/Project/{id} - Updates an existing project
        [HttpPut("{id}")]
        //Checks that the ID provided in the URL matches the project ID sent in the request body.
        //Returns 400 Bad Request if the IDs do not match, or 404 Not Found if the project does not exist.
        public async Task<IActionResult> UpdateProject(int id, Project project)
        {
            if (id != project.Id)
            {
                return BadRequest();
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
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        //DELETE /api/Project/{id} - Deletes a project based on its Id
        [HttpDelete("{id}")]
        //Returns 204 No Content on success or 404 Not Found if the project does not exist
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //Interaction between Microservices (HTTP Communication)
        //The ProjectService can call an API exposed by the UserService to validate or retrieve information about users.
        public class UserServiceClient
        {
            private readonly HttpClient _httpClient;

            public UserServiceClient(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public async Task<User?> GetUserByIdAsync(int userId)
            {
                var response = await _httpClient.GetAsync($"http://localhost:32775/user-service/api/users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<User>(content);
                }
                return null;
            }
        }
    }

}

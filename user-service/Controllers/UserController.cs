using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using user_service.Data;
using user_service.Models.Entities;

namespace user_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;

        public UserController(UserContext context)
        {
            _context = context;
        }

        // GET: api/User - Returns a list of all users
        [HttpGet]
        //Use ToListAsync() to retrieve data asynchronously.
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/User/{id} - Get users informations
        [HttpGet("user/{id}")]
        //Returns a specific user based on its Id.
        //Returns a HTTP 404 Not Found code if the user does not exist.
        public async Task<ActionResult<User>> GetUser(int id)
        {
            //var user = await _context.Users.GetUserById(id);
            var user = await _context.Users.FindAsync(id);
            //return user != null ? Ok(user) : NotFound();
            if (user == null)
                return NotFound("User not found.");
            return user;
        }

        // POST: api/User - Creates a new user (login)
        [HttpPost()]
        //Returns an HTTP 201 Created code with the new user and its ID.
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return Conflict(new { message = "A user with this email already exists." });
            }

            var newUser = new User
            {
                UserName = user.UserName,
                PasswordHash = HashPassword(user.PasswordHash),
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            //    return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, new { newUser.Id, newUser.UserName });
            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, user);
        }

        // POST: api/User/login - User Authentication
        [HttpPost("login")]
        public async Task<IActionResult> Login(User login)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == login.UserName);

            if (user == null || user.PasswordHash != HashPassword(login.PasswordHash))
                return Unauthorized("UserName or Password is not correct.");

            return Ok(new { user.Id, user.UserName });
        }
        // Password hashing method
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // PUT: api/User/{id} - Updates an existing user
        [HttpPut("user/{id}")]
        //Verifies that the ID provided in the URL matches the user ID in the request body.
        //Returns 400 Bad Request if the IDs don't match, or 404 Not Found if the user doesn't exist.
        public async Task<IActionResult> UpdateUser(int id, User userUpdate)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            user.Email = userUpdate.Email;
            user.FullName = userUpdate.FullName;
            user.UserName = userUpdate.UserName;
            user.PhoneNumber = userUpdate.PhoneNumber;
            user.Address = userUpdate.Address;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(await _context.Users.ToListAsync());
        }

        //DELETE: api/User/user/{id} - Deletes a user based on their Id
        [HttpDelete("user/{id}")]
        //Returns 204 No Content on success or 404 Not Found if the user does not exist.
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(await _context.Users.ToListAsync());
        }

    }
}

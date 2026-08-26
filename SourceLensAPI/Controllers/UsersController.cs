using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SourceLensAPI.Models;

namespace SourceLensAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly SourceLensDbContext _context;

        public UsersController(SourceLensDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // PUT: api/Users/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (existingUser == null)
                return NotFound($"User with ID {id} not found.");

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;

            await _context.SaveChangesAsync();

            return Ok(existingUser);
        }

        // DELETE: api/Users/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound($"User with ID {id} not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User deleted successfully.",
                userId = id
            });
        }
    }
}
using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user is null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto request)
        {
            try
            {
                var createdUser = await _userService.CreateUserAsync(request);

                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {              
                return BadRequest($"Error creating user: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto request)
        {
            try
            {
                var result = await _userService.LoginAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {               
                return Unauthorized(ex.Message);
            }
        }
    }
}

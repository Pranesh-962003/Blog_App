using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly SupabaseServices? _supaServices;
        private readonly MongoService _mongoService;

        public AuthController(SupabaseServices supabaseServices, MongoService mongo)
        {
            _supaServices =  supabaseServices;
            _mongoService = mongo;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await _supaServices!.InitializeAsync();
            var client = _supaServices.Client;
            return Ok("supabase initialised");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string email, [FromForm] string password)
        {
            await _supaServices!.InitializeAsync();
            var result = _supaServices.Client.Auth.SignUp(email, password);

            if(result?.IsCompleted != null)
            {
                return Ok(result.IsCompleted);
            }


            return Ok("Registration successfull");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            await _supaServices!.InitializeAsync();

            var session = await _supaServices.Client.Auth.SignIn(email, password);

            if (session != null && session.User != null)
            {
                if (!string.IsNullOrEmpty(session.User?.Email))
                {
                    await _mongoService.AddUserIfNotExist(email);

                    return Ok(new { Token = session.User.Email });
                }
                else
                {

                    return Unauthorized("Login failed user not authenticated.");
                }
            }
            else
            {
                return Unauthorized("Login failed.");
            }
        }

    }
}

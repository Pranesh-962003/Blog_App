using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {

        private readonly MongoService? _mongoService;

        public UserController(MongoService mongoService) { 
            _mongoService = mongoService;
        }

        [HttpGet("test")]
      public IActionResult Test()
        {
            return Ok("Working");
        }


        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromQuery] string subscriberEmail, [FromQuery] string targetEmail)
        {
            if (subscriberEmail == targetEmail)
                return BadRequest("You cannot subscribe to yourself.");

            await _mongoService!.SubscribeUser(subscriberEmail, targetEmail);
            return Ok("Subscribed successfully.");
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromQuery] string subscriberEmail, [FromQuery] string targetEmail)
        {
            await _mongoService!.UnsubscribeUser(subscriberEmail, targetEmail);
            return Ok("Unsubscribed successfully.");
        }

        [HttpGet("subscribers")]
        public async Task<IActionResult> GetSubscribers([FromQuery] string email)
        {
            var subscribers = await _mongoService!.GetAllSubscribers(email);
            return Ok(subscribers);
        }

        [HttpGet("subscribedto")]
        public async Task<IActionResult> GetSubscribedTo([FromQuery] string email)
        {
            var subscribedTo = await _mongoService!.GetAllSubscribedTo(email);
            return Ok(subscribedTo);
        }

        [HttpPut("update-name")]
        public async Task<IActionResult> UpdateName([FromBody] UpdateNameRequest request)
        {
            var success = await _mongoService!.UpdateUserName(request.Email, request.NewName);
            if (!success)
                return NotFound("User not found.");

            return Ok("Name updated successfully.");
        }

        [HttpGet("get_article_author/{email}")]
        public async Task<ActionResult<List<ArticleModel>>> GetArticlesByAuthor(string email)
        {
            var articles = await _mongoService!.GetArticlesByAuthor(email);
            return Ok(articles);
        }

        [HttpGet("get-user/{email}")]
        public async Task<ActionResult<UserModel>> GetUserByEmail(string email)
        {
            Console.WriteLine(email);
            var user = await _mongoService!.GetUserByEmail(email);
            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }



        [HttpGet("get-email-by-id/{id}")]
        public async Task<IActionResult> GetEmailById(string id)
        {
            var email = await _mongoService!.GetUserEmailById(id);
            if (string.IsNullOrEmpty(email))
                return NotFound("User not found or no email available.");

            return Ok(email);
        }

        [HttpPost("favorite/add")]
        public async Task<IActionResult> AddToFavorites([FromQuery] string userEmail, [FromQuery] string articleId)
        {
            var success = await _mongoService!.AddToFavoritesAsync(userEmail, articleId);
            if (!success) return BadRequest("Failed to add article to favorites.");
            return Ok("Article added to favorites.");
        }

        [HttpPost("favorite/remove")]
        public async Task<IActionResult> RemoveFromFavorites([FromQuery] string userEmail, [FromQuery] string articleId)
        {
            var success = await _mongoService!.RemoveFromFavoritesAsync(userEmail, articleId);
            if (!success) return BadRequest("Failed to remove article from favorites.");
            return Ok("Article removed from favorites.");
        }

        [HttpGet("favorite/list")]
        public async Task<IActionResult> GetFavoriteArticles([FromQuery] string userEmail)
        {
            var articles = await _mongoService!.GetFavoriteArticlesAsync(userEmail);
            return Ok(articles);
        }


        [HttpGet("is-favorited")]
        public async Task<IActionResult> IsFavorited([FromQuery] string userEmail, [FromQuery] string articleId)
        {
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(articleId))
                return BadRequest("Missing userEmail or articleId");

            var isFavorited = await _mongoService!.IsArticleFavoritedAsync(userEmail, articleId);
            return Ok(isFavorited); // returns true or false
        }

    }

    public class UpdateNameRequest
    {
        public string Email { get; set; } = null!;
        public string NewName { get; set; } = null!;
    }
}

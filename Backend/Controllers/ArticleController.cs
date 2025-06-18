using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleController : Controller
    {
       private readonly MongoService _mongoService;
        private readonly ImageUploadService _imageUploadService;
        private readonly IHubContext<NotificationHub>? hubContext;
        private readonly EmbeddingService _embeddingService;
        private readonly SearchService _searchService;
        public ArticleController(MongoService mongoService, ImageUploadService imageUploadService, IHubContext<NotificationHub> _hubContext, EmbeddingService embeddingService, SearchService searchService)
        {
            _mongoService = mongoService;
            _imageUploadService = imageUploadService;
            hubContext = _hubContext;
            _embeddingService = embeddingService;
            _searchService = searchService;
        }
        [HttpPost("add-article")]
        public async Task<IActionResult> CreateArticle([FromForm] IFormFile? image, [FromForm] string heading, [FromForm] string articleBody, [FromForm] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Author email is missing.");

            // Ensure user exists
            await _mongoService.AddUserIfNotExist(email);

            // Upload image to Cloudinary if provided
            string imageUrl = "";
            if (image != null)
            {
                imageUrl = await _imageUploadService.UploadImageAsync(image) ?? "";
            }

            // Create article
            var article = new ArticleModel
            {
                Heading = heading,
                ArticleBody = articleBody,
                Email = email,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };
            var embedding = await _embeddingService.GetEmbedding(articleBody);
            article.Embedding = embedding;

            // Save to MongoDB
            await _mongoService.AddArticle(article);

            // Notify subscribers
            var subscribers = await _mongoService.GetAllSubscribers(email);
            foreach (var subscriber in subscribers)
            {
                try
                {
                    Console.WriteLine(subscriber);
                    var notificationMessage = $"New article posted: {article.Heading}";
                    string? userId = subscriber;
                    await hubContext!.Clients.User(userId).SendAsync("ReceiveNotification", notificationMessage);
                  
                   
              


                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("pranesh962003.p@gmail.com", "kdmpmtkhjkdxlfna"),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("pranesh962003.p@gmail.com"), // Use your verified sender
                        Subject = "New Article Posted",
                        Body = $"Hello,\n\nA new article has been posted by {email}.\n\nTitle: {heading}\n\n{articleBody}",
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add(subscriber);

                    smtpClient.Send(mailMessage);





                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email to {subscriber}: {ex.Message}");
                    // Don't return error, log and continue
                }
            }

            return Ok(new { message = "Article created and emails sent." });
        }

        [HttpGet]
        public async Task<ActionResult<List<ArticleModel>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required.");

            var result = await _searchService.SearchArticlesAsync(query, topN: 1);
            return Ok(result);
        }

        [HttpGet("author/{email}")]
        public async Task<ActionResult<List<ArticleModel>>> GetArticlesByAuthor(string email)
        {
            var articles = await _mongoService.GetArticlesByAuthor(email);
            return Ok(articles);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateArticle(string id, [FromBody] ArticleModel article)
        {
            if (article == null || article.Id != id)
                return BadRequest("Article data is invalid.");

            await _mongoService.UpdateArticle(article);
            return Ok(new { message = "Article updated successfully." });
        }

        // DELETE api/articles/{id}
        [HttpDelete("article-delete/{id}")]
        public async Task<IActionResult> DeleteArticle(string id)
        {
            await _mongoService.DeleteArticle(id);
            return Ok(new { message = "Article deleted successfully." });
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetArticle() {
           var responce =  await _mongoService.GetAllArticles();
            return Ok(responce);
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<ActionResult<ArticleModel>> GetById(string id)
        {
            var article = await _mongoService.GetArticleById(id);

            if (article == null)
            {
                return NotFound($"Article with ID '{id}' was not found.");
            }

            return Ok(article);
        }


        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeArticle(string id, [FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("User email is missing.");

            var result = await _mongoService.LikeArticle(id, email);

            return result ? Ok(new { message = "Article liked." }) :
              BadRequest("Already liked or error occurred.");
        }


        [HttpPost("{id}/unlike")]
        public async Task<IActionResult> UnlikeArticle(string id, [FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("User email is missing.");

            var result = await _mongoService.UnlikeArticle(id, email);

            return result ? Ok(new { message = "Article unliked." }) :
              BadRequest("Not liked before or error occurred.");
        }


        [HttpGet("{id}/likes/count")]
        public async Task<IActionResult> GetLikeCount(string id)
        {
            var count = await _mongoService.GetArticleLikeCount(id);
            return Ok(new { likeCount = count });
        }

        [HttpGet("{id}/likes/status")]
        public async Task<IActionResult> HasUserLikedArticle(string id, [FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("User email is missing.");

            var liked = await _mongoService.HasUserLikedArticle(id, email);
            return Ok(new { liked });
        }


        [HttpGet("{id}/has-liked")]
        public async Task<IActionResult> HasUserLiked(string id, [FromQuery] string userEmail)
        {
            bool hasLiked = await _mongoService.HasUserLikedArticle(id, userEmail);
            return Ok(hasLiked); // ✅ Make sure this returns JUST true/false, not an object
        }


    }
}

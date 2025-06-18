using Backend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;
using System.Xml.Linq;

namespace Backend.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<UserModel> _users;
        private readonly IMongoCollection<ArticleModel> _articles;

        public MongoService(IConfiguration config)
        {
            var settings = config.GetSection("MongodbSettings").Get<MongodbSettings>();
            var client = new MongoClient(settings?.ConnectionString);
            var database = client.GetDatabase(settings?.DatabaseName);

            _users = database.GetCollection<UserModel>("Users");
            _articles = database.GetCollection<ArticleModel>("articles");
        }

        // Users collection methods

        public async Task AddUserIfNotExist(string email)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Email, email);
            var update = Builders<UserModel>.Update
                .SetOnInsert(u => u.Email, email)
                .SetOnInsert(u => u.Name, "")
                .SetOnInsert(u => u.Subscribers, new List<string>())
                .SetOnInsert(u => u.SubscribedTo, new List<string>());

            var options = new UpdateOptions { IsUpsert = true };

            await _users.UpdateOneAsync(filter, update, options);
        }

        //public async Task AddSubscriber(string authorEmail, string subscriberEmail)
        //{
        //    var filter = Builders<UserModel>.Filter.Eq(u => u.Email, authorEmail);
        //    var update = Builders<UserModel>.Update.AddToSet(u => u.Subscribers, subscriberEmail);
        //    var options = new UpdateOptions { IsUpsert = true }; // create user if not exists
        //    await _users.UpdateOneAsync(filter, update, options);
        //}


        public async Task SubscribeUser(string subscriberEmail, string targetEmail)
        {
            var subscriberFilter = Builders<UserModel>.Filter.Eq(u => u.Email, subscriberEmail);
            var targetFilter = Builders<UserModel>.Filter.Eq(u => u.Email, targetEmail);

            // Add subscriberEmail to target's Subscribers list
            var updateTarget = Builders<UserModel>.Update.AddToSet(u => u.Subscribers, subscriberEmail);
            await _users.UpdateOneAsync(targetFilter, updateTarget);

            // Add targetEmail to subscriber's SubscribedTo list
            var updateSubscriber = Builders<UserModel>.Update.AddToSet(u => u.SubscribedTo, targetEmail);
            await _users.UpdateOneAsync(subscriberFilter, updateSubscriber);
        }

        public async Task UnsubscribeUser(string subscriberEmail, string targetEmail)
        {
            var subscriberFilter = Builders<UserModel>.Filter.Eq(u => u.Email, subscriberEmail);
            var targetFilter = Builders<UserModel>.Filter.Eq(u => u.Email, targetEmail);

            // Remove subscriber from target’s Subscribers list
            var updateTarget = Builders<UserModel>.Update.Pull(u => u.Subscribers, subscriberEmail);
            await _users.UpdateOneAsync(targetFilter, updateTarget);

            // Remove target from subscriber's SubscribedTo list
            var updateSubscriber = Builders<UserModel>.Update.Pull(u => u.SubscribedTo, targetEmail);
            await _users.UpdateOneAsync(subscriberFilter, updateSubscriber);
        }

        public async Task<List<string>> GetAllSubscribedTo(string userEmail)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Email, userEmail);
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            return user?.SubscribedTo ?? new List<string>();
        }


        //public async Task RemoveSubscriber(string authorEmail, string subscriberEmail)
        //{
        //    var filter = Builders<UserModel>.Filter.Eq(u => u.Email, authorEmail);
        //    var update = Builders<UserModel>.Update.Pull(u => u.Subscribers, subscriberEmail);
        //    await _users.UpdateOneAsync(filter, update);
        //}

        //public async Task<int> GetSubscriberCount(string authorEmail)
        //{
        //    var filter = Builders<UserModel>.Filter.Eq(u => u.Email, authorEmail);
        //    var user = await _users.Find(filter).FirstOrDefaultAsync();
        //    return user?.Subscribers.Count ?? 0;
        //}
        public async Task<List<string>> GetAllSubscribers(string authorEmail)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Email, authorEmail);
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            return user?.Subscribers ?? new List<string>();
        }

        // Articles collection methods

        public async Task AddArticle(ArticleModel article)
        {
            article.CreatedAt = DateTime.UtcNow;
            await _articles.InsertOneAsync(article);
        }

        public async Task<List<ArticleModel>> GetAllArticles()
        {
            return await _articles.Find(_ => true).ToListAsync();
        }

        public async Task<List<ArticleModel>> GetArticlesByAuthor(string authorEmail)
        {
            var filter = Builders<ArticleModel>.Filter.Eq(a => a.Email, authorEmail);
            return await _articles.Find(filter).ToListAsync();
        }

        public async Task<ArticleModel?> GetArticleById(string articleId)
        {
            var filter = Builders<ArticleModel>.Filter.Eq(a => a.Id, articleId);
            return await _articles.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateArticle(ArticleModel updatedArticle)
        {
            var filter = Builders<ArticleModel>.Filter.Eq(a => a.Id, updatedArticle.Id);
            await _articles.ReplaceOneAsync(filter, updatedArticle);
        }

        public async Task DeleteArticle(string articleId)
        {
            var filter = Builders<ArticleModel>.Filter.Eq(a => a.Id, articleId);
            await _articles.DeleteOneAsync(filter);
        }
        public async Task<bool> UpdateUserName(string email, string newName)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Email, email);
            var update = Builders<UserModel>.Update.Set(u => u.Name, newName);

            var result = await _users.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task<UserModel?> GetUserByEmail(string email)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Email, email);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> LikeArticle(string articleId, string userEmail)
        {
            var filter = Builders<ArticleModel>.Filter.Eq(a => a.Id, articleId);
            var update = Builders<ArticleModel>.Update.AddToSet(a => a.Likes, userEmail);
            var result = await _articles.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UnlikeArticle(string articleId, string userEmail)
        {
            var filter = Builders<ArticleModel>.Filter.Eq(a => a.Id, articleId);
            var update = Builders<ArticleModel>.Update.Pull(a => a.Likes, userEmail);
            var result = await _articles.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }


        public async Task<int> GetArticleLikeCount(string articleId)
        {
            var article = await GetArticleById(articleId);
            return article?.Likes?.Count ?? 0;
        }

        public async Task<bool> HasUserLikedArticle(string articleId, string userEmail)
        {
            var article = await GetArticleById(articleId);
            return article?.Likes?.Contains(userEmail) ?? false;
        }

        public async Task<string?> GetUserEmailById(string id)
        {
            var filter = Builders<ArticleModel>.Filter.Eq(u => u.Id, id);
            var projection = Builders<ArticleModel>.Projection.Include(u => u.Email);
            var result = await _articles.Find(filter).Project<UserModel>(projection).FirstOrDefaultAsync();

            return result?.Email;
        }


        public async Task<bool> AddToFavoritesAsync(string userEmail, string articleId)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Email, userEmail);
            var update = Builders<UserModel>.Update.AddToSet(u => u.FavoriteArticles, articleId);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveFromFavoritesAsync(string userEmail, string articleId)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Email, userEmail);
            var update = Builders<UserModel>.Update.Pull(u => u.FavoriteArticles, articleId);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<string>> GetFavoriteArticlesAsync(string userEmail)
        {
            var user = await _users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();
            return user?.FavoriteArticles ?? new List<string>();
        }

        public async Task<bool> IsArticleFavoritedAsync(string userEmail, string articleId)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.Email, userEmail);
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            if (user == null) return false;

            return user.FavoriteArticles.Contains(articleId);
        }

    }
}

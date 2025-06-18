using Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class SearchService
    {
        private readonly EmbeddingService _embeddingService;
        private readonly MongoService _mongoService;

        public SearchService(EmbeddingService embeddingService, MongoService mongoService)
        {
            _embeddingService = embeddingService;
            _mongoService = mongoService;
        }

        public async Task<List<ArticleModel>> SearchArticlesAsync(string query, int topN = 1)
        {
            // Get the query embedding
            var queryVector = await _embeddingService.GetEmbedding(query, isQuery: true);
            var articles = await _mongoService.GetAllArticles();

            var scoredArticles = new List<(ArticleModel Article, float Score)>();

            foreach (var article in articles)
            {
                // Use precomputed embedding if available
                var articleVector = article.Embedding?.ToArray();
                if (articleVector == null || articleVector.Length == 0)
                {
                    // Fallback: Compute embedding using heading + body
                    string content = $"{article.Heading} {article.ArticleBody}";
                    articleVector = await _embeddingService.GetEmbedding(content, isQuery: false);
                }

                float similarity = CosineSimilarity(queryVector, articleVector);
                scoredArticles.Add((article, similarity));
            }

            // Order by descending similarity and return top N
            return scoredArticles
        .OrderByDescending(x => x.Score)
        .Take(topN)
        .Select(x => x.Article)
        .ToList();
        }

        private float CosineSimilarity(float[] v1, float[] v2)
        {
            float dot = 0f, mag1 = 0f, mag2 = 0f;
            for (int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }

            return (float)(dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2)));
        }
    }
}

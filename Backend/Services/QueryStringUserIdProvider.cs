using Microsoft.AspNetCore.SignalR;

namespace Backend.Services
{
    public class QueryStringUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var httpContext = connection.GetHttpContext();
            if (httpContext != null && httpContext.Request.Query.TryGetValue("userId", out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}

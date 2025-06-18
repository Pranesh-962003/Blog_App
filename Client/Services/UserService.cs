using Client.Models;
using System.Net;
using System.Net.Http;
namespace Client.Services
{
    public class UserService
    {
        private readonly HttpClient _http;

        public UserService(HttpClient http)
        {
            _http = http;
        }

        public async Task<UserModel?> GetUserByEmail(string email)
        {
            return await _http.GetFromJsonAsync<UserModel>($"{ApiConstants.BaseUrl}/api/user/get-user/{email}");
        }


        public async Task<bool> UnsubscribeUser(string subscriberEmail, string targetEmail)
        {
            var url = $"{ApiConstants.BaseUrl}/api/user/unsubscribe?targetEmail={targetEmail}&subscriberEmail={subscriberEmail}";

            var response = await _http.PostAsync(url, null); // POST request with empty body

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserName(string email, string newName)
        {
            Console.WriteLine(newName);
          
            try
            {
                var request = new
                {
                    Email = email,
                    NewName = newName
                };

                var response = await _http.PutAsJsonAsync($"{ApiConstants.BaseUrl}/api/user/update-name", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
                
            }
        }




        public async Task LikeArticle(string articleId, string userEmail)
        {
            var response = await _http.PostAsJsonAsync(
                $"{ApiConstants.BaseUrl}/api/article/{articleId}/like", userEmail);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                Console.WriteLine(result?["message"]);
            }
            else
            {
                Console.WriteLine("Error liking article.");
            }
        }
    }
}

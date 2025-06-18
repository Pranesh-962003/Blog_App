namespace Client.Services
{
    public class TokenServices
    {
        public string? accesstoken { get; private set; } = null;
        public void setToken (string token) {  
            accesstoken = token; 
        }
    }
}

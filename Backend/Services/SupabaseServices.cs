using Supabase;
namespace Backend.Services
{
    public class SupabaseServices
    {
        private Supabase.Client? _client;

        private const string SupabaseUrl = "https://nlohtcebaoatxsxwfthv.supabase.co";
        private const string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im5sb2h0Y2ViYW9hdHhzeHdmdGh2Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDcyOTY2MDAsImV4cCI6MjA2Mjg3MjYwMH0.qXk91FJzIC0qzQ7n7tsAzNqRLyBimONTa_0fcPyKvTQ";


        public async Task InitializeAsync()
        {
            try
            {

                var option = new SupabaseOptions
                {
                    AutoConnectRealtime = true
                };

                _client = new Supabase.Client(SupabaseUrl, supabaseKey, option);
                await _client.InitializeAsync();
            }
            catch (Exception ex) { 
            Console.WriteLine($"Error initializing Supabase client: {ex.Message}");
                throw;
            }
        }
        public Supabase.Client Client
        {
            get
            {
                if (_client == null)
                    throw new InvalidOperationException("Supabase client not initialized.");
                return _client;
            }
        }
    }
}

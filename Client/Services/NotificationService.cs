using Microsoft.AspNetCore.SignalR.Client;

namespace Client.Services
{
    public class NotificationService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private string? userId;

        public event Action<string>? OnNotificationReceived;

        public async Task InitializeAsync(string hubUrl, string userEmail)
        {

            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                Console.WriteLine("⚠️ Already connected. Skipping reinitialization.");
                return;
            }

            userId = userEmail;
            Console.WriteLine(userId);

            _hubConnection = new HubConnectionBuilder()
           .WithUrl($"{hubUrl}?userId={Uri.EscapeDataString(userId)}")
           .WithAutomaticReconnect()
           .Build();

            _hubConnection.On<string>("ReceiveNotification", message =>
            {
                Console.WriteLine("📩 SignalR message received: " + message);
                OnNotificationReceived?.Invoke(message);
            });

            await StartAsync();
        }

        private async Task StartAsync()
        {
            if (_hubConnection?.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("✅ SignalR connected.");
            }
        }

        public async Task StopAsync()
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
                await _hubConnection.StopAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }

        }
    }
}

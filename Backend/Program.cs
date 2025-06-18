using Backend.Hubs;
using Backend.Models;
using Backend.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<SupabaseServices>();
builder.Services.AddScoped<MongoService>();
builder.Services.AddSingleton<IUserIdProvider, QueryStringUserIdProvider>();
builder.Services.AddHttpClient<EmbeddingService>();
builder.Services.AddScoped<SearchService>();



//Cloudinary

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton(sp =>
{
    var settings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
    var account = new Account(settings!.CloudName, settings.ApiKey, settings.ApiSecret);
    return new CloudinaryDotNet.Cloudinary(account);
});

builder.Services.AddScoped<ImageUploadService>();

builder.Services.AddSignalR();






var app = builder.Build();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

app.Run();

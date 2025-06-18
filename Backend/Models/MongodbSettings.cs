namespace Backend.Models
{
    public class MongodbSettings
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public string? CollectionsName { get; set; }
    }
}

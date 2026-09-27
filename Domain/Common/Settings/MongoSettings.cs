namespace Domain.Common.Settings
{
    public class MongoSettings
    {
        public const string SectionName = "MongoSettings";

        public string ConnectionString { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string ReviewsCollection { get; set; } = "game_reviews";
    }
}

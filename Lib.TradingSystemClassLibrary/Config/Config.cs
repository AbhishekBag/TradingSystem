namespace TradingSystem.TradingSystemConfig
{
    public class Config
    {
        public int NumberOfThreads { get; set; } = 50;
        
        public int OrderExpiryMinutes { get; set; } = 30; // Default expiry duration
        
        public int ExpiryCheckIntervalSeconds { get; set; } = 60; // Frequency of expiry checks
    }
}

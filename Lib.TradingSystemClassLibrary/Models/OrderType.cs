using System.Text.Json.Serialization;

namespace TradingSystem.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderType
    {
        Buy,
        Sell
    }
}

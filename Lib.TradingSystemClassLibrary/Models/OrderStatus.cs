using System.Text.Json.Serialization;

namespace TradingSystem.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        Accepted,
        Rejected,
        Canceled,
        Completed,
        Expired
    }
}

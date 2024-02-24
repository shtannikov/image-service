namespace DataConsistencyFunction;

public class LambdaEvent
{
    [System.Text.Json.Serialization.JsonPropertyName("detail-type")]
    public string? DetailType { get; set; }
}
namespace DataConsistencyFunction;

public class LambdaEvent
{
    [System.Text.Json.Serialization.JsonPropertyName("detail-type")]
    public string? DetailType { get; set; }
    
    public string? Path { get; set; }

    public string GetSource()
    {
        if (Path != null)
            return "API Gateway";

        return DetailType ?? "Unknown";
    }
}
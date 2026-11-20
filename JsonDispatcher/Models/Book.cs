using System.Text.Json.Serialization;

namespace JsonDispatcher.Models;

public class Book
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("author")] public string Author { get; set; } = "";
    [JsonPropertyName("year")] public int Year { get; set; }
    [JsonPropertyName("genre")] public string Genre { get; set; } = "";
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("available")] public bool Available { get; set; } = true;

    public Book Clone() => (Book)MemberwiseClone();
}
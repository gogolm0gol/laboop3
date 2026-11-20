using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using JsonDispatcher.Models;

namespace JsonDispatcher.Services;

public static class JsonService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
    };

    public static List<Book> Load(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<List<Book>>(json, Options) ?? new List<Book>();
    }

    public static void Save(string path, IEnumerable<Book> books)
    {
        string json = JsonSerializer.Serialize(books, Options);
        File.WriteAllText(path, json, Encoding.UTF8);
    }
}
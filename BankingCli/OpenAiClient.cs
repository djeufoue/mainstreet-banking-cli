using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankingCli;

internal sealed class OpenAiClient(HttpClient http, Uri endpoint, string model, string apiKey)
{
    public static OpenAiClient? FromEnvironment(HttpClient http)
    {
        // Read the key at runtime so it stays outside the repository.
        string? key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        string endpoint = Environment.GetEnvironmentVariable("OPENAI_API_ENDPOINT")
            ?? "https://api.openai.com/v1/chat/completions";
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("OPENAI_API_ENDPOINT must be an absolute HTTPS URL.");
        }

        string? configuredModel = Environment.GetEnvironmentVariable("OPENAI_MODEL");
        string model = string.IsNullOrWhiteSpace(configuredModel) ? "gpt-4.1-mini" : configuredModel.Trim();
        return new OpenAiClient(http, uri, model, key.Trim());
    }

    public async Task<string> CompleteAsync(string instructions, string input)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model,
            store = false,
            messages = new[]
            {
                new { role = "system", content = instructions },
                new { role = "user", content = input }
            }
        });

        using HttpResponseMessage response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Completion? completion = await response.Content.ReadFromJsonAsync<Completion>();
        Choice? choice = completion?.Choices?.FirstOrDefault();
        string? content = choice?.Message?.Content;
        if (choice?.FinishReason != "stop" || string.IsNullOrWhiteSpace(content))
        {
            throw new JsonException("The API did not return a complete text reply.");
        }

        return content.Trim();
    }

    private sealed record Completion(Choice[]? Choices);
    private sealed record Choice(
        Message? Message,
        [property: JsonPropertyName("finish_reason")] string? FinishReason);
    private sealed record Message(string? Content);
}

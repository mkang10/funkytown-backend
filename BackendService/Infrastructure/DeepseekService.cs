using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Interfaces;
using System.Net.Http.Headers;
using Domain.DTO.Request;
using Domain.DTO.Response;
namespace Infrastructure
{
    public class DeepSeekService : IChatServices
    {
        private readonly HttpClient _httpClient;
        private readonly IChatBotRepository _botRepo;
        private readonly JsonSerializerOptions _opts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public DeepSeekService(HttpClient httpClient, IChatBotRepository botRepo)
        {
            _httpClient = httpClient;
            _botRepo = botRepo;
        }

        public async Task<ChatCompletionResponse> CreateChatCompletionWithFunctionsAsync(
    List<ChatMessage> history,
    List<FunctionDefinition> functions,
    CancellationToken ct = default)
        {
            var req = new ChatCompletionRequest
            {
                Model = "deepseek-chat",
                Stream = false,
                Messages = history,
                Functions = functions,
                FunctionCall = "auto"
            };
            var json = JsonSerializer.Serialize(req, _opts);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _httpClient.PostAsync("/chat/completions", content, ct);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<ChatCompletionResponse>(body, _opts)!;
        }

        public async Task StreamChatAsync(
            List<ChatMessage> history,
            Func<string, Task> onChunk,
            CancellationToken ct = default)
        {
            var bot = await _botRepo.GetDefaultAsync(ct)
                      ?? throw new InvalidOperationException("ChatBot config missing");
            // set base URL + API key
            _httpClient.BaseAddress = new Uri(bot.BaseUrl!);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", bot.Key);

            var req = new ChatCompletionRequest
            {
                Model = "deepseek-chat",
                Stream = true,
                Messages = history
            };
            var json = JsonSerializer.Serialize(req, _opts);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = await _httpClient.PostAsync("/chat/completions", content, ct);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;
                var payload = line["data: ".Length..].Trim();
                if (payload == "[DONE]") break;

                var chunkObj = JsonSerializer.Deserialize<ChatCompletionChunk>(payload, _opts);
                var delta = chunkObj?.Choices?.FirstOrDefault()?.Delta?.Content;
                if (!string.IsNullOrEmpty(delta))
                    await onChunk(delta);
            }
        }

        public async Task<string> GetFullChatReplyAsync(
            List<ChatMessage> history,
            CancellationToken ct = default)
        {
            var bot = await _botRepo.GetDefaultAsync(ct)
                      ?? throw new InvalidOperationException("ChatBot config missing");
            _httpClient.BaseAddress = new Uri(bot.BaseUrl!);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", bot.Key);

            var req = new ChatCompletionRequest
            {
                Model = "deepseek-chat",
                Stream = false,
                Messages = history
            };
            var json = JsonSerializer.Serialize(req, _opts);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _httpClient.PostAsync("/chat/completions", content, ct);
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(body, _opts)
                         ?? throw new InvalidOperationException("Empty response");

            return result.Choices?.FirstOrDefault()?.Message?.Content
                   ?? throw new InvalidOperationException("No reply");
        }


    }
}


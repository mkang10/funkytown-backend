using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Models;

namespace Infrastructure
{
    public class ChatAppService
    {
        private readonly IConversationBotRepository _convRepo;
        private readonly IMessageBotRepository _msgRepo;
        private readonly IChatBotRepository _botRepo;
        private readonly IChatServices _chatSvc;
        private readonly IOutfitRecommendationService _outfitSvc;

        public ChatAppService(
            IConversationBotRepository convRepo,
            IMessageBotRepository msgRepo,
            IChatBotRepository botRepo,
            IChatServices chatSvc,
            IOutfitRecommendationService outfitSvc)
        {
            _convRepo = convRepo;
            _msgRepo = msgRepo;
            _botRepo = botRepo;
            _chatSvc = chatSvc;
            _outfitSvc = outfitSvc;
        }

        public async Task<int> GetOrCreateConversationAsync(int userId, CancellationToken ct = default)
        {
            var list = await _convRepo.GetByUserAsync(userId, ct);
            var exist = list.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
            if (exist != null) return exist.ConversationId;

            var title = $"Chat_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
            var conv = new ConversationsBot { UserId = userId, CreatedAt = DateTime.UtcNow, Title = title };
            await _convRepo.AddAsync(conv, ct);
            return conv.ConversationId;
        }

        private async Task<List<ChatMessage>> BuildHistoryAsync(int conversationId, CancellationToken ct)
        {
            var botConfig = await _botRepo.GetDefaultAsync(ct)
                           ?? throw new InvalidOperationException("ChatBot config missing");

            var conv = await _convRepo.GetByIdAsync(conversationId, ct)
                       ?? throw new InvalidOperationException("Conversation not found");

            var history = new List<ChatMessage>();
            if (!string.IsNullOrWhiteSpace(botConfig.Context))
            {
                history.Add(new ChatMessage
                {
                    Role = "system",
                    Content = botConfig.Context
                             
                });
            }

            history.AddRange(conv.MessagesBots
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessage { Role = m.Sender, Content = m.MessageContent }));

            return history;
        }

        public async Task<string> GetFullReplyAsync(
            int userId,
            string userContent,
            CancellationToken ct = default)
        {
            // 1) Conversation
            var convId = await GetOrCreateConversationAsync(userId, ct);

            // 2) Lưu user message
            await _msgRepo.AddAsync(new MessagesBot
            {
                ConversationId = convId,
                Sender = "user",
                MessageContent = userContent,
                SentAt = DateTime.UtcNow
            }, ct);

            // 3) Xây lịch sử
            var history = await BuildHistoryAsync(convId, ct);

            // Trong ChatAppService.GetFullReplyAsync, thay đổi đoạn gọi outfitSvc như sau:

            // 4) Phát hiện và xử lý yêu cầu phối đồ
            //if (TryParseOutfitRequest(userContent, out var outfitReq))
            //{
            //    // Nếu SizeId chưa xác định, map từ height/weight
            //    if (!outfitReq.SizeId.HasValue)
            //        outfitReq.SizeId = GetSizeIdFromHeightWeight(outfitReq.HeightCm, outfitReq.WeightKg);

            //    // Lấy màu ưa thích đầu tiên hoặc "Any"
            //    var colorPref = outfitReq.ColorPreferences?.FirstOrDefault() ?? "Any";

            //    // Lấy gợi ý từ DB
            //    var suggestion = await _outfitSvc.RecommendOutfitAsync(
            //        outfitReq.Occasion ?? "Casual",
            //        outfitReq.Style ?? "Basic",
            //        colorPref,
            //        outfitReq.SizeId!.Value,
            //        ct);

            //    // Format reply
            //    var replyText = FormatOutfitSuggestion(suggestion);

            //    // Lưu assistant message
            //    await _msgRepo.AddAsync(new MessagesBot
            //    {
            //        ConversationId = convId,
            //        Sender = "assistant",
            //        MessageContent = replyText,
            //        SentAt = DateTime.UtcNow
            //    }, ct);

            //    return replyText;
            //}


            // 5) Ngược lại gọi LLM
            var fullReply = await _chatSvc.GetFullChatReplyAsync(history, ct);

            // 6) Lưu assistant message
            await _msgRepo.AddAsync(new MessagesBot
            {
                ConversationId = convId,
                Sender = "assistant",
                MessageContent = fullReply,
                SentAt = DateTime.UtcNow
            }, ct);

            return fullReply;
        }

        #region Helpers

        private bool TryParseOutfitRequest(string content, out OutfitRequest req)
        {
            req = new OutfitRequest();

            var h = Regex.Match(content, @"cao\s*(?<v>\d+(\.\d+)?)\s*(m|cm)?", RegexOptions.IgnoreCase);
            var w = Regex.Match(content, @"nặng\s*(?<v>\d+(\.\d+)?)\s*(kg)?", RegexOptions.IgnoreCase);
            var o = Regex.Match(content, @"(tiệc|dạo phố|đi học|đi làm)", RegexOptions.IgnoreCase);
            var s = Regex.Match(content, @"phong cách\s+(?<v>\w+)", RegexOptions.IgnoreCase);
            var c = Regex.Match(content, @"màu\s+(?<v>[\w,\s]+)", RegexOptions.IgnoreCase);

            bool hasH = h.Success, hasW = w.Success;
            int? heightCm = null, weightKg = null;
            if (hasH)
            {
                var hv = double.Parse(h.Groups["v"].Value);
                heightCm = hv > 10 ? (int)hv : (int)(hv * 100);
            }
            if (hasW)
                weightKg = (int)double.Parse(w.Groups["v"].Value);

            List<string>? colors = null;
            if (c.Success)
                colors = c.Groups["v"].Value
                          .Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(x => x.Trim()).ToList();

            if (hasH && hasW)
            {
                req = new OutfitRequest
                {
                    HeightCm = heightCm,
                    WeightKg = weightKg,
                    Occasion = o.Success ? o.Value : null,
                    Style = s.Success ? s.Groups["v"].Value : null,
                    ColorPreferences = colors,
                    SizeId = null
                };
                return true;
            }

            return false;
        }

        private int GetSizeIdFromHeightWeight(int? h, int? w)
        {
            if (!h.HasValue || !w.HasValue) return 1;
            var heightM = h.Value / 100.0;
            var bmi = w.Value / (heightM * heightM);
            return bmi < 18.5 ? 1 :
                   bmi < 22 ? 2 :
                   bmi < 25 ? 3 :
                   bmi < 28 ? 4 : 5;
        }

        private string FormatOutfitSuggestion(OutfitSuggestionDto s)
        {
            return
$@"Dưới đây là gợi ý set đồ cho bạn:
- Áo (Top): {s.Top?.Product.Name} (Màu {s.Top?.Color?.ColorName}, Giá {s.Top?.Price:C})
- Quần (Bottom): {s.Bottom?.Product.Name} (Màu {s.Bottom?.Color?.ColorName}, Giá {s.Bottom?.Price:C})
- Giày (Shoes): {s.Shoes?.Product.Name} (Màu {s.Shoes?.Color?.ColorName}, Giá {s.Shoes?.Price:C})
- Phụ kiện: {s.Accessory?.Product.Name} (Màu {s.Accessory?.Color?.ColorName}, Giá {s.Accessory?.Price:C})
";
        }

        #endregion
    }
}

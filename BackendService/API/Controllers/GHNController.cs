
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Repositories;
using Infrastructure.HelperServices;
using Domain.Interfaces;
using Application.Interfaces;
using Domain.DTO.Request;
using Domain.DTO.Response;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GHNController : ControllerBase
    {


        private readonly HttpClient _httpClient;
        private string _url = "https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/create";
        private string token = "8c24c3ed-fb9d-11ef-82e7-a688a46b55a3";
        private string shopid = "196109";
        private readonly GHNLogHandler _logHandler;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderProcessingHelper _orderProcessingHelper;

        public GHNController(HttpClient httpClient, GHNLogHandler logHandler, IOrderRepository orderRepository, IOrderProcessingHelper orderProcessingHelper)
        {
            _httpClient = httpClient;
            _logHandler = logHandler;
            _orderRepository = orderRepository;
            _orderProcessingHelper = orderProcessingHelper;
        }

        [HttpPost("create-order/{id}")]
        public async Task<IActionResult> CreateOrder(int id)
        {
            try
            {
                var data = await _logHandler.AutoCreateOrderGHN(id);
                // Newtonsoft.Json.JsonConvert.SerializeObject(data) vẫn giữ lại kiểu viết việt nam để kh bị lỗi fomat khi lên GHN api
                var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(data);

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                content.Headers.Add("ShopId", shopid);
                content.Headers.Add("Token", token);
                Console.WriteLine(requestJson);

                var response = await _httpClient.PostAsync(_url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();

                    var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);

                    if (jsonResponse != null && jsonResponse.ContainsKey("data"))
                    {
                        // tách data ra
                        var dataDict = jsonResponse["data"] as Newtonsoft.Json.Linq.JObject;
                        // sử dụng linq để truy xuất từ data trong dataDict để lấy ra giá trị "order_code"
                        var orderCode = dataDict?["order_code"]?.ToString();

                        var success = await _logHandler.AddGHNIdToOrderTableHandler(id, new UpdateGHNIdDTO { GHNId = orderCode }); // xuất GHNId vô

                        if (success)
                        {
                            return Ok(responseData);
                        }
                        else
                        {
                            return StatusCode(500, "Failed to save GHN ID");
                        }
                    }
                    else
                    {
                        return StatusCode(500, "Invalid response format");
                    }
                }

                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

        [HttpPost("order-status-list")]
        public async Task<IActionResult> GetOrderStatusListl([FromBody] OrderDetailWithUpdateRequest orderDetailRequest)
        {
            if (orderDetailRequest == null || string.IsNullOrEmpty(orderDetailRequest.order_code))
            {
                return BadRequest("Invalid order code.");
            }

            var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(orderDetailRequest);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            // Thêm token
            content.Headers.Add("Token", token);
            var response = await _httpClient.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/detail", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseData);

                var data = jsonResponse["data"];

                if (data == null)
                {
                    return BadRequest("Dữ liệu không tồn tại trong phản hồi từ GHN.");
                }

                // Kiểm tra nếu có log
                var logsToken = data["log"];
                if (logsToken != null && logsToken.Type == JTokenType.Array)
                {
                    var logs = logsToken.ToObject<List<Domain.DTO.Request.LogEntry>>();

                    var latestStatuses = logs
         .OrderByDescending(log => log.updated_date) // sắp xếp toàn bộ theo thời gian giảm dần
         .GroupBy(log => log.status) // group theo trạng thái
         .Select(g => g.First()) // Lấy log đầu tiên trong mỗi nhóm (log mới nhất)
         .Select(log => new
         {
             log.status,
             UpdatedDate = log.updated_date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") // Định dạng DATE TIME
         })
         .ToList();
                    if (latestStatuses.Any(s => s.status == "delivered"))
                    {
                        // Gọi hàm cập nhật nếu trạng thái là "delivered"
                        var update = await _logHandler.GetOrderByGHNId(orderDetailRequest.order_code, "completed");
                    }
                    return Ok(new { latestStatuses });
                }

                else
                {
                    // Không có log → fallback dùng created_date và status
                    var fallbackStatus = data["status"]?.ToString();
                    var fallbackDate = data["created_date"]?.ToObject<DateTime?>();

                    if (!string.IsNullOrEmpty(fallbackStatus) && fallbackDate.HasValue)
                    {
                        var fallbackResult = new
                        {
                            status = fallbackStatus,
                            UpdatedDate = fallbackDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                        };

                        if (fallbackStatus == "delivered")
                        {
                            await _logHandler.GetOrderByGHNId(orderDetailRequest.order_code, "Delivered");
                        }

                        return Ok(fallbackResult);
                    }

                    var er = await response.Content.ReadAsStringAsync();
                    return BadRequest(er);
                }

            }
            var errorResponseData = await response.Content.ReadAsStringAsync();
            return BadRequest(errorResponseData);
        }


        //[HttpPost("order-status-newest")]
        //public async Task<IActionResult> GetOrderStatusNewest([FromBody] OrderDetailWithUpdateRequest orderDetailRequest)
        //{
        //    if (orderDetailRequest == null || string.IsNullOrEmpty(orderDetailRequest.order_code))
        //        return BadRequest("Invalid order code.");

        //    var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(orderDetailRequest);
        //    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        //    content.Headers.Add("Token", token);

        //    var response = await _httpClient.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/detail", content);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorResponseData = await response.Content.ReadAsStringAsync();
        //        return BadRequest(errorResponseData);
        //    }

        //    var responseData = await response.Content.ReadAsStringAsync();
        //    var jsonResponse = JObject.Parse(responseData);
        //    var data = jsonResponse["data"];
        //    if (data == null)
        //        return BadRequest("Dữ liệu không tồn tại trong phản hồi từ GHN.");

        //    var systemAccountId = -1; // Account ID hệ thống (hoặc bạn gán giá trị phù hợp)
        //    var logsToken = data["log"];

        //    if (logsToken != null && logsToken.Type == JTokenType.Array)
        //    {
        //        var logs = logsToken.ToObject<List<Application.DTO.Request.LogEntry>>();
        //        var latestLog = logs
        //            .OrderByDescending(log => log.updated_date)
        //            .FirstOrDefault();

        //        if (latestLog != null)
        //        {
        //            var order = await _orderRepository.GetOrderByIdGHNAsync(orderDetailRequest.order_code);

        //            if (latestLog.status == "delivering")
        //            {
        //                await _logHandler.GetOrderByGHNId(orderDetailRequest.order_code, "Delivering");

        //                if (order != null)
        //                {
        //                    try
        //                    {
        //                        await _orderProcessingHelper.LogDeliveringStatusAsync(order.OrderId, systemAccountId);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.WriteLine($"Error logging delivering status: {ex.Message}");
        //                    }
        //                }
        //            }
        //            else if (latestLog.status == "delivered")
        //            {
        //                await _logHandler.GetOrderByGHNId(orderDetailRequest.order_code, "Delivered");

        //                if (order != null)
        //                {
        //                    try
        //                    {
        //                        await _orderProcessingHelper.LogDeliveredStatusAsync(order.OrderId, systemAccountId);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.WriteLine($"Error logging delivered status: {ex.Message}");
        //                    }
        //                }
        //            }

        //            return Ok(new
        //            {
        //                status = latestLog.status,
        //                UpdatedDate = latestLog.updated_date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        //            });
        //        }
        //    }

        //    // Xử lý fallback nếu log bị thiếu
        //    var fallbackStatus = data["status"]?.ToString();
        //    var fallbackDate = data["created_date"]?.ToObject<DateTime?>();

        //    if (!string.IsNullOrEmpty(fallbackStatus) && fallbackDate.HasValue)
        //    {
        //        var order = await _orderRepository.GetOrderByIdGHNAsync(orderDetailRequest.order_code);

        //        if (fallbackStatus == "delivering")
        //        {
        //            await _logHandler.GetOrderByGHNId(orderDetailRequest.order_code, "Delivering");

        //            if (order != null)
        //            {
        //                try
        //                {
        //                    await _orderProcessingHelper.LogDeliveringStatusAsync(order.OrderId, systemAccountId);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"Error logging delivering status: {ex.Message}");
        //                }
        //            }
        //        }
        //        else if (fallbackStatus == "delivered")
        //        {
        //            await _logHandler.GetOrderByGHNId(orderDetailRequest.order_code, "Delivered");

        //            if (order != null)
        //            {
        //                try
        //                {
        //                    await _orderProcessingHelper.LogDeliveredStatusAsync(order.OrderId, systemAccountId);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"Error logging delivered status: {ex.Message}");
        //                }
        //            }
        //        }

        //        return Ok(new
        //        {
        //            status = fallbackStatus,
        //            UpdatedDate = fallbackDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        //        });
        //    }

        //    var fallbackError = await response.Content.ReadAsStringAsync();
        //    return BadRequest(fallbackError);
        //}



        [HttpPost("order-detail")]
        public async Task<IActionResult> GetOrderDetailWithData([FromBody] OrderDetailRequest orderDetailRequest)
        {
            if (orderDetailRequest == null || string.IsNullOrEmpty(orderDetailRequest.order_code))
            {
                return BadRequest("Invalid order code.");
            }

            var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(orderDetailRequest);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            content.Headers.Add("Token", token);
            var response = await _httpClient.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/detail", content);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                // parse JSON ra 
                var jsonResponse = JObject.Parse(responseData);

                // truy vấn từ data xuống mảng log status
                var logs = jsonResponse["data"]["log"].ToObject<List<Domain.DTO.Request.LogEntry>>();

                // Lấy trạng thái mới nhất theo thời gian và sắp xếp
                var latestStatuses = logs
                    .GroupBy(log => log.status)
                    .Select(g => g.OrderByDescending(log => log.updated_date).First())
                    .Select(log => new Domain.DTO.Response.LogEntry // gán giá trị vào log entry
                    {
                        status = log.status,
                        updated_date = log.updated_date
                    })
                    .ToList();

                // Gán từng giá trị vô để call lên FE cho dễ
                var orderDetail = new OrderDetailDtoOrder
                {
                    Items = jsonResponse["data"]["items"].ToObject<List<Domain.DTO.Response.Item>>(),
                    RequiredNote = jsonResponse["data"]["required_note"].ToString(),
                    FromName = jsonResponse["data"]["from_name"].ToString(),
                    FromPhone = jsonResponse["data"]["from_phone"].ToString(),
                    FromAddress = jsonResponse["data"]["from_address"].ToString(),
                    ToName = jsonResponse["data"]["to_name"].ToString(),
                    ToPhone = jsonResponse["data"]["to_phone"].ToString(),
                    ToAddress = jsonResponse["data"]["to_address"].ToString(),
                    LatestStatuses = latestStatuses
                };

                return Ok(new { OrderDetail = orderDetail, responseData });
            }

            var errorResponseData = await response.Content.ReadAsStringAsync();
            return BadRequest(errorResponseData);
        }
    }
}
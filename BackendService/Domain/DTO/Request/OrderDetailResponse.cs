
namespace Domain.DTO.Request
{
    public class LogEntry
    {
        public string status { get; set; }
        public DateTime updated_date { get; set; }
    }

    public class OrderDetails
    {
        public int shop_id { get; set; }
        public int client_id { get; set; }
        public string return_name { get; set; }
        public string return_phone { get; set; }
        public string return_address { get; set; }
        public string return_ward_code { get; set; }
        public int return_district_id { get; set; }
        public string from_name { get; set; }
        public string from_phone { get; set; }
        public string from_address { get; set; }
        public string from_ward_code { get; set; }
        public int from_district_id { get; set; }
        public int deliver_station_id { get; set; }
        public string to_name { get; set; }
        public string to_phone { get; set; }
        public string to_address { get; set; }
        public string to_ward_code { get; set; }
        public int to_district_id { get; set; }
        public int weight { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int converted_weight { get; set; }
        public int service_type_id { get; set; }
        public int service_id { get; set; }
        public int payment_type_id { get; set; }
        public decimal custom_service_fee { get; set; }
        public decimal cod_amount { get; set; }
        public string required_note { get; set; }
        public string content { get; set; }
        public string note { get; set; }
        public string order_code { get; set; }
        public string status { get; set; }
        public List<LogEntry> log { get; set; }
    }

    public class OrderDetailResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<OrderDetails> data { get; set; }
    }
}

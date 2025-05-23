
using Domain.DTO.Response;

namespace Domain.DTO.Request
{
    public class OrderRequest
    {
        public int payment_type_id { get; set; }
        public string note { get; set; }
        public string required_note { get; set; }
        public string return_phone { get; set; }
        public string return_address { get; set; }
        public int? return_district_id { get; set; }
        public string return_ward_code { get; set; }
        public string client_order_code { get; set; }
        public string from_name { get; set; }
        public string from_phone { get; set; }
        public string from_address { get; set; }
        public string from_ward_name { get; set; }
        public string from_district_name { get; set; }
        public string from_province_name { get; set; }
        public string to_name { get; set; }
        public string to_phone { get; set; }
        public string to_address { get; set; }
        public string to_ward_name { get; set; }
        public string to_ward_code { get; set; }
        public string to_district_name { get; set; }
        public string to_province_name { get; set; }
        public int cod_amount { get; set; }
        public string content { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int weight { get; set; }
        public int cod_failed_amount { get; set; }
        public int pick_station_id { get; set; }
        public int? deliver_station_id { get; set; }
        public int insurance_value { get; set; }
        public int service_id
        { get; set; }

        public int service_type_id { get; set; }
        public string coupon { get; set; }
        public long pickup_time { get; set; }
        public int[] pick_shift { get; set; }
        public Item[] items { get; set; }
    }
    public class Item
    {
        public string name { get; set; }
        public string code { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int weight { get; set; }
        public CategoryGHNDTO categorydto { get; set; }
    }
    public class ErrorResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string CodeMessage { get; set; }
    }

    public class CreateOrderResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public OrderData Data { get; set; }
        public string MessageDisplay { get; set; }
        public string CodeMessageValue { get; set; }  // Add this if it exists in the JSON
    }
    public class OrderData
    {
        public string OrderCode { get; set; }
        public string SortCode { get; set; }
        public string TransType { get; set; }
        public string WardEncode { get; set; }
        public string DistrictEncode { get; set; }
        public FeeDetails Fee { get; set; }
        public string TotalFee { get; set; }
        public string ExpectedDeliveryTime { get; set; }
        public string OperationPartner { get; set; }  // Add this if it exists in the JSON
    }
    public class CategoryGHNDTO
    {
        public string level1 { get; set; }
    }
}

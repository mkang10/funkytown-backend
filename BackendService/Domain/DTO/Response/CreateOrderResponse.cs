using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
    public class FeeDetails
    {
        public int MainService { get; set; }
        public int Insurance { get; set; }
        public int CodFee { get; set; }  // Change this to match the JSON key
        public int StationDo { get; set; }
        public int StationPu { get; set; }
        public int Return { get; set; }
        public int R2S { get; set; }
        public int Coupon { get; set; }
        public int CodFailedFee { get; set; }
        // Add any missing properties as needed based on JSON
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
}

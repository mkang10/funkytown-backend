using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
   
        public class Item
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public int Quantity { get; set; }
            public int Length { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string CategoryLevel1 { get; set; }
            public int Weight { get; set; }
            public string Status { get; set; }
            public string ItemOrderCode { get; set; }
        }
        public class OrderDetailDtoOrder
        {
            public string RequiredNote { get; set; }
            public string FromName { get; set; }
            public string FromPhone { get; set; }
            public string FromAddress { get; set; }
            public string ToName { get; set; }
            public string ToPhone { get; set; }
            public string ToAddress { get; set; }
            public List<Item> Items { get; set; }

            public List<LogEntry> LatestStatuses { get; set; }
        }
        public class LogEntry
        {
            public string status { get; set; }
            public DateTime updated_date { get; set; }
        
    }
   
}

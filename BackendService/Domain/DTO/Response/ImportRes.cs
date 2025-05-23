using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Response
{
  
        public class ImportDto
        {
            public int ImportId { get; set; }
            public string? CreatedByName { get; set; }
            public int CreatedBy { get; set; }
            public string Email { get; set; }

            public DateTime? CreatedDate { get; set; }
            public string? Status { get; set; }
            public string? ReferenceNumber { get; set; }
            public decimal? TotalCost { get; set; }
            public DateTime? ApprovedDate { get; set; }
            public DateTime? CompletedDate { get; set; }
            public int? OriginalImportId { get; set; }
            public int? HandleBy { get; set; }
            public string? HandleByName { get; set; }
            public List<ImportDetailDto> ImportDetails { get; set; } = new List<ImportDetailDto>();
        }


        public class ImportDetailDto
        {
            public int ImportDetailId { get; set; }
            public int ImportId { get; set; }
            public int ProductVariantId { get; set; }
            public int Quantity { get; set; }
            public List<ImportStoreDetailDto> ImportStoreDetails { get; set; } = new List<ImportStoreDetailDto>();
        }


        public class ImportStoreDetailDto
        {
            public int ImportDetailId { get; set; }
            public int WareHouseId { get; set; }
            public int ActualReceivedQuantity { get; set; }

            public int AllocatedQuantity { get; set; }
            public string? Status { get; set; }
            public string? Comments { get; set; }
            public int? StaffDetailId { get; set; }
            public int ImportStoreId { get; set; }
        }

    }




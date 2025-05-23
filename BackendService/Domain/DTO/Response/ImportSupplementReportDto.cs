using Domain.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Response
{
    public class ImportSupplementReportDto
    {
        public ImportDto ImportData { get; set; }
        public string ReportFileBase64 { get; set; }
    }
}
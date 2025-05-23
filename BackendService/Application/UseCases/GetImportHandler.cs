using AutoMapper;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class GetImportHandler
    {
        private readonly IImportRepos _importRepos;
        private readonly IMapper _mapper;

        public GetImportHandler(IImportRepos importRepos, IMapper mapper)
        {
            _importRepos = importRepos;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<PaginatedResponseDTO<ImportDto>>> GetAllImportsAsync(ImportFilterDto filter)
        {
            var (imports, totalRecords) = await _importRepos.GetAllImportsAsync(filter, CancellationToken.None);

            // Mapping từ entity sang DTO, bao gồm CreatedBy và TotalCost
            var mappedData = _mapper.Map<List<ImportDto>>(imports);
            var paginatedResult = new PaginatedResponseDTO<ImportDto>(mappedData, totalRecords, filter.Page, filter.PageSize);

            return new ResponseDTO<PaginatedResponseDTO<ImportDto>>(paginatedResult, true, "Data fetched successfully");
        }
    }
}

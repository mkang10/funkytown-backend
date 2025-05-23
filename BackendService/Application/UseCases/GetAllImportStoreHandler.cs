using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTO.Request;

namespace Application.UseCases
{
  
        public class GetAllImportStoreHandler
        {
            private readonly IImportRepos _repository;
            private readonly IMapper _mapper;

            public GetAllImportStoreHandler(IImportRepos repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<ResponseDTO<PaginatedResponseDTO<ImportStoreDetailDtoStore>>> GetStoreExportByStaffDetailAsync(ImportStoreDetailFilterDtO filter)
            {
                var result = await _repository.GetImportStoreDetailByStaffDetailAsync(filter);
                return new ResponseDTO<PaginatedResponseDTO<ImportStoreDetailDtoStore>>(result, true, "Data fetched successfully");
            }
        }
    
}

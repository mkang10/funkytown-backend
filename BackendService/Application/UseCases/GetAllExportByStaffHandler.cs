using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;

using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{

    public class GetAllExportByStaffHandler
    {
        private readonly IDispatchRepos _repository;
        private readonly IMapper _mapper;

        public GetAllExportByStaffHandler(IDispatchRepos repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<PaginatedResponseDTO<StoreExportStoreDetailDto>>> GetStoreExportByStaffDetailAsync(StoreExportStoreDetailFilterDtO filter)
        {
            var result = await _repository.GetStoreExportStoreDetailByStaffDetailAsync(filter);
            return new ResponseDTO<PaginatedResponseDTO<StoreExportStoreDetailDto>>(result, true, "Data fetched successfully");
        }
    }
}

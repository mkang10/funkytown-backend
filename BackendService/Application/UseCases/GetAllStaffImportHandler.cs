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

    public class GetAllStaffImportHandler
    {
        private readonly IImportRepos _repository;
        private readonly IMapper _mapper;

        public GetAllStaffImportHandler(IImportRepos repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<PaginatedResponseDTO<ImportStoreDetailDtoStore>>> GetStoreDetailsByStaffDetailAsync(ImportStoreDetailFilterDto filter)
        {
            var result = await _repository.GetStoreDetailsByStaffDetailAsync(filter);
            return new ResponseDTO<PaginatedResponseDTO<ImportStoreDetailDtoStore>>(result, true, "Data fetched successfully");
        }

    }

}

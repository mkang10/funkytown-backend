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
    public class GetAllImportHandler
    {
        private readonly IImportRepos _repository;
        private readonly IMapper _mapper;

        public GetAllImportHandler(IImportRepos repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PagedResult<InventoryImportResponseDto>> GetInventoryImportsAsync(InventoryImportFilterDto filter)
        {
            var pagedResult = await _repository.GetImportsAsync(filter);
            var dtoList = _mapper.Map<List<InventoryImportResponseDto>>(pagedResult.Data);
            return new PagedResult<InventoryImportResponseDto>
            {
                Data = dtoList,
                TotalCount = pagedResult.TotalCount
            };
        }
    }

}

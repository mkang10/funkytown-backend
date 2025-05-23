using AutoMapper;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetWareHouseHandler
    {
        private readonly IImportRepos _repository;
        private readonly IMapper _mapper;

        public GetWareHouseHandler(IImportRepos repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResponseDTO<Warehouse>> GetAllWareHouse (int page, int pageSize)
        {
            var pagedResult = await _repository.GetAllWarehousesAsync(page, pageSize);
            var dtos = _mapper.Map<List<Warehouse>>(pagedResult.Data);
            return new PaginatedResponseDTO<Warehouse>(dtos, pagedResult.TotalRecords, pagedResult.Page, pagedResult.PageSize);
        }


    }
}

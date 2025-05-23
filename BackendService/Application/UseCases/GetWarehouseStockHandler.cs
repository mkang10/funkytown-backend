using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetWarehouseStockHandler
    {
        private readonly IWareHousesStockRepository _repository;
        private readonly IMapper _mapper;

        public GetWarehouseStockHandler(IWareHousesStockRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<PaginatedResponseDTO<WarehouseStockDto>>> HandleAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var pagedResult = await _repository.GetAllWareHouse(page, pageSize, cancellationToken);
            var dtos = _mapper.Map<List<WarehouseStockDto>>(pagedResult.Data);

            var paginatedResponse = new PaginatedResponseDTO<WarehouseStockDto>(
                dtos,
                pagedResult.TotalRecords,
                pagedResult.Page,
                pagedResult.PageSize
            );

            return new ResponseDTO<PaginatedResponseDTO<WarehouseStockDto>>(
                paginatedResponse,
                status: true,
                message: "Fetched warehouse stocks successfully."
            );
        }
    }
}

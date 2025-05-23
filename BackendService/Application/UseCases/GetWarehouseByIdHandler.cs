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
    public class GetWarehouseByIdHandler
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IMapper _mapper;

        public GetWarehouseByIdHandler(IWarehouseRepository warehouseRepository, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        public async Task<WarehouseResponse?> Handle(int storeId)
        {
            var store = await _warehouseRepository.GetWarehouseByIdAsync(storeId);
            if (store == null) return null;

            // Map sang StoreResponse
            return _mapper.Map<WarehouseResponse>(store);
        }
    }

}

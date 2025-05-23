using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.DTO.Request.StoreExportStoreDetailReq;

namespace Application.UseCases
{
    
        public class GetAllStaffDispatchHandler
        {
            private readonly IDispatchRepos _repository;
            private readonly IMapper _mapper;

            public GetAllStaffDispatchHandler(IDispatchRepos repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

        public Task<PaginatedResponseDTO<ExportDetailDto>> HandleAsync(
    int page,
    int pageSize,
    StoreExportStoreDetailFilterDto filter)
        {
            return _repository.GetAllExportStoreDetailsAsync(page, pageSize, filter);
        }

    }

    }




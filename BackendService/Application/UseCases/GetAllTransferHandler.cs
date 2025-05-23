// Application/UseCases/GetAllTransferHandler.cs
using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetAllTransferHandler
    {
        private readonly ITransferRepos _repository;
        private readonly IMapper _mapper;

        public GetAllTransferHandler(ITransferRepos repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<PaginatedResponseDTO<TransferResponseDto>>> HandleAsync(
            int page,
            int pageSize,
            string? filter,
            CancellationToken cancellationToken = default)
        {
            var paged = await _repository.GetAllWithPagingAsync(page, pageSize, filter, cancellationToken);
            var dtos = _mapper.Map<List<TransferResponseDto>>(paged.Data);

            var paginated = new PaginatedResponseDTO<TransferResponseDto>(dtos, paged.TotalRecords, paged.Page, paged.PageSize);

            return new ResponseDTO<PaginatedResponseDTO<TransferResponseDto>>(
                paginated,
                status: true,
                message: "Fetched transfers successfully."
            );
        }
    }
}

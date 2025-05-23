using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Interfaces;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetAllDispatchHandler
    {
        private readonly IDispatchRepos _repository;
        private readonly IMapper _mapper;

        public GetAllDispatchHandler(IDispatchRepos repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<PaginatedResponseDTO<DispatchResponseDto>>> HandleAsync(int page, int pageSize, DispatchFilterDto filter)
        {
            var result = await _repository.GetAllDispatchAsync(page, pageSize, filter);
            return new ResponseDTO<PaginatedResponseDTO<DispatchResponseDto>>(result, true, "Lấy danh sách dispatch thành công");
        }

    }
}

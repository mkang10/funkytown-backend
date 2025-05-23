using Domain.DTO.Request;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreatePromotionHandler
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;

        public CreatePromotionHandler(IPromotionRepository promotionRepository, IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreatePromotionRequest request)
        {
            var promotion = _mapper.Map<Promotion>(request);
            promotion.ApplyValue = JsonConvert.SerializeObject(request.ApplyValue);
            return await _promotionRepository.CreatePromotionAsync(promotion);
        }
    }

}

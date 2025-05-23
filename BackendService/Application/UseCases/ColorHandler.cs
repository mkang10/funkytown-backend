using AutoMapper;
using Domain.Commons;
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
    public class ColorHandler
    {
        private readonly ISizeAndColorRepository _repository;
        private readonly IMapper _mapper;

        public ColorHandler(ISizeAndColorRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ColorDTO>> GetByColorCode(string name)
        {
            if (name == null) throw new ArgumentNullException("Wrong name!");
            var data = await _repository.GetByCode(name);
            if (data == null) throw new ArgumentNullException("No data!");
            var map = _mapper.Map<List<ColorDTO>>(data);
            return map;
        }
        public async Task<Pagination<ColorDTO>> GetAllColor(PaginationParameter paginationParameter)
        {
            var data = await _repository.GetAllColor(paginationParameter);
            if (!data.Any())
            {
                throw new Exception("No data!");
            }

            var map = _mapper.Map<List<ColorDTO>>(data);
            var paginationResult = new Pagination<ColorDTO>(map,
                data.TotalCount,
                data.CurrentPage,
                data.PageSize);
            return paginationResult;
        }
        public async Task<List<ColorDTO>> CreateOrUpdateColors(List<CreateColorDTO> data)
        {
            var createdColors = new List<ColorDTO>();

            try
            {
                foreach (var colorDto in data)
                {
                    // Kiểm tra xem đã có color với ColorCode chưa
                    var existingColor = await _repository.GetByCodeColor(colorDto.ColorCode);

                    if (existingColor != null)
                    {
                        // Nếu tồn tại thì cập nhật thông tin
                        existingColor.ColorName = colorDto.ColorName;
                        existingColor.ColorCode = colorDto.ColorCode;
                        existingColor.CreatedDate = DateTime.UtcNow;

                        var updatedColor = await _repository.UpdateColor(existingColor);
                        createdColors.Add(_mapper.Map<ColorDTO>(updatedColor));
                    }
                    else
                    {
                        // Nếu chưa có thì tạo mới
                        var newColor = _mapper.Map<Color>(colorDto);
                        newColor.CreatedDate = DateTime.UtcNow;

                        var createdColor = await _repository.CreateColor(newColor);
                        createdColors.Add(_mapper.Map<ColorDTO>(createdColor));

                    }
                }

                return createdColors;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating/updating colors: " + ex.Message);
            }
        }

        public async Task<bool> DeleteColor(string data)
        {
            try
            {
                var dataCheck = await _repository.GetByCodeColor(data);
                if (dataCheck == null)
                {
                    throw new Exception($"Color does not exist");
                }

                await _repository.DeleteColor(dataCheck);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> UpdateColor( CreateColorDTO data)
        {
            try
            {
                var dataCheck = await _repository.GetByCodeColor(data.ColorCode);
                if (data == null)
                {
                    throw new Exception("No data!");
                }

                _mapper.Map(data, dataCheck);

                await _repository.UpdateColor(dataCheck);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }
    }
}

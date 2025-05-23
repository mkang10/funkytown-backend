using AutoMapper;
using Domain.Commons;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class SizeHandler 
    {
        private readonly ISizeAndColorRepository _repository;
        private readonly IMapper _mapper;

        public SizeHandler(ISizeAndColorRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<SizeDTO>> GetBySizeName(string name)
        {
            if (name == null) throw new ArgumentNullException("Wrong name!");
            var data = await _repository.GetBySizeName(name);
            if (data == null) throw new ArgumentNullException("No data!");
            var map = _mapper.Map<List<SizeDTO>>(data);
            return map;
        }
        public async Task<Pagination<SizeDTO>> GetAllSize(PaginationParameter paginationParameter)
        {
            var data = await _repository.GetAllSize(paginationParameter);
            if (!data.Any())
            {
                throw new Exception("No data!");
            }

            var map = _mapper.Map<List<SizeDTO>>(data);
            var paginationResult = new Pagination<SizeDTO>(map,
                data.TotalCount,
                data.CurrentPage,
                data.PageSize);
            return paginationResult;
        }
        public async Task<List<SizeDTO>> CreateSize(List<CreateSizeDTO> data)
        {
            var createdColors = new List<SizeDTO>();

            try
            {
                foreach (var colorDto in data)
                {
                    // Kiểm tra xem đã có color với ColorCode chưa
                    var existingColor = await _repository.GetByName(colorDto.SizeName);

                    if (existingColor != null)
                    {
                        // Nếu tồn tại thì cập nhật thông tin
                        existingColor.SizeName = colorDto.SizeName;
                        existingColor.SizeDescription = colorDto.SizeDescription;
                        existingColor.CreatedDate = DateTime.UtcNow;

                        var updatedColor = await _repository.UpdateSize(existingColor);
                        createdColors.Add(_mapper.Map<SizeDTO>(updatedColor));
                    }
                    else
                    {
                        // Nếu chưa có thì tạo mới
                        var newColor = _mapper.Map<Size>(colorDto);
                        newColor.CreatedDate = DateTime.UtcNow;

                        var createdColor = await _repository.CreateSize(newColor);
                        createdColors.Add(_mapper.Map<SizeDTO>(createdColor));

                    }
                }

                return createdColors;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating/updating sizes: " + ex.Message);
            }
        }
        public async Task<bool> DeleteSize(string data)
        {
            try
            {
                var dataCheck = await _repository.GetByName(data);
                if (dataCheck == null)
                {
                    throw new Exception($"Size does not exist");
                }

                await _repository.DeleteSize(dataCheck);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> UpdateSize( CreateSizeDTO data)
        {
            try
            {
                var dataCheck = await _repository.GetByName(data.SizeName);
                if (data == null)
                {
                    throw new Exception("No data!");
                }

                _mapper.Map(data, dataCheck);

                await _repository.UpdateSize(dataCheck);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }
    }
}

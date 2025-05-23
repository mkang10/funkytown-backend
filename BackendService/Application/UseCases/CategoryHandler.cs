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
    public class CategoryHandler
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;

        public CategoryHandler(ICategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<List<CategoryDTO>> GetByName(string name)
        {
            if (name == null) throw new ArgumentNullException("Wrong name!");
            var data = await _repository.GetByCategoryName(name);
            if (data == null) throw new ArgumentNullException("No data!");
            var map = _mapper.Map<List<CategoryDTO>>(data);
            return map;
        }
        public async Task<Pagination<CategoryDTO>> GetAllCategoryHandler(PaginationParameter paginationParameter)
        {
            var data = await _repository.GetAllCategory(paginationParameter);
            if (!data.Any())
            {
                throw new Exception("No data!");
            }

            var map = _mapper.Map<List<CategoryDTO>>(data);
            var paginationResult = new Pagination<CategoryDTO>(map,
                data.TotalCount,
                data.CurrentPage,
                data.PageSize);
            return paginationResult;
        }
        public async Task<List<CategoryDTO>> CreateOrUpdateCategory(List<CreateCategoryDTO> data)
        {
            var createdColors = new List<CategoryDTO>();

            try
            {
                foreach (var colorDto in data)
                {
                    // Kiểm tra xem đã có color với ColorCode chưa
                    var existingColor = await _repository.GetByName(colorDto.Name);

                    if (existingColor != null)
                    {
                        // Nếu tồn tại thì cập nhật thông tin
                        existingColor.Name = colorDto.Name;
                        existingColor.Description = colorDto.Description;
                        existingColor.DisplayOrder = colorDto.DisplayOrder;

                        var updatedColor = await _repository.Update(existingColor);
                        createdColors.Add(_mapper.Map<CategoryDTO>(updatedColor));
                    }
                    else
                    {
                        // Nếu chưa có thì tạo mới
                        var newColor = _mapper.Map<Category>(colorDto);

                        var createdColor = await _repository.Create(newColor);
                        createdColors.Add(_mapper.Map<CategoryDTO>(createdColor));

                    }
                }

                return createdColors;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating/updating category: " + ex.Message);
            }
        }

        public async Task<bool> DeleteColor(int data)
        {
            try
            {
                var dataCheck = await _repository.GetById(data);
                if (dataCheck == null)
                {
                    throw new Exception($"Category does not exist");
                }

                await _repository.Delete(dataCheck);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> UpdateColor(int id,CreateCategoryDTO data)
        {
            try
            {
                var dataCheck = await _repository.GetById(id);
                if (data == null)
                {
                    throw new Exception("No data!");
                }

                _mapper.Map(data, dataCheck);

                await _repository.Update(dataCheck);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }
    }
}

using Domain.Commons;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISizeAndColorRepository
    {
        public Task<List<Color>> GetByCode(string id);
        public Task<Color> GetByCodeColor(string id);
        public Task<Pagination<Color>> GetAllColor(PaginationParameter paginationParameter);
        public Task<Color> CreateColor(Color data);
        public Task<bool> DeleteColor(Color data);
        public Task<Color> UpdateColor(Color data);

        public Task<List<Size>> GetBySizeName(string name);
        public Task<Size> GetByName(string name);
        public Task<Pagination<Size>> GetAllSize(PaginationParameter paginationParameter);
        public Task<Size> CreateSize(Size data);
        public Task<bool> DeleteSize(Size data);
        public Task<Size> UpdateSize(Size data);

    }
}

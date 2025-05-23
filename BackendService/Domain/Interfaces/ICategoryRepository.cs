using Domain.Commons;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<List<Category>> GetByCategoryName(string name);
        public Task<Category> GetById(int id);
        public Task<Category> GetByName(string id);

        public Task<Pagination<Category>> GetAllCategory(PaginationParameter paginationParameter);
        public Task<Category> Create(Category data);
        public Task<bool> Delete(Category data);
        public Task<Category> Update(Category data);
    }
}

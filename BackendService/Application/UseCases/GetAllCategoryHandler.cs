//using Domain.Entities;
//using Domain.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Application.UseCases.GetAllCategoryHandler;

//namespace Application.UseCases
//{

//    public class GetAllCategoryHandler
//    {
//        private readonly ICategoryRepository _repo;

//        public GetAllCategoryHandler(ICategoryRepository repo)
//        {
//            _repo = repo;
//        }

//        public async Task<IEnumerable<Category>> GetAllAsync()
//        {
//            var categories = await _repo.GetAllAsync();

//            return categories;

//        }
//    }

//}

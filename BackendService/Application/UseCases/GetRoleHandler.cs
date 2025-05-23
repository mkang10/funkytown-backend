//using Application.Interfaces;
//using AutoMapper;
//using Domain.Entities;
//using Domain.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Application.UseCases
//{
//    public class GetRoleHandler : IRoleService
//    {
//        private readonly IUserManagementRepository _repo;
//        private readonly IMapper _mapper;

//        public GetRoleHandler(IUserManagementRepository repo, IMapper mapper)
//        {
//            _repo = repo;
//            _mapper = mapper;
//        }

//        public async Task<RoleCreateRequestDTO> Create(RoleCreateRequestDTO role)
//        {
//            try
//            {
//                var map = _mapper.Map<Role>(role);
//                var userCreate = await _repo.CreateRole(map);
//                var resutl = _mapper.Map<RoleCreateRequestDTO>(userCreate);
//                return resutl;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }

//        public async Task<bool> Delete(int id)
//        {
//            try
//            {
//                var role = await _repo.GetRoleById(id);
//                if (role == null)
//                {
//                    throw new Exception($"Role {id} does not exist");
//                }

//                await _repo.DeleteRole(role);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }

//        public async Task<List<RoleRequestDTO>> GetAll()
//        {
//            try
//            {

//                var data = await _repo.GetAllRole();
//                var map = _mapper.Map<List<RoleRequestDTO>>(data);
//                return map;

//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }

//        public async Task<bool> Update(int id, RoleRequestDTO role)
//        {
//            try
//            {
//                var roleData = await _repo.GetRoleById(id);
//                if (roleData == null)
//                {
//                    return false;
//                }

//                _mapper.Map(role, roleData);
//                await _repo.UpdateRole(roleData);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Fail to update role {ex.Message} !");
//                return false;
//            }
//        }
//    }
//}

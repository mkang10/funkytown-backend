using Application.Interfaces;
using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
	public class GetCustomerProfileHandler
	{
		private readonly ICustomerProfileDataService _customerProfileDataService;
		private readonly IMapper _mapper;

		public GetCustomerProfileHandler(ICustomerProfileDataService customerProfileDataService, IMapper mapper)
		{
			_customerProfileDataService = customerProfileDataService;
			_mapper = mapper;
		}

		public async Task<CustomerProfileResponse?> GetCustomerProfile(int accountId)
		{
			var (account, customerDetail) = await _customerProfileDataService.GetAccountAndDetailAsync(accountId);
			if (account == null || customerDetail == null)
			{
				return null;
			}

			var response = _mapper.Map<CustomerProfileResponse>(account);
			_mapper.Map(customerDetail, response);
			return response;
		}
	}

}

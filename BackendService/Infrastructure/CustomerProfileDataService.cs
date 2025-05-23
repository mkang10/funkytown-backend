using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
	public class CustomerProfileDataService : ICustomerProfileDataService
	{
		private readonly IProfileRepository _editProfileRepository;

		public CustomerProfileDataService(IProfileRepository editProfileRepository)
		{
			_editProfileRepository = editProfileRepository;
		}

		public async Task<(Account? Account, CustomerDetail? CustomerDetail)> GetAccountAndDetailAsync(int accountId)
		{
			var account = await _editProfileRepository.GetAccountByIdAsync(accountId);
			var customerDetail = await _editProfileRepository.GetCustomerDetailByAccountIdAsync(accountId);
			return (account, customerDetail);
		}
	}

}

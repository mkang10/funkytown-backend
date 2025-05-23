using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
	public interface ICustomerProfileDataService
	{
		Task<(Account? Account, CustomerDetail? CustomerDetail)> GetAccountAndDetailAsync(int accountId);
	}

}

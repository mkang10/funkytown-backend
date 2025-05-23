using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Application.Interfaces;

namespace Infrastructure.HelperServices
{
    public class OrderAutoCompletionService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OrderAutoCompletionService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var orderAutoCompletionHandler = scope.ServiceProvider.GetRequiredService<IOrderAutoCompletionHandler>();

                try
                {
                    await orderAutoCompletionHandler.ProcessAutoCompleteOrdersAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while auto completing orders: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken); // Kiểm tra mỗi 1 tiếng
            }
        }
    }
}

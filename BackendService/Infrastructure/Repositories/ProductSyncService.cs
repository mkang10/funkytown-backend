//using Application.DTO.Response;
//using Domain.Entities;
//using Microsoft.EntityFrameworkCore;
//using Nest;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Infrastructure
//{
//    public class ProductSyncService
//    {
//        private readonly FtownContext _dbContext;
//        private readonly IElasticClient _elasticClient;

//        public ProductSyncService(FtownContext dbContext, IElasticClient elasticClient)
//        {
//            _dbContext = dbContext;
//            _elasticClient = elasticClient;
//        }

//        public async Task SyncProductsToElasticsearch()
//        {
//            var products = await _dbContext.Products
//                .Include(p => p.ProductVariants)
//                .Include(p => p.Category)
//                .ToListAsync();

//            if (products.Count == 0)
//            {
//                Console.WriteLine("⚠️ Không có sản phẩm nào trong database để đồng bộ!");
//                return;
//            }

//            foreach (var product in products)
//            {
//                var response = await _elasticClient.IndexDocumentAsync(product);

//                if (response.IsValid)
//                {
//                    Console.WriteLine($"✅ Sản phẩm {product.ProductId} đã được đồng bộ lên Elasticsearch!");
//                }
//                else
//                {
//                    Console.WriteLine($"❌ Lỗi khi đồng bộ sản phẩm {product.ProductId}:");
//                    Console.WriteLine($"Status Code: {response.ApiCall.HttpStatusCode}");
//                    Console.WriteLine($"Lỗi chi tiết: {response.DebugInformation}");
//                }
//            }
//        }

//    }
//}

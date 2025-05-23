//using Application.DTO.Response;
//using Microsoft.Extensions.Configuration;
//using Nest;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Infrastructure
//{
//    public class ElasticsearchService
//    {
//        private readonly IElasticClient _elasticClient;
//        private readonly string _indexName;

//        public ElasticsearchService(IConfiguration configuration)
//        {
//            var settings = new ConnectionSettings(new Uri(configuration["Elasticsearch:Url"]))
//                .DefaultIndex(configuration["Elasticsearch:IndexName"]);

//            _elasticClient = new ElasticClient(settings);
//            _indexName = configuration["Elasticsearch:IndexName"];
//        }

//        public async Task<List<ProductListResponse>> SearchProductsAsync(string query)
//        {
//            var searchResponse = await _elasticClient.SearchAsync<ProductListResponse>(s => s
//                .Index(_indexName)
//                .Query(q => q
//                    .Match(m => m
//                        .Field(f => f.Name)
//                        .Query(query)
//                    )
//                )
//            );

//            return searchResponse.Documents.ToList();
//        }

//        public async Task IndexProductAsync(ProductListResponse product)
//        {
//            await _elasticClient.IndexDocumentAsync(product);
//        }
//    }
//}

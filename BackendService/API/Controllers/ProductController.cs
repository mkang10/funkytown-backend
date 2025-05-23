using Application.Interfaces;
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly GetAllProductsHandler _getAllProductsHandler;
        private readonly GetProductDetailHandler _getProductDetailHandler;
        private readonly GetProductVariantByIdHandler _getProductVariantByIdHandler;
        private readonly GetAllProductVariantsByIdsHandler _getAllProductVariantsByIdsHandler;
        private readonly GetProductVariantByDetailsHandler _getProductVariantByDetailsHandler;
        private readonly FilterProductHandler _filterProductHandler;
        private readonly GetTopSellingProductHandler _getTopSellingProductHandler;
        private readonly GetProductsByStyleHandler _getProductsByStyleHandler;
        private readonly CreateProductHandler _createProductHandler;
        private readonly GetAllProductHandler _getallProductHandler;
        private readonly GetProductDetailHandler _detailHandler;
        private readonly EditProductHandler _editProductHandler;
        private readonly GetVariantHandler _getVariantHandler;
        private readonly EditVariantHandler _editVariantHandler;
        private readonly RedisHandler _redisHandler;
        public ProductController(IInventoryServiceClient inventoryServiceClient,
            GetAllProductsHandler getAllProductsHandler,
            GetProductDetailHandler getProductDetailHandler,
            GetProductVariantByIdHandler getProductVariantByIdHandler,
            GetAllProductVariantsByIdsHandler getAllProductVariantsByIdsHandler,
            GetProductVariantByDetailsHandler getAllProductVariantByDetailsHandler,
            FilterProductHandler filterProductHandler,
            GetTopSellingProductHandler getTopSellingProductHandler,
            GetProductsByStyleHandler getProductsByStyleHandler,
            RedisHandler redisHandler, EditVariantHandler editVariantHandler, GetVariantHandler getVariantHandler, EditProductHandler editProductHandler, GetProductDetailHandler detailHandler, CreateProductHandler createProductHandler, GetAllProductHandler getAllProductHandler)
        {
            _inventoryServiceClient = inventoryServiceClient;
            _getAllProductsHandler = getAllProductsHandler;
            _getProductDetailHandler = getProductDetailHandler;
            _getProductVariantByIdHandler = getProductVariantByIdHandler;
            _getAllProductVariantsByIdsHandler = getAllProductVariantsByIdsHandler;
            _getProductVariantByDetailsHandler = getAllProductVariantByDetailsHandler;
            _filterProductHandler = filterProductHandler;
            _getTopSellingProductHandler = getTopSellingProductHandler;
            _getProductsByStyleHandler = getProductsByStyleHandler;

            _createProductHandler = createProductHandler;
            _getallProductHandler = getAllProductHandler;
            _detailHandler = detailHandler;
            _editProductHandler = editProductHandler;
            _getVariantHandler = getVariantHandler;
            _editVariantHandler = editVariantHandler;
            _redisHandler = redisHandler;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ sản phẩm từ InventoryService
        /// </summary>
        [HttpGet("view-all")]
        public async Task<ActionResult<ResponseDTO<List<ProductListResponse>>>> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var products = await _getAllProductsHandler.Handle(page, pageSize);

            if (products == null || !products.Any())
                return NotFound(new ResponseDTO<List<ProductListResponse>>(null, false, "Không có sản phẩm nào được tìm thấy."));

            return Ok(new ResponseDTO<List<ProductListResponse>>(products, true, "Lấy danh sách sản phẩm thành công!"));
        }
        [HttpGet("filter-by-category")]
        public async Task<ActionResult<ResponseDTO<List<ProductListResponse>>>> FilterProductsByCategory([FromQuery] string categoryName)
        {
            var products = await _filterProductHandler.Handle(categoryName);

            if (products == null || !products.Any())
                return NotFound(new ResponseDTO<List<ProductListResponse>>(null, false, "Không có sản phẩm nào thuộc danh mục này."));

            return Ok(new ResponseDTO<List<ProductListResponse>>(products, true, "Lọc sản phẩm theo danh mục thành công!"));
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<ResponseDTO<ProductDetailResponseInven>>> GetProductDetail(int productId, [FromQuery] int? accountId)
        {
            var product = await _getProductDetailHandler.Handle(productId, accountId);

            if (product == null)
                return NotFound(new ResponseDTO<ProductDetailResponseInven>(null, false, "Không tìm thấy sản phẩm!"));

            return Ok(new ResponseDTO<ProductDetailResponseInven>(product, true, "Lấy chi tiết sản phẩm thành công!"));
        }



        [HttpGet("variant/{variantId}")]
        public async Task<ActionResult<ResponseDTO<ProductVariantResponse>>> GetProductVariantById(int variantId)
        {
            var variant = await _getProductVariantByIdHandler.Handle(variantId);

            if (variant == null)
                return NotFound(new ResponseDTO<ProductVariantResponse>(null, false, "Không tìm thấy biến thể sản phẩm."));

            return Ok(new ResponseDTO<ProductVariantResponse>(variant, true, "Lấy biến thể sản phẩm thành công!"));
        }

        [HttpPost("variants/details")]
        public async Task<ActionResult<ResponseDTO<List<ProductVariantResponse>>>> GetAllProductVariantsByIdsAsync([FromBody] List<int> variantIds)
        {
            var variants = await _getAllProductVariantsByIdsHandler.Handle(variantIds);

            if (variants == null || variants.Count == 0)
                return NotFound(new ResponseDTO<List<ProductVariantResponse>>(null, false, "Không tìm thấy biến thể sản phẩm nào."));

            return Ok(new ResponseDTO<List<ProductVariantResponse>>(variants, true, "Lấy danh sách biến thể sản phẩm thành công!"));
        }

        [HttpGet("variant/details")]
        public async Task<ActionResult<ResponseDTO<ProductVariantResponse>>> GetProductVariantByDetails([FromQuery] GetProductVariantByDetailsRequest request)
        {
            var variant = await _getProductVariantByDetailsHandler.HandleAsync(request);

            if (variant == null)
                return NotFound(new ResponseDTO<ProductVariantResponse>(null, false, "Không tìm thấy biến thể sản phẩm."));

            return Ok(new ResponseDTO<ProductVariantResponse>(variant, true, "Lấy biến thể sản phẩm thành công!"));
        }
        [HttpGet("by-style")]
        public async Task<IActionResult> GetProductsByStyleName([FromQuery] string styleName, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(styleName))
            {
                return BadRequest(new ResponseDTO(false, "StyleName không được bỏ trống."));
            }

            var result = await _getProductsByStyleHandler.HandleAsync(styleName, page, pageSize);

            return Ok(new ResponseDTO<List<ProductListResponse>>(result, true, "Lấy danh sách sản phẩm theo style thành công."));
        }
       
        [HttpGet("top-selling-products")]
        public async Task<ActionResult<ResponseDTO<List<TopSellingProductResponse>>>> GetTopSellingProducts(
                                        [FromQuery] DateTime? from,
                                        [FromQuery] DateTime? to,
                                        [FromQuery] int top = 10)
        {
            var result = await _getTopSellingProductHandler.GetTopSellingProductsAsync(from, to, top);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDto dto)
        {
            try
            {
                int productId = await _createProductHandler.CreateProductAsync(dto);
                var response = new ResponseDTO<int>(
                    productId,
                    true,
                    "Product created successfully."
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log exception nếu cần
                var response = new ResponseDTO<int>(
                    0,
                    false,
                    $"An error occurred while creating the product: {ex.Message}"
                );
                return StatusCode(500, response);
            }
        }

        // POST: api/Products/variant
        [HttpPost("variant")]
        public async Task<IActionResult> CreateVariant([FromForm] ProductVariantCreateDto dto)
        {
            try
            {
                int variantId = await _createProductHandler.CreateVariantAsync(dto);
                var response = new ResponseDTO<int>(
                    variantId,
                    true,
                    "Product variant created successfully."
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseDTO<int>(
                    0,
                    false,
                    $"An error occurred while creating the product variant: {ex.Message}"
                );
                return StatusCode(500, response);
            }
        }

        [HttpGet("{productId}-with-variants")]
        public async Task<IActionResult> GetProductWithVariants(int productId)
        {
            var result = await _detailHandler.GetProductWithVariantsAsync(productId);

            if (!result.Status)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("variant-detail")]
        public async Task<IActionResult> GetDetail(int variantId)
        {
            var response = await _getVariantHandler.GetProductVariantDetailAsync(variantId);
            if (!response.Status)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponseDTO<ProductDto>>>
            GetAll(
                [FromQuery] string? name,
                [FromQuery] string? description,
                [FromQuery] int? category,
                [FromQuery] string? origin,
                [FromQuery] string? model,
                [FromQuery] string? occasion,
                [FromQuery] string? style,
                [FromQuery] string? material,
                [FromQuery] string? status,
                                [FromQuery] string? skuFilter,

                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
        {
            var result = await _getallProductHandler.GetAllProductsAsync(
                name,
                description,
                category,
                origin,
                model,
                occasion,
                style,
                material,
                status,
                skuFilter,
                page,
                pageSize);
            return Ok(result);
        }

        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Edit([FromForm] EditProductVariantDto dto)
        {
            try
            {
                // 1. Thực hiện cập nhật variant
                var result = await _editVariantHandler.EditProductVariantAsync(dto);
                if (!result.Status)
                {
                    // Trả về ResponseDTO nếu không tìm thấy hoặc cập nhật thất bại
                    var notFoundResponse = new ResponseDTO<string>(
                        data: null,
                        status: false,
                        message: result.Message
                    );
                    return NotFound(notFoundResponse);
                }

                // 2. Nếu thành công, clear cache
                string instanceName = "ProductInstance";  // 👈 Đặt InstanceName của bạn tại đây
                var cacheMessage = await _redisHandler.ClearInstanceCacheAsync(instanceName);

                // 3. Trả về ResponseDTO<string>
                var response = new ResponseDTO<string>(
                    data: cacheMessage,
                    status: true,
                    message: "Đã cập nhật biến thể thành công !"
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(
                    data: null,
                    status: false,
                    message: $"Đã xảy ra lỗi: {ex.Message}"
                );
                return StatusCode(500, errorResponse);
            }
        }



        [HttpGet("color")]
        public async Task<IActionResult> GetColors()
        {
            var result = await _editVariantHandler.GetAllColorsByProductAsync();
            return Ok(result);
        }

        [HttpGet("size")]
        public async Task<IActionResult> GetSizes()
        {
            var result = await _editVariantHandler.GetAllSizesByProductAsync();
            return Ok(result);
        }


        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Edit(int id, [FromForm] ProductEditDto dto)
        {
            try
            {
                // 1. Cập nhật product
                await _editProductHandler.EditAsync(id, dto);

                // 2. Nếu thành công, clear cache
                string instanceName = "ProductInstance";
                var resultMessage = await _redisHandler.ClearInstanceCacheAsync(instanceName);

                // 3. Trả về ResponseDTO<string>
                var response = new ResponseDTO<string>(
                    data: resultMessage,
                    status: true,
                    message: "Đã cập nhật sản phẩm thành công !"
                );
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                var notFoundResponse = new ResponseDTO<string>(
                    data: null,
                    status: false,
                    message: $"Product with id={id} not found"
                );
                return NotFound(notFoundResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(
                    data: null,
                    status: false,
                    message: $"An error occurred: {ex.Message}"
                );
                return StatusCode(500, errorResponse);
            }
        }




    }
}

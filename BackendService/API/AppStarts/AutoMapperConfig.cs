
using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Entities;
using Infrastructure.HelperServices.Models;
using Newtonsoft.Json;
using static Domain.DTO.Response.OrderAssigmentRes;
using static Domain.DTO.Response.OrderDoneRes;
using static Domain.DTO.Response.ProductDetailDTO;

namespace API.AppStarts
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
        //Customerservice
            // Mapping từ Account -> CustomerProfileResponse
            CreateMap<Account, CustomerProfileResponse>()
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImagePath))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.LastLoginDate))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // Mapping từ CustomerDetail -> CustomerProfileResponse
            CreateMap<CustomerDetail, CustomerProfileResponse>()
                .ForMember(dest => dest.LoyaltyPoints, opt => opt.MapFrom(src => src.LoyaltyPoints))
                .ForMember(dest => dest.MembershipLevel, opt => opt.MapFrom(src => src.MembershipLevel))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => src.CustomerType))
                .ForMember(dest => dest.PreferredPaymentMethod, opt => opt.MapFrom(src => src.PreferredPaymentMethod));
            // Map từ EditProfileRequest -> Account
            CreateMap<EditProfileRequest, Account>()
                .ForMember(dest => dest.AccountId, opt => opt.Ignore()); // Tránh ghi đè ID

            // Map từ EditProfileRequest -> CustomerDetail
            CreateMap<EditProfileRequest, CustomerDetail>()
                .ForMember(dest => dest.AccountId, opt => opt.Ignore()) // Tránh ghi đè ID
                .ForMember(dest => dest.LoyaltyPoints, opt => opt.Ignore()) // Nếu LoyaltyPoints không cập nhật
                .ForMember(dest => dest.MembershipLevel, opt => opt.Ignore()); // Nếu MembershipLevel không cập nhật
            // Chuyển đổi Entity → DTO
            CreateMap<CartItem, CartItemResponse>()
                .ForMember(dest => dest.Price, opt => opt.Ignore()); // Lấy giá từ DB sau

            CreateMap<ShoppingCart, ShoppingCartResponseDto>();

            // Chuyển đổi DTO → Entity
            CreateMap<AddToCartRequest, CartItem>();

            //mapping feedback 
            CreateMap<CreateFeedBackRequestDTO, Feedback>()
           .ForMember(dest => dest.FeedbackId, opt => opt.Ignore());
            //update feedback
            CreateMap<UpdateFeedbackRequestDTO, Feedback>()
           .ForMember(dest => dest.ProductId, opt => opt.Ignore());
            //feedback reverse
            CreateMap<Feedback, CreateFeedBackRequestDTO>()
                                .ForMember(dest => dest.ImgFile, opt => opt.Ignore())
.ReverseMap();
            CreateMap<Feedback, CreateFeedBackArrayRequestDTO>()
                                .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
.ReverseMap();
            CreateMap<Feedback, FeedbackRequestDTO>()
                     .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product.Name))
                     .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.Account.FullName)).ReverseMap();

            //mapping Reply request
            CreateMap<ReplyFeedback, ReplyRequestDTO>()
                .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.Account.FullName)).ReverseMap();
            CreateMap<ReplyFeedback, CreateReplyRequestDTO>().ReverseMap();
            CreateMap<ReplyFeedback, UpdateReplyRequestDTO>().ReverseMap();
            CreateMap<Order, UpdateOrderStatusDTO>().ReverseMap();

            CreateMap<ProductResponse, SuggestedProductResponse>();
            CreateMap<Style, StyleResponse>();
            //inventory
            //Exxcel


            CreateMap<Product, ProductListResponse>()
               .ForMember(dest => dest.CategoryName,
                   opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Uncategorized"))
               .ForMember(dest => dest.Price,
                   opt => opt.MapFrom(src => src.ProductVariants.Any() ? src.ProductVariants.First().Price : 0))
               .ForMember(dest => dest.ImagePath,
                   opt => opt.MapFrom(src => src.ProductImages
                       .Where(pi => pi.IsMain)
                       .Select(pi => pi.ImagePath)
                       .FirstOrDefault()))
               .ForMember(dest => dest.Colors,
                   opt => opt.MapFrom(src =>
                       src.ProductVariants
                           .Where(pv => pv.Color != null && !string.IsNullOrEmpty(pv.Color.ColorCode))
                           .Select(pv => pv.Color.ColorCode)
                           .Distinct()
                           .ToList()
                   ));


            CreateMap<Color, ColorInfo>();

            
            CreateMap<Product, ProductDetailResponseInven>()
                 .ForMember(dest => dest.CategoryName,
                            opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Uncategorized"))
                 .ForMember(dest => dest.Variants, opt => opt.Ignore())
                 .ForMember(dest => dest.ImagePath, // Ảnh chính
                            opt => opt.MapFrom(src => src.ProductImages
                                                       .Where(i => i.IsMain)
                                                       .Select(i => i.ImagePath)
                                                       .FirstOrDefault()))

                 .ForMember(dest => dest.ImagePaths, // Danh sách ảnh
                            opt => opt.MapFrom(src => src.ProductImages
                                                       .Select(i => i.ImagePath)
                                                       .ToList()));

            // Mapping từ ProductVariant -> ProductVariantResponse
            CreateMap<ProductVariant, ProductVariantResponse>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size != null ? src.Size.SizeName : null))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color != null ? src.Color.ColorCode : null))
            .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src =>
                src.WareHousesStocks
                    .Where(ws => ws.WareHouseId == 2) // ✅ Lọc WarehouseId = 2
                    .Sum(ws => ws.StockQuantity))); // ✅ Tính tổng số lượng;
            CreateMap<Warehouse, WarehouseResponse>();

            CreateMap<WarehouseRequest, Warehouse>()
                .ForMember(dest => dest.WarehouseId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());
            CreateMap<ProductVariantRequest, ProductVariant>().ReverseMap();

            CreateMap<Product, TopSellingProductResponse>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ProductImages
                                                       .Where(i => i.IsMain)
                                                       .Select(i => i.ImagePath)
                                                       .FirstOrDefault()))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.Price, opt => opt.Ignore()) // sẽ set thủ công từ variant
            .ForMember(dest => dest.DiscountedPrice, opt => opt.Ignore())
            .ForMember(dest => dest.QuantitySold, opt => opt.Ignore())
            .ForMember(dest => dest.Revenue, opt => opt.Ignore())
            .ForMember(dest => dest.PromotionTitle, opt => opt.Ignore())
            .ForMember(dest => dest.Colors,
                        opt => opt.MapFrom(src =>
                            src.ProductVariants
                                .Where(pv => pv.Color != null && !string.IsNullOrEmpty(pv.Color.ColorCode))
                                .Select(pv => pv.Color.ColorCode!)
                                .Distinct()
                                .ToList()
                        ));
            CreateMap<ColorDTO, Color>().ReverseMap();
            CreateMap<CreateColorDTO, Color>().ReverseMap();
            CreateMap<SizeDTO, Size>().ReverseMap();
            CreateMap<CreateSizeDTO, Size>().ReverseMap();
            CreateMap<CategoryDTO, Category>().ReverseMap();
            CreateMap<CreateCategoryDTO, Category>().ReverseMap();

            //order service
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.OrderTotal ?? 0))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderDetails));

            CreateMap<OrderDetail, OrderItemResponse>()
                .ForMember(dest => dest.OrderDetailId, opt => opt.MapFrom(src => src.OrderDetailId))
                .ForMember(dest => dest.ProductVariantId, opt => opt.MapFrom(src => src.ProductVariantId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.PriceAtPurchase, opt => opt.MapFrom(src => src.PriceAtPurchase));

            CreateMap<ShippingAddress, Order>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.District))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.RecipientName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.RecipientPhone));
            CreateMap<PayOSCreateResult, CreatePaymentResponse>();

            CreateMap<OrderItemResponse, ReturnItemResponse>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.PriceAtPurchase)); // Map giá lúc mua

            CreateMap<Order, InvoiceForEmailDTO>()
                .ForMember(dest => dest.OrderdetailEmail, opt => opt.MapFrom(src => src.OrderDetails)).ReverseMap();
            CreateMap<OrderDetail, OrderDetailEmailDTO>()
    .ForMember(dest => dest.Item, opt => opt.MapFrom(src => src.ProductVariant))
    .ReverseMap();
            CreateMap<ProductVariant, ProductDetailEmailDTO>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.SizeId, opt => opt.MapFrom(src => src.Size.SizeName))
                .ForMember(dest => dest.ColorId, opt => opt.MapFrom(src => src.Color.ColorName)).ReverseMap();
            CreateMap<UpdateGHNIdDTO, Order>().ReverseMap();
            CreateMap<UpdateShippingAddressRequest, ShippingAddress>()
                .ForMember(dest => dest.AddressId, opt => opt.Ignore())
                .ForMember(dest => dest.AccountId, opt => opt.Ignore());
            CreateMap<CreateShippingAddressRequest, ShippingAddress>();
            CreateMap<ShippingAddress, ShippingAddressResponse>();

            CreateMap<ReturnOrderItem, ReturnItemResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.Color, opt => opt.Ignore())
                .ForMember(dest => dest.Size, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore());
            CreateMap<ReturnOrder, ReturnOrderInfo>()
            .ForMember(dest => dest.AccountName,
                opt => opt.Ignore()); // Vì cần lấy từ OrderDetail

            CreateMap<ReturnOrder, ReturnOrderDetailInfo>()
                .ForMember(dest => dest.ReturnImages,
                    opt => opt.Ignore()); // Parse thủ công từ JSON

            CreateMap<ReturnOrderItem, ReturnOrderItemInfo>()
                .ForMember(dest => dest.ProductVariantName, opt => opt.Ignore())
                .ForMember(dest => dest.Size, opt => opt.Ignore())
                .ForMember(dest => dest.Color, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ForMember(dest => dest.PriceAtPurchase, opt => opt.Ignore())
                .ForMember(dest => dest.ShippingCost, opt => opt.Ignore());
            //chatbot
            CreateMap<Conversation, ConversationCreateRequest>().ReverseMap();
            CreateMap<Conversation, ConversationRequest>().ReverseMap();

            CreateMap<Message, MessageCreateRequest>().ReverseMap();
            CreateMap<Message, MessageRequest>().ReverseMap();
            CreateMap<Message, UpdateStatusIsReadMessageDTO>().ReverseMap();

            //promotion
            // CreatePromotionRequest -> Promotion (Entity)
            CreateMap<CreatePromotionRequest, Promotion>()
                .ForMember(dest => dest.ApplyValue, opt => opt.MapFrom(src =>
                    src.ApplyValue != null ? JsonConvert.SerializeObject(src.ApplyValue) : null));

            // UpdatePromotionRequest -> Promotion (Entity)
            CreateMap<UpdatePromotionRequest, Promotion>()
                .ForMember(dest => dest.ApplyValue, opt => opt.MapFrom(src =>
                    src.ApplyValue != null ? JsonConvert.SerializeObject(src.ApplyValue) : null));

            // Promotion (Entity) -> PromotionResponse
            CreateMap<Promotion, PromotionResponse>()
                .ForMember(dest => dest.ApplyValue, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.ApplyValue)
                    ? JsonConvert.DeserializeObject<List<int>>(src.ApplyValue)
                    : new List<int>()));

            // Promotion (Entity) -> PromotionListResponse
            CreateMap<Promotion, PromotionListResponse>();

            //owner
            CreateMap<Import, ImportDto>();
            // Map từ ImportDetail sang ImportDetailDto
            CreateMap<ImportDetail, ImportDetailDto>();
            // Nếu có mapping cho StoreDetail cũng cần khai báo
            CreateMap<ImportStoreDetail, ImportStoreDetailDto>();

            //CreateMap<Account, AccountDTO>();
            CreateMap<Import, InventoryImportResponseDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
            .ForMember(dest => dest.CreatedByEmail, opt => opt.MapFrom(src => src.CreatedByNavigation.Email))
            .ForMember(dest => dest.CreatedByPhone, opt => opt.MapFrom(src => src.CreatedByNavigation.PhoneNumber))
            .ForMember(dest => dest.CreatedByAddress, opt => opt.MapFrom(src => src.CreatedByNavigation.Address));

            CreateMap<Import, InventoryImportDetailDto>()
                .ForMember(dest => dest.CreatedByName,
                           opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
                .ForMember(dest => dest.Details,
                           opt => opt.MapFrom(src => src.ImportDetails))
                   .ForMember(dest => dest.AuditLogs, opt => opt.Ignore()); // Vì ta gán sau khi lấy dữ liệu audit log;

            // Mapping từ entity ImportDetail sang InventoryImportDetailItemDto
            CreateMap<ImportDetail, InventoryImportDetailItemDto>()
     .ForMember(dest => dest.ProductVariantName,
         opt => opt.MapFrom(src => $"{src.ProductVariant.Product.Name} - {src.ProductVariant.Size.SizeName} - {src.ProductVariant.Color.ColorName}"))
     .ForMember(dest => dest.StoreDetails,
         opt => opt.MapFrom(src => src.ImportStoreDetails));

            // Mapping từ entity ImportStoreDetail sang InventoryImportStoreDetailDto
            CreateMap<ImportStoreDetail, Domain.DTO.Response.InventoryImportStoreDetailDto>()
    .ForMember(dest => dest.StoreId,
               opt => opt.MapFrom(src => src.Warehouse.WarehouseId))
    .ForMember(dest => dest.StoreName,
               opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
    .ForMember(dest => dest.StaffName,
               opt => opt.MapFrom(src => src.StaffDetail != null ? src.StaffDetail.Account.FullName : null))
             .ForMember(dest => dest.ActualQuantity,
               opt => opt.MapFrom(src => src.ActualReceivedQuantity));



            //opt => opt.MapFrom(src => src.StaffDetail != null ? src.StaffDetail.Account.FullName : null))
            //    .ForMember(dest => dest.ActualQuantity,
            //               opt => opt.MapFrom(src => src.ActualReceivedQuantity));
        
            CreateMap<AuditLog, AuditLogRes>().ForMember(dest => dest.ChangedByName,
               opt => opt.MapFrom(src => src.ChangedByNavigation.FullName));

            // --- Mapping từ Request DTO sang Entity ---
            CreateMap<CreateImportDto, Import>()
                .ForMember(dest => dest.ImportId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ImportDetails, opt => opt.MapFrom(src => src.ImportDetails));

            CreateMap<CreateImportDetailDto, ImportDetail>()
                .ForMember(dest => dest.ImportDetailId, opt => opt.Ignore())
                .ForMember(dest => dest.Import, opt => opt.Ignore()) // Không map navigation property để tránh vòng lặp
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.MapFrom(src => src.StoreDetails));

            CreateMap<CreateStoreDetailDto, ImportStoreDetail>()
                .ForMember(dest => dest.ImportStoreId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));

            // Với SupplementImportRequestDto, ta dùng IncludeBase để kế thừa mapping từ CreateImportDto
            CreateMap<SupplementImportRequestDto, Import>()
             .ForMember(dest => dest.ImportId, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.Status, opt => opt.Ignore())
             .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
             .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
             .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
             .ForMember(dest => dest.ImportDetails, opt => opt.MapFrom(src => src.ImportDetails))
             .ForMember(dest => dest.OriginalImportId, opt => opt.MapFrom(src => src.OriginalImportId));

            CreateMap<SupplementImportDetailDto, ImportDetail>()
                .ForMember(dest => dest.ImportDetailId, opt => opt.Ignore())
                .ForMember(dest => dest.Import, opt => opt.Ignore()) // bỏ qua navigation property
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.Ignore());


            // Mapping từ CreateTransferFullFlowDto sang Transfer
            CreateMap<CreateTransferFullFlowDto, Transfer>()
                .ForMember(dest => dest.TransferDetails, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());
            CreateMap<CreateTransferFullFlowDto, Import>()
    .ForMember(dest => dest.ImportId, opt => opt.Ignore());
            // Mapping từng chi tiết chuyển hàng
            CreateMap<CreateTransferDetailDto, TransferDetail>();

            // Mapping từ Transfer sang TransferFullFlowDto
            CreateMap<Transfer, TransferFullFlowDto>()
                .ForMember(dest => dest.TransferOrderId, opt => opt.MapFrom(src => src.TransferOrderId))
                // Giả sử bạn lưu ImportId và DispatchId trong Transfer nếu cần
                .ForMember(dest => dest.ImportId, opt => opt.MapFrom(src => src.ImportId))
                .ForMember(dest => dest.DispatchId, opt => opt.MapFrom(src => src.DispatchId));
            //mapping transfer
            CreateMap<TransferDetail, TransferDetailDto>();
            CreateMap<TransferDto, TransferResponseDto>()
                           .ForMember(dest => dest.ImportReferenceNumber,
                                      opt => opt.MapFrom(src => src.ImportReferenceNumber))
                           .ForMember(dest => dest.DispatchReferenceNumber,
                                      opt => opt.MapFrom(src => src.DispatchReferenceNumber))
                            .ForMember(dest => dest.CreatedByName,
                           opt => opt.MapFrom(src => src.CreatedByName));


            //================Dispatch============
            CreateMap<Dispatch, JSONDispatchDTO>()
                .ForMember(dest => dest.CreatedByUser,
                           opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
                .ForMember(dest => dest.Details,
                           opt => opt.MapFrom(src => src.DispatchDetails)).ReverseMap();

            // Mapping DispatchDetail -> JSONDispatchDetailDTO
            CreateMap<DispatchDetail, JSONDispatchDetailDTO>()
                .ForMember(dest => dest.VariantName,
                           opt => opt.MapFrom(src => src.Variant.Product.Name))
                    .ForMember(dest => dest.SizeName,
                           opt => opt.MapFrom(src => src.Variant.Size.SizeName))
                           .ForMember(dest => dest.ColorName,
                           opt => opt.MapFrom(src => src.Variant.Color.ColorName))
                .ForMember(dest => dest.PriceProductVariant,
           opt => opt.MapFrom(src => src.Variant.Price))
                .ForMember(dest => dest.StoreExportDetail,
                           opt => opt.MapFrom(src => src.StoreExportStoreDetails)).ReverseMap(); //
            CreateMap<StoreExportStoreDetail, JSONStoreExportDetailDTO>()
                .ForMember(dest => dest.WarehouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.Staff,
                           opt => opt.MapFrom(src => src.StaffDetail.Account.FullName))
                .ForMember(dest => dest.HandleBy,
                           opt => opt.MapFrom(src => src.HandleByNavigation.Account.FullName))



                .ReverseMap();

            CreateMap<StoreExportStoreDetail, JSONStoreExportStoreDetailByIdHandlerDTO>()
                .ForMember(dest => dest.WarehouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.Staff,
                           opt => opt.MapFrom(src => src.StaffDetail.Account.FullName))
                .ForMember(dest => dest.HandleBy,

                           opt => opt.MapFrom(src => src.HandleByNavigation.Account.FullName))
                .ForMember(dest => dest.ReferenceNumber,
                           opt => opt.MapFrom(src => src.DispatchDetail.Dispatch.ReferenceNumber))
                 .ForMember(dest => dest.ProductName,
                           opt => opt.MapFrom(src => src.DispatchDetail.Variant.Product.Name))
                    .ForMember(dest => dest.SizeName,
                           opt => opt.MapFrom(src => src.DispatchDetail.Variant.Size.SizeName))
                           .ForMember(dest => dest.ColorName,
                           opt => opt.MapFrom(src => src.DispatchDetail.Variant.Color.ColorName))

                   .ForMember(dest => dest.WarehouseDestinationName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName)).ReverseMap();


            // Import ========
            CreateMap<ImportStoreDetail, JSONImportStoreDetailDTO>()
                .ForMember(dest => dest.Staff,
                           opt => opt.MapFrom(src => src.StaffDetail.Account.FullName))
                .ForMember(dest => dest.WarehouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.HandleBy,
                           opt => opt.MapFrom(src => src.HandleByNavigation.Account.FullName))
                .ForMember(dest => dest.ReferenceNumber,
                           opt => opt.MapFrom(src => src.ImportDetail.Import.ReferenceNumber))
                .ForMember(dest => dest.CostPrice,
                           opt => opt.MapFrom(src => src.ImportDetail.CostPrice))
                .ForMember(dest => dest.ProductName,
                           opt => opt.MapFrom(src => src.ImportDetail.ProductVariant.Product.Name))
                .ForMember(dest => dest.SizeName,
                           opt => opt.MapFrom(src => src.ImportDetail.ProductVariant.Size.SizeName))
                .ForMember(dest => dest.ColorName,
                           opt => opt.MapFrom(src => src.ImportDetail.ProductVariant.Color.ColorName))
                .ForMember(dest => dest.AuditLogs, opt => opt.Ignore()).ReverseMap();
            //Iport json
            CreateMap<Import, JSONImportDTO>()
                .ForMember(dest => dest.CreatedBy,
                           opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
                .ForMember(dest => dest.Details,
                           opt => opt.MapFrom(src => src.ImportDetails)).ReverseMap();
            CreateMap<ImportDetail, JSONImportDetailDTO>()
                .ForMember(dest => dest.Product,
                           opt => opt.MapFrom(src => src.ProductVariant.Product.Name))
                    .ForMember(dest => dest.Size,
                           opt => opt.MapFrom(src => src.ProductVariant.Size.SizeName))
                        .ForMember(dest => dest.Color,
                           opt => opt.MapFrom(src => src.ProductVariant.Color.ColorName))
                .ForMember(dest => dest.PriceProductVariant,
                           opt => opt.MapFrom(src => src.ProductVariant.Price))
                .ForMember(dest => dest.StoreImportDetail,
                           opt => opt.MapFrom(src => src.ImportStoreDetails)).ReverseMap(); //
            CreateMap<ImportStoreDetail, JSONImportStoreDetailGetDTO>()
                .ForMember(dest => dest.Staff,
                           opt => opt.MapFrom(src => src.StaffDetail.Account.FullName))
                .ForMember(dest => dest.WarehouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.HandleBy,
                           opt => opt.MapFrom(src => src.HandleByNavigation.Account.FullName)).ReverseMap();

            // Transfer
            CreateMap<Transfer, JSONTransferOrderDTO>()
                .ForMember(dest => dest.CreatedBy,
                           opt => opt.MapFrom(src => src.Import.CreatedByNavigation.FullName))
                .ForMember(dest => dest.DetailsTransferOrder,
                           opt => opt.MapFrom(src => src.TransferDetails)).ReverseMap();

            CreateMap<TransferDetail, JSONTransferOrderDetailDTO>()
                .ForMember(dest => dest.Product,
                           opt => opt.MapFrom(src => src.Variant.Product.Name))
                  .ForMember(dest => dest.Size,
                           opt => opt.MapFrom(src => src.Variant.Color.ColorName))
              .ForMember(dest => dest.Color,
                           opt => opt.MapFrom(src => src.Variant.Size.SizeName)).ReverseMap();




            CreateMap<Product, ProductDto>()
            // map ImagePath từ bộ ProductImages
            .ForMember(dest => dest.ImagePath,
                       opt => opt.MapFrom(src => src.ProductImages
                                                   .FirstOrDefault(pi => pi.IsMain)
                                                   .ImagePath));

            CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.ImagePath,
               opt => opt.MapFrom(src => src.ProductImages.Select(img => img.ImagePath).ToList()));
            CreateMap<ProductVariant, ProductVariantDto>().ReverseMap();

            CreateMap<ProductEditDto, Product>()
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore()); // Bỏ qua Images vì bạn xử lý riêng

            //shopmanager
            CreateMap<CreateImportDto, Import>()
                .ForMember(dest => dest.ImportId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ImportDetails, opt => opt.MapFrom(src => src.ImportDetails));

            CreateMap<CreateImportDetailDto, ImportDetail>()
                .ForMember(dest => dest.ImportDetailId, opt => opt.Ignore())
                .ForMember(dest => dest.Import, opt => opt.Ignore()) // Không map navigation property để tránh vòng lặp
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.MapFrom(src => src.StoreDetails))
                                .ForMember(dest => dest.CostPrice, opt => opt.MapFrom(src => src.CostPrice));
            ;


            CreateMap<CreateStoreDetailDto, ImportStoreDetail>()
                .ForMember(dest => dest.ImportStoreId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));


            // Với SupplementImportRequestDto, ta dùng IncludeBase để kế thừa mapping từ CreateImportDto
            CreateMap<SupplementImportRequestDto, Import>()
             .ForMember(dest => dest.ImportId, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
             .ForMember(dest => dest.Status, opt => opt.Ignore())
             .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
             .ForMember(dest => dest.ApprovedDate, opt => opt.Ignore())
             .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
             .ForMember(dest => dest.ImportDetails, opt => opt.MapFrom(src => src.ImportDetails))
             .ForMember(dest => dest.OriginalImportId, opt => opt.MapFrom(src => src.OriginalImportId));

            CreateMap<SupplementImportDetailDto, ImportDetail>()
                .ForMember(dest => dest.ImportDetailId, opt => opt.Ignore())
                .ForMember(dest => dest.Import, opt => opt.Ignore()) // bỏ qua navigation property
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.Ignore());

            // --- Mapping từ Entity sang Response DTO ---
            CreateMap<Import, ImportDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.CreatedByNavigation.Email))
                .ForMember(dest => dest.ImportDetails, opt => opt.MapFrom(src => src.ImportDetails))

            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                    src.CreatedByNavigation != null ? src.CreatedByNavigation.FullName : null));
            CreateMap<ImportDetail, ImportDetailDto>()
                .ForMember(dest => dest.ImportStoreDetails, opt => opt.MapFrom(src => src.ImportStoreDetails))
                // Loại bỏ property 'Import' (không map) để tránh vòng lặp:
                .ForSourceMember(src => src.Import, opt => opt.DoNotValidate());

            CreateMap<ImportStoreDetail, ImportStoreDetailDtoStore>().ForMember(dest => dest.HandleByName, opt => opt.MapFrom(src =>
                    src.HandleByNavigation != null ? src.HandleByNavigation.Account.FullName : null))
                 .ForMember(dest => dest.HandleBy, opt => opt.MapFrom(src => src.HandleByNavigation.ShopManagerDetailId));
            CreateMap<ProductVariant, ProductVariantResponseDto>();

            //get all staff
            CreateMap<ImportStoreDetail, Domain.DTO.Request.InventoryImportStoreDetailDto>()
                // Các property có cùng tên sẽ được map tự động:
                // ImportStoreId, ImportDetailId, WareHouseId, AllocatedQuantity, Status, Comments, StaffDetailId
                .ForMember(dest => dest.WareHouseName,
                           opt => opt.MapFrom(src => src.Warehouse.WarehouseName))
                .ForMember(dest => dest.ImportId, opt => opt.MapFrom(src => src.ImportDetail.Import.ImportId))
 .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                // string interpolation để nối 3 phần: tên sản phẩm, màu, kích thước
                $"{src.ImportDetail.ProductVariant.Product.Name} - " +
                $"{src.ImportDetail.ProductVariant.Color.ColorName} - " +
                $"{src.ImportDetail.ProductVariant.Size.SizeName}"
            ))
                .ForMember(dest => dest.StaffName,
                           opt => opt.MapFrom(src => src.StaffDetail != null ? src.StaffDetail.Account.FullName : null));
            CreateMap<ProductVariant, ProductVariantResponseDto>()
              .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.VariantId))
              .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
              .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.Size == null ? string.Empty : src.Size.SizeName))
              .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.Color == null ? string.Empty : src.Color.ColorName))
              .ForMember(dest => dest.MainImagePath, opt => opt.MapFrom(src =>
                  src.ImagePath));

            CreateMap<Dispatch, DispatchResponseDto>()
           .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByNavigation.FullName))
           .ForMember(dest => dest.DispatchDetails, opt => opt.MapFrom(src => src.DispatchDetails));

            CreateMap<DispatchDetail, DispatchDetailDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src =>
                    src.Variant.Product.Name + " " +
                    src.Variant.Color.ColorName + " " +
                    src.Variant.Size.SizeName))
                .ForMember(dest => dest.ExportDetails, opt => opt.MapFrom(src => src.StoreExportStoreDetails));

            CreateMap<StoreExportStoreDetail, ExportDetailDto>()
                .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.StaffDetail != null ? src.StaffDetail.Account.FullName : null))
                .ForMember(dest => dest.DispatchId, opt => opt.MapFrom(src => src.DispatchDetail != null ? src.DispatchDetail.DispatchId : 0))
    ;

            CreateMap<AssignStaffDTO, OrderAssignment>()
            .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments));

            CreateMap<OrderAssignment, OrderAssignmentResponseDTO>();
            CreateMap<Order, OrderResponseDTO>();

        }
    }
       

    }
    
    

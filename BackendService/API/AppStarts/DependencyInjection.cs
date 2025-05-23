

using Application.Interfaces;
using Application.Service;
using Application.Services;
using Application.UseCases;
using CloudinaryDotNet;
using Domain.DTO.Response;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Clients;
using Infrastructure.HelperServices;
using Infrastructure.Repositories;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.AppStarts
{
    public static class DependencyInjectionContainers
    {

        public static void InstallService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true; ;
                options.LowercaseQueryStrings = true;
            });
            services.AddDbContext<FtownContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DBDefault"));
            });

            // use DI here
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<AuthAdminHandler>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAccountRepos, AccountRepos>();
            services.AddScoped<IUserManagementRepository, UserManagementRepository>();

            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
                if (string.IsNullOrEmpty(settings.CloudName) ||
                    string.IsNullOrEmpty(settings.ApiKey) ||
                    string.IsNullOrEmpty(settings.ApiSecret))
                {
                    throw new ArgumentException("CloudinarySettings không được cấu hình đúng trong appsettings.json");
                }
                var account = new CloudinaryDotNet.Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
                return new Cloudinary(account);
            });

            services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7261/api/");
            });
            services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7261/api/");
            });
            services.AddHttpClient<INotificationClient, NotificationServiceClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7261/api/");
            });
            services.AddHttpClient<IPayOSService, PayOSService>(client =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var payOSBaseUrl = configuration["PayOS:ApiUrl"];
                client.BaseAddress = new Uri(payOSBaseUrl);
            });

            // use DI here

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddScoped<IOrderProcessingHelper, OrderProcessingHelper>();
            services.AddScoped<IPaginationHelper, PaginationHelper>();
            services.AddScoped<IAssignmentSettingService, AssignmentSettingService>();
            services.AddScoped<IOrderAutoCompletionHandler, OrderAutoCompletionHandler>();
            services.AddHostedService<OrderAutoCompletionService>();
            //Repository
            services.AddScoped<IConversationService, ConversationHandler>();
            services.AddScoped<IMessageService, MessageHandler>();
            services.AddScoped<IConversationBotRepository, ConversationBotRepository>();
            services.AddScoped<IMessageBotRepository, MessageBotRepository>();
            services.AddScoped<IChatBotRepository, ChatBotRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOutfitRecommendationService, OutfitRecommendationService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.AddScoped<IImportRepos, InventoryImportRepository>();

            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IDispatchRepos, DispatchRepos>();
            services.AddScoped<IImportStoreRepos, ImportStoreRepos>();
            services.AddScoped<IStoreExportRepos, StoreExportRepos>();
            services.AddScoped<ITransferRepos, TransferRepos>();
            //services.AddScoped<IRoleService, GetRoleHandler>();
            services.AddScoped<IProductVarRepos, ProductVarRepos>();
            services.AddScoped<IUploadImageService, UploadImageService>();
            //ducanh
            services.AddScoped<ITransferRepos, TransferRepos>();
            services.AddScoped<IDispatchRepos, DispatchRepos>();
            services.AddScoped<IWarehouseStaffRepos, WarehouseStaffRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<ITransferDetailRepository, TransferDetailRepository>();
            services.AddScoped<IDispatchDetailRepository, DispatchDetailRepos>();

            services.AddScoped<ReportService>();
            services.AddScoped<GetWareHouseIdHandler>();

            services.AddScoped<ApproveHandler>();
            services.AddScoped<RejectHandler>();
            services.AddScoped<GetAllImportHandler>();
            services.AddScoped<GetImportDetailHandler>();
            services.AddScoped<GetAllProductHandler>();
            services.AddScoped<GetProductDetailHandler>();
            services.AddScoped<EditProductHandler>();

            services.AddScoped<CreateImportHandler>();
            services.AddScoped<GetWareHouseHandler>();
            services.AddScoped<TransferHandler>();
            services.AddScoped<GetAllTransferHandler>();
            services.AddScoped<CreateProductHandler>();
            services.AddScoped<CreateWarehouseHandler>();
            //services.AddScoped<GetAllCategoryHandler>();
            services.AddScoped<GetVariantHandler>();
            services.AddScoped<EditVariantHandler>();
            services.AddScoped<DashboardHandler>();

            //ducanh
            services.AddScoped<DispatchHandler>();
            services.AddScoped<ImportStoreDetailHandler>();
            services.AddScoped<RedisHandler>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            services.AddScoped<IUserManagementRepository, UserManagementRepository>();
            services.AddScoped<IRedisRepository, RedisRepository>();
            services.AddHttpClient<IChatServices, DeepSeekService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IShippingAddressRepository, ShippingAddressRepository>();
            services.AddScoped<IReturnOrderRepository, ReturnOrderRepository>();
            services.AddScoped<IRedisRepository, RedisRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IEmailRepository, EmailService>();
            services.AddScoped<IWareHouseStockAuditRepository, WareHouseStockAuditRepository>();
            services.AddScoped<IMessageRepository, ConversationRepository>();

            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentService, FeedbackHandler>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IReplyFeedbackService, ReplyHandler>();
            services.AddScoped<ICustomerProfileDataService, CustomerProfileDataService>();
            services.AddScoped<ICustomerRecentClickService, CustomerRecentClickService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<ISizeAndColorRepository, ColorAndSizeRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IWareHousesStockRepository, WareHousesStockRepository>();
            services.AddScoped<IRedisRepository, RedisRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();

            //GHN
            services.AddScoped<IGHNLogRepository, GHNLogRepository>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
          
            services.AddScoped<ICartRepository, CartRepository>();

            //Handler
            services.AddScoped<GHNLogHandler>();

            services.AddScoped<GetNotificationsHandler>();
            services.AddScoped<MarkNotificationAsReadHandler>();
            services.AddScoped<SendNotificationHandler>();


            //Handler
            services.AddScoped<CreateImportTransferHandler>();

            services.AddScoped<CreateOrderHandler>();
            services.AddScoped<GetShippingAddressHandler>();
            services.AddScoped<GetOrdersByStatusHandler>();
            services.AddScoped<GetSelectedCartItemsHandler>();
            services.AddScoped<CheckOutHandler>();
            services.AddScoped<ShippingCostHandler>();
            services.AddScoped<GetOrderDetailHandler>();
            services.AddScoped<UpdateOrderStatusHandler>();
            services.AddScoped<GetOrderItemsHandler>();
            services.AddScoped<GetReturnableOrdersHandler>();
            services.AddScoped<GetOrderItemsForReturnHandler>();
            services.AddScoped<ProcessReturnCheckoutHandler>();
            services.AddScoped<SubmitReturnRequestHandler>();
            services.AddScoped<AuditLogHandler>();
            services.AddScoped<RevenueHandler>();
            services.AddScoped<GetAllReturnRequestsHandler>();
            services.AddScoped<WareHouseStockAuditHandler>();
            services.AddScoped<EmailHandler>();
            services.AddScoped<UpdateReturnOrderStatusHandler>();

            services.AddScoped<EditProfileHandler>();
            services.AddScoped<GetCustomerProfileHandler>();
            services.AddScoped<GetShoppingCartHandler>();
            services.AddScoped<InteractionHandler>();
            services.AddScoped<SuggestProductsHandler>();
            services.AddScoped<PreferredStyleHandler>();

            services.AddScoped<ColorHandler>();
            services.AddScoped<SizeHandler>();


            services.AddScoped<GetAllProductsHandler>();
            services.AddScoped<FilterProductHandler>();
            services.AddScoped<GetProductDetailHandler>();
            services.AddScoped<GetProductVariantByIdHandler>();
            services.AddScoped<GetWarehouseByIdHandler>();
            services.AddScoped<GetWareHouseStockByVariantHandler>();
            services.AddScoped<UpdateStockAfterOrderHandler>();
            services.AddScoped<GetAllProductVariantsByIdsHandler>();
            services.AddScoped<GetStockQuantityHandler>();
            services.AddScoped<GetProductVariantByDetailsHandler>();
            services.AddScoped<RedisHandler>();
            services.AddScoped<GetFavoriteProductsHandler>();
            services.AddScoped<AddFavoriteHandler>();
            services.AddScoped<RemoveFavoriteHandler>();
            services.AddScoped<GetTopSellingProductHandler>();
            services.AddScoped<GetProductsByStyleHandler>();
            services.AddScoped<CategoryHandler>();

            //Handler
            services.AddScoped<CreatePromotionHandler>();
            services.AddScoped<DeletePromotionHandler>();
            services.AddScoped<UpdatePromotionHandler>();
            services.AddScoped<GetAllPromotionsHandler>();
            services.AddScoped<ChatAppService>();


            services.AddScoped<IImportRepos, InventoryImportRepository>();
            services.AddScoped<IStaffDetailRepository, StaffDetailRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IWareHousesStockRepository, WareHousesStockRepository>();
            services.AddScoped<IDispatchRepos, DispatchRepos>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IImportStoreRepos, ImportStoreRepos>();
            services.AddScoped<ITransferDetailRepository, TransferDetailRepository>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IWarehouseStaffRepos, WarehouseStaffRepository>();
            services.AddScoped<IWareHouseStockAuditRepository, WareHouseStockAuditRepository>();
            services.AddScoped<IProductVarRepos, ProductVarRepos>();
            services.AddScoped<ITransferRepos, TransferRepos>();
            services.AddScoped<IDispatchDetailRepository, DispatchDetailRepos>();
            services.AddScoped<IStoreExportRepos, StoreExportRepos>();
            services.AddScoped<ReportService>();

            services.AddScoped<CreateImportHandler>();
            services.AddScoped<GetImportHandler>();
            services.AddScoped<GetAllStaffHandler>();
            services.AddScoped<AssignStaffHandler>();
            services.AddScoped<GetAllDispatchHandler>();
            services.AddScoped<GetAllStaffDispatchHandler>();
            services.AddScoped<GetAllExportByStaffHandler>();
            services.AddScoped<GetAllImportStoreHandler>();


            services.AddScoped<GetAllProductHandler>();
            services.AddScoped<ImportDoneHandler>();
            services.AddScoped<ImportIncompletedHandler>();
            services.AddScoped<ImportShortageHandler>();
            services.AddScoped<DispatchDoneHandler>();

            services.AddScoped<GetAllStaffImportHandler>();
            services.AddScoped<GetWarehouseStockHandler>();
            services.AddScoped<GetOrderHandler>();
            services.AddScoped<CompletedOrderHandler>();


            // auto mapper
            services.AddAutoMapper(typeof(AutoMapperConfig).Assembly);

            services.AddHttpContextAccessor();

            //services.AddScoped<IUserService, UserServices>();

        }


    }
}

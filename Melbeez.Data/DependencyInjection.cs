using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Melbeez.Data.Database;
using Melbeez.Data.Repositories;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities.Identity;

namespace Melbeez.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureDataProject(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                //.AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //Add Unit Of Work Dependencies 
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ICitiesRepository, CitiesRepository>();
            services.AddScoped<IStatesRepository, StatesRepository>();
            services.AddScoped<ILocationsRepository, LocationsRepository>();
            services.AddScoped<IProductsRepository, ProductsRepository>();
            services.AddScoped<IProductWarrantiesRepository, ProductWarrantiesRepository>();
            services.AddScoped<IProductCategoriesRepository, ProductCategoriesRepository>();
            services.AddScoped<IRegisterDeviceRepository, RegisterDeviceRepository>();
            services.AddScoped<IOTPRepositry, OTPRepositry>();
            services.AddScoped<IAddressesRepository, AddressesRepository>();
            services.AddScoped<IContactusRepository, ContactusRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IReceiptRepository, ReceiptRepository>();
            services.AddScoped<IReceiptProductRepository, ReceiptProductRepository>();
            services.AddScoped<ISMSTransactionLogRepository, SMSTransactionLogRepository>();
            services.AddScoped<IEmailTransactionLogRepository, EmailTransactionLogRepository>();
            services.AddScoped<ISMLErrorLogRepository, SMLErrorLogRepository>();
            services.AddScoped<IPushNotificationRepositry, PushNotificationRepositry>();
            services.AddScoped<IUserNotificationPreferenceRepository, UserNotificationPreferenceRepository>();
            services.AddScoped<IAdminTransactionLogRepository, AdminTransactionLogRepository>();
            services.AddScoped<IUsersActivityTransactionLogRepository, UsersActivityTransactionLogRepository>();
            services.AddScoped<IAPIActivitiesRepository, APIActivitiesRepository>();
            services.AddScoped<IAPIDownStatusRepository, APIDownStatusRepository>();
            services.AddScoped<IBarCodeTransactionLogsRepository, BarCodeTransactionLogsRepository>();
            services.AddScoped<IMoveProductsToAnotherUserLocationRepository, MoveProductsToAnotherUserLocationRepository>();
            services.AddScoped<IItemTransferInvitationRepository, ItemTransferInvitationRepository>();
            services.AddScoped<IItemTransferRepository, ItemTransferRepository>();
            services.AddScoped<IProductModelInformationRepository, ProductModelInformationRepository>();
            return services;
        }
    }
}

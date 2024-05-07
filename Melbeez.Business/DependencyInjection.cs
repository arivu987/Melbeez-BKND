using Melbeez.Business.Common.Services;
using Melbeez.Business.Managers;
using Melbeez.Business.Managers.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Melbeez.Business
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureBusinessServices(
            this IServiceCollection services
        )
        {
            //Add Mapping Dependency
            services.AddAutoMapper(typeof(AutoMappingProfile));

            //Database Seed Manager
            services.AddScoped<ISeedManager, SeedManager>();

            //Add Managers Dependencies 
            services.AddScoped<IUserRefreshTokenManager, UserRefreshTokenManager>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IEmailManager, EmailManager>();
            services.AddScoped<ICountryManager, CountryManager>();
            services.AddScoped<ICitiesManager, CitiesManager>();
            services.AddScoped<IStatesManager, StatesManager>();
            services.AddScoped<ILocationsManager, LocationsManager>();
            services.AddScoped<IProductsManager, ProductsManager>();
            services.AddScoped<IProductWarrantiesManager, ProductWarrantiesManager>();
            services.AddScoped<IProductCategoriesManager, ProductCategoriesManager>();
            services.AddScoped<IRegisterDeviceManager, RegisterDeviceManager>();
            services.AddScoped<IAddressesManager, AddressesManager>();
            services.AddScoped<IContactusManager, ContactusManager>();
            services.AddScoped<IProductImageManager, ProductImageManager>();
            services.AddScoped<IReceiptManager, ReceiptManager>();
            services.AddScoped<ISMSTransactionLogManager, SMSTransactionLogManager>();
            services.AddScoped<IEmailTransactionLogManager, EmailTransactionLogManager>();
            services.AddScoped<ISMLErrorLogManager, SMLErrorLogManager>();
            services.AddScoped<ISMSManager, SMSManager>();
            services.AddScoped<IOtpManager, OtpManager>();
            services.AddScoped<IPushNotificationManager, PushNotificationManager>();
            services.AddScoped<ISendNotificationManager, SendNotificationManager>();
            services.AddScoped<IUserNotificationPreferenceManager, UserNotificationPreferenceManager>();
            services.AddScoped<IAdminTransactionLogManager, AdminTransactionLogManager>();
            services.AddScoped<IChartManager, ChartManager>();
            services.AddScoped<IPrivacyPolicyManager, PrivacyPolicyManager>();
            services.AddScoped<ITermsAndConditionsManager, TermsAndConditionsManager>();
            services.AddScoped<IUsersActivityTransactionLogManager, UsersActivityTransactionLogManager>();
            services.AddScoped<IAPIActivitiesManager, APIActivitiesManager>();
            services.AddScoped<IAPIDownStatusManager, APIDownStatusManager>();
            services.AddScoped<IBarCodeTransactionLogsManager, BarCodeTransactionLogsManager>();
            services.AddScoped<IMoveProductsToAnotherUserLocationManager, MoveProductsToAnotherUserLocationManager>();
            services.AddScoped<ICookiePolicyManager, CookiePolicyManager>();
            services.AddScoped<IEulaManager, EulaManager>();
            services.AddScoped<IItemTransferInvitationManager, ItemTransferInvitationManager>();
            services.AddScoped<IItemTransferManager, ItemTransferManager>();
            services.AddScoped<IProductModelInformationManager, ProductModelInformationManager>();
            return services;
        }
    }
}

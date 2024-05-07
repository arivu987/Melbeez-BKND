using Microsoft.Extensions.Logging;
using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using System.Threading.Tasks;
using System;

namespace Melbeez.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ILogger<UnitOfWork> logger;
        private readonly ApplicationDbContext dbContext;

        public UnitOfWork(
            ILogger<UnitOfWork> logger,
            ApplicationDbContext dbContext,
            ICountryRepository countryRepository,
            IStatesRepository statesRepository,
            ICitiesRepository citiesRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ILocationsRepository locationsRepository,
            IProductsRepository productsRepository,
            IProductWarrantiesRepository productWarrantiesRepository,
            IProductCategoriesRepository productCategoriesRepository,
            IRegisterDeviceRepository registerDeviceRepository,
            IAddressesRepository addressesRepository,
            IProductImageRepository productImageRepository,
            IReceiptRepository receiptRepository,
            IReceiptProductRepository receiptProductRepository,
            IEmailTransactionLogRepository emailTransactionLogRepository,
            ISMSTransactionLogRepository sMSTransactionLogRepository,
            ISMLErrorLogRepository sMLErrorLogRepository,
            IOTPRepositry otpRepositry,
            IPushNotificationRepositry pushNotificationRepositry,
            IUserNotificationPreferenceRepository userNotificationPreferenceRepository,
            IAdminTransactionLogRepository adminTransactionLogRepository,
            IContactusRepository contactusRepository,
            IUsersActivityTransactionLogRepository usersActivityTransactionLogRepository,
            IAPIActivitiesRepository apiActivitiesRepository,
            IAPIDownStatusRepository apiDownStatusRepository,
            IBarCodeTransactionLogsRepository barCodeTransactionLogsRepository,
            IMoveProductsToAnotherUserLocationRepository moveProductsToAnotherUserLocationRepository,
            IItemTransferInvitationRepository itemTransferInvitationRepository,
            IItemTransferRepository itemTransferRepository,
            IProductModelInformationRepository productModelInformationRepository
        //####WriteHere: Constructor####
        )
        {
            this.logger = logger;
            this.dbContext = dbContext;
            RefreshTokenRepository = refreshTokenRepository;
            CountryRepository = countryRepository;
            StatesRepository = statesRepository;
            CitiesRepository = citiesRepository;
            LocationsRepository = locationsRepository;
            ProductsRepository = productsRepository;
            ProductWarrantiesRepository = productWarrantiesRepository;
            ProductCategoriesRepository = productCategoriesRepository;
            RegisterDeviceRepository = registerDeviceRepository;
            AddressesRepository = addressesRepository;
            ContactusRepository = contactusRepository;
            ProductImageRepository = productImageRepository;
            ReceiptRepository = receiptRepository;
            ReceiptProductRepository = receiptProductRepository;
            EmailTransactionLogRepository = emailTransactionLogRepository;
            SMSTransactionLogRepository = sMSTransactionLogRepository;
            SMLErrorLogRepository = sMLErrorLogRepository;
            OTPRepositry = otpRepositry;
            PushNotificationRepositry = pushNotificationRepositry;
            UserNotificationPreferenceRepository = userNotificationPreferenceRepository;
            AdminTransactionLogRepository = adminTransactionLogRepository;
            UsersActivityTransactionLogRepository = usersActivityTransactionLogRepository;
            APIActivitiesRepository = apiActivitiesRepository;
            APIDownStatusRepository = apiDownStatusRepository;
            BarCodeTransactionLogsRepository = barCodeTransactionLogsRepository;
            MoveProductsToAnotherUserLocationRepository = moveProductsToAnotherUserLocationRepository;
            ItemTransferInvitationRepository = itemTransferInvitationRepository;
            ItemTransferRepository = itemTransferRepository;
            ProductModelInformationRepository = productModelInformationRepository;
            //####WriteHere: Constructor2####
        }

        public IRefreshTokenRepository RefreshTokenRepository { get; }
        public ICountryRepository CountryRepository { get; }
        public IStatesRepository StatesRepository { get; }
        public ICitiesRepository CitiesRepository { get; }
        public ILocationsRepository LocationsRepository { get; }
        public IProductsRepository ProductsRepository { get; }
        public IProductWarrantiesRepository ProductWarrantiesRepository { get; }
        public IProductCategoriesRepository ProductCategoriesRepository { get; }
        public IRegisterDeviceRepository RegisterDeviceRepository { get; }
        public IAddressesRepository AddressesRepository { get; }
        public IContactusRepository ContactusRepository { get; }
        public IProductImageRepository ProductImageRepository { get; }
        public IReceiptRepository ReceiptRepository { get; }
        public IReceiptProductRepository ReceiptProductRepository { get; }
        public IEmailTransactionLogRepository EmailTransactionLogRepository { get; }
        public ISMSTransactionLogRepository SMSTransactionLogRepository { get; }
        public ISMLErrorLogRepository SMLErrorLogRepository { get; }
        public IOTPRepositry OTPRepositry { get; }
        public IPushNotificationRepositry PushNotificationRepositry { get; }
        public IUserNotificationPreferenceRepository UserNotificationPreferenceRepository { get; }
        public IAdminTransactionLogRepository AdminTransactionLogRepository { get; }
        public IUsersActivityTransactionLogRepository UsersActivityTransactionLogRepository { get; }
        public IAPIActivitiesRepository APIActivitiesRepository { get; }
        public IAPIDownStatusRepository APIDownStatusRepository { get; }
        public IBarCodeTransactionLogsRepository BarCodeTransactionLogsRepository { get; }
        public IMoveProductsToAnotherUserLocationRepository MoveProductsToAnotherUserLocationRepository { get; }
        public IItemTransferInvitationRepository ItemTransferInvitationRepository { get; }
        public IItemTransferRepository ItemTransferRepository { get; }
        public IProductModelInformationRepository ProductModelInformationRepository { get; }
        //####WriteHere: Constructor3####
        public async Task CommitAsync()
        {
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occurred on SaveChanges.");
                throw;
            }
        }
        void IDisposable.Dispose()
        {
            if (dbContext != null)
            {
                dbContext.Dispose();
            }
        }
    }
}

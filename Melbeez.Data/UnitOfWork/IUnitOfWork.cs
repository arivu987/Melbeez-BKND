using Melbeez.Data.Repositories.Abstractions;

namespace Melbeez.Data.UnitOfWork
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
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
        //####WriteHere####
    }
}

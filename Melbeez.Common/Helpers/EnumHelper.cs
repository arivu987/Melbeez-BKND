namespace Melbeez.Common.Helpers
{
    public enum AddressType
    {
        Residential = 1,
        Billing = 2
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }
    public enum UserRole
    {
        Admin = 1,
        User = 2,
        SuperAdmin = 3
    }
    public enum NotificationType
    {
        WarrantyExpiry = 1,
        LocationUpdate = 2,
        ProductUpdate = 3,
        DeviceActivation = 4,
        MarketingAlert = 5,
        ItemMove = 6,
        ItemDeleted = 7,
    }
    public enum Status
    {
        Pending = 1,
        Success = 2
    }
    public enum LocationType
    {
        Residential = 1,
        Business = 2
    }
    public enum WarrantyStatus
    {
        Active = 1,
        Expired = 2
    }
    public enum Months
    {
        Jan = 1,
        Feb = 2,
        Mar = 3,
        Apr = 4,
        May = 5,
        Jun = 6,
        Jul = 7,
        Aug = 8,
        Sep = 9,
        Oct = 10,
        Nov = 11,
        Dec = 12
    }
    public enum MovedStatus
    {
        None = 0,
        Initiated = 1,
        Waiting = 2,
        Transferred = 3,
        Rejected = 4,
        Cancelled = 5,
        Expired = 6
    }

    public enum ProductModelStatus 
    {
        Pendding = 0,
        Approved = 1,
        Submitted = 2,
        Rejected = 3
    }
}

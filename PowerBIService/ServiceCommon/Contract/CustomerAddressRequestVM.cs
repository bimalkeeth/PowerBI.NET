namespace ServiceCommon.Contract
{
    public class CustomerAddressRequestVM
    {
        public int Id { get; set; }
        public bool IsPrimary { get; set; }
        public int CustomerId { get; set; }
        public int AddressId { get; set; }
        public  AddressRequestVM Address { get; set; }
    }
}
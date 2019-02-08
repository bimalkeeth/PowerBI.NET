namespace ServiceCommon.Contract
{
    public class CustomerDetailVM
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Contact { get; set; }
        public int ContactId { get; set; }
        public int ContactTypeId { get; set; }
        public int CustomerContactId { get; set; }
        public int AddressId { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string Suburb { get; set; }
        public string StateName { get; set; }
        public int StateId { get; set; }
        public string Country { get; set; }
        public int AddressTypeId { get; set; }
        public int CustomerAddressId { get; set; }
        public string CustomerCode { get; set; }
        public string DateOfBirth { get; set; }
    }
}
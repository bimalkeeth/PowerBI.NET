namespace ServiceCommon.Contract
{
    public class CustomerRequestVM
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public CustomerContactRequestVM[] CustomerContacts { get; set; }
        public CustomerAddressRequestVM[] CustomerAddress { get; set; }
    }
}
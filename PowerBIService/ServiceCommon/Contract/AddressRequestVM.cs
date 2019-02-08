namespace ServiceCommon.Contract
{
    public class AddressRequestVM
    {
        public int Id { get; set; }
        public int AddressTypeId { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string Suburb { get; set; }
        public int StateId { get; set; }
        public string Country { get; set; }
    }
}
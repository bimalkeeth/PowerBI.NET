namespace ServiceCommon.Contract
{
    public class CustomerContactRequestVM
    {
        public int Id { get; set; }
        public bool IsPrimary { get; set; }
        public int CustomerId { get; set; }
        public int ContactId { get; set; }
        public  ContactRequestVM Contact { get; set; }
    }
}
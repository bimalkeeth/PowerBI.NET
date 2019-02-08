namespace ServiceCommon.Contract
{
    public class ContactRequestVM
    {
        public int Id { get; set; }
        public int ContactTypeId { get; set; }
        public string Contact { get; set; }
    }
}
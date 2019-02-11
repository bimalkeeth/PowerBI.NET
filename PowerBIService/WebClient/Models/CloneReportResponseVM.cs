namespace WebClient.Models
{
    public class CloneReportResponseVM
    {
        public string ParentReportName { get; set; }
        public string CloneReportName { get; set; }
        public bool Success { get; set; }
    }
}
namespace WebClient.Models
{
    public class CloneReportRequestVM
    {
        public string ParentWorkSpace { get; set; }
        public string ClientWorkSpace { get; set; }
        public CloneReportVM[] CloneReports { get; set; }
    }
}
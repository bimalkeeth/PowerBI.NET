namespace WebClientDemo.Models
{
    public class CloneReportRequestVM
    {
        public string ParentWorkSpaceId { get; set; }
        public string ClientWorkSpaceId { get; set; }
        public CloneReportVM[] CloneReports { get; set; }
    }
}
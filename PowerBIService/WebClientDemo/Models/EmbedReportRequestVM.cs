namespace WebClientDemo.Models
{
    public class EmbedReportRequestVM
    {
        public string ReportId { get; set; }
        public string WorkSpaceId { get; set; }
        public string EmbedReportUrl { get; set; }
        public EmbededReporParamVM[] ParaMeters { get; set; }
        public string EmbedUserName { get; set; }
        public string EmbedRoles { get; set; }
    }
}
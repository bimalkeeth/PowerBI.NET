namespace WebClientDemo.Models
{
    public class EmbedReportRequestVM
    {
        public string ReportName { get; set; }
        public string WorkSpaceName { get; set; }
        public string EmbedReportUrl { get; set; }
        public EmbededReporParamVM[] ParaMeters { get; set; }
        public string EmbedUserName { get; set; }
        public string EmbedRoles { get; set; }
    }
}
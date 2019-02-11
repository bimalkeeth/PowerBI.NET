using PowerBIService.Common;

namespace ServiceCommon.Contract
{
    public class EmbedReportRequest
    {
        public string ReportName { get; set; }
        public string WorkSpaceName { get; set; }
        public UserData Credential { get; set; }
        public string EmbedReportUrl { get; set; }

        public EmbededReportDataSetParam[] ParaMeters { get; set; }
        public string EmbedUserName { get; set; }
        public string EmbedRoles { get; set; }
    }
}
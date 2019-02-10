using PowerBIService.Common;

namespace ServiceCommon.Contract
{
    public class EmbedReportRequest
    {
        public string ReportName { get; set; }
        public string WorkSpaceName { get; set; }
        public UserData Credential { get; set; }
    }
}
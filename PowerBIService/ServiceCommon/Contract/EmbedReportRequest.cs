using PowerBIService.Common;

namespace ServiceCommon.Contract
{
    public class EmbedReportRequest
    {
        public string ReportName { get; set; }
        public UserData Credential { get; set; }
    }
}
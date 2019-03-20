using PowerBIService.Common;

namespace ServiceCommon.Contract
{
    public class EmbedReportRequest
    {
        public string ReportId { get; set; }
        public string WorkSpaceId { get; set; }
        public UserCredentials Credential { get; set; }
        public string EmbedReportUrl { get; set; }

        public EmbededReportDataSetParam[] ParaMeters { get; set; }
        public string EmbedUserName { get; set; }
        public string EmbedRoles { get; set; }
    }
}
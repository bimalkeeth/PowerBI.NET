using PowerBIService.Common;

namespace ServiceCommon.Contract
{
    public class GetReportRequest
    {
        public string WorkSpaceId { get; set; }
        public UserCredentials Credential { get; set; }
    }
}
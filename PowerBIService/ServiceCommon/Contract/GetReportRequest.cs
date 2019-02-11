using PowerBIService.Common;

namespace ServiceCommon.Contract
{
    public class GetReportRequest
    {
        public string WorkSpaceId { get; set; }
        public UserData Credential { get; set; }
    }
}


using PowerBIService.Common;

namespace ServiceCommon.Contract
{
    public class CloneReportRequest
    {
        public UserData Credential { get; set; }
        public CloneReport[] CloneReports { get; set; }
        public string ParentWorkSpaceId { get; set; }

        public string ClientWorkSpaceId { get; set; }
    }
}
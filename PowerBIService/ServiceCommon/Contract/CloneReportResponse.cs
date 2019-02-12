namespace ServiceCommon.Contract
{
    public class CloneReportResponse
    {
        public string ParentReportName { get; set; }
        public string ParentReportId { get; set; }
        public string CloneReportName { get; set; }
        public string CloneReportId { get; set; }
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }
    }
}
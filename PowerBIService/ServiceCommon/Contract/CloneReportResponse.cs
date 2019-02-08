namespace ServiceCommon.Contract
{
    public class CloneReportResponse
    {
        public string ParentReportName { get; set; }
        public string CloneReportName { get; set; }
        public bool Success { get; set; }
    }
}
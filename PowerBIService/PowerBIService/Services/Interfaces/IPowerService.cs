using System.Threading.Tasks;
using PowerBIService.Common;
using ServiceCommon.Contract;

namespace PowerBIService.Services.Interfaces
{
    public interface IPowerService
    {
        /// <summary>
        /// Embed Report
        /// </summary>
        /// <param name="embedReportRequest"></param>
        /// <returns></returns>
        Task<EmbedConfig> ClientEmbedReport(EmbedReportRequest embedReportRequest);
        
        /// <summary>
        /// Create Group
        /// </summary>
        /// <param name="groupCreateRequest"></param>
        /// <returns></returns>
        Task<bool> CreateGroup(GroupCreateRequest groupCreateRequest);

        /// <summary>
        /// Clone one or more reports
        /// </summary>
        /// <param name="cloneReportRequest"></param>
        /// <returns></returns>
        Task<CloneReportResponse[]> CloneReports(CloneReportRequest cloneReportRequest);
    }

   
}
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
        /// <param name="embedReportRequestt"></param>
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

        /// <summary>
        /// Get All Groups
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task<NameValueContract[]> GetAllGroups(UserData credential);

        /// <summary>
        /// Get Reports for the workspace
        /// </summary>
        /// <param name="getReportRequest"></param>
        /// <returns></returns>
        Task<NameValueContract[]> GetAllReportInWorkSpace(GetReportRequest getReportRequest);
    }

   
}
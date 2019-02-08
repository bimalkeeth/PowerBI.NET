using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using PowerBIService.Common;
using PowerBIService.Services.Base;
using PowerBIService.Services.Interfaces;
using ServiceCommon.Contract;
using ServiceCommon.Resources;
using CloneReportRequest = ServiceCommon.Contract.CloneReportRequest;

namespace PowerBIService.Services.Implementation
{
    public class PowerService:PowerServiceBase,IPowerService
    {
        private EmbedConfig         MEmbedConfig;
        private TileEmbedConfig     MTileEmbedConfig;
        private TokenCredentials    MTokenCredentials;


        public PowerService()
        {
            MTokenCredentials = null;
            MEmbedConfig = new EmbedConfig();
            MTileEmbedConfig = new TileEmbedConfig();
        }

        public async Task<CloneReportResponse[]> CloneReports(CloneReportRequest cloneReportRequest)
        {
            if (string.IsNullOrWhiteSpace(cloneReportRequest.Credential.TenantId))
            {
                throw new ValidationException(PowerResource.ValidationError_TenantIdMissing);
            }
            if (string.IsNullOrWhiteSpace(cloneReportRequest.Credential.SecretId))
            {
                throw new ValidationException(PowerResource.ValidationError_SecretIdMissing);
            }
            if (string.IsNullOrWhiteSpace(cloneReportRequest.Credential.ApplicationId))
            {
                throw new ValidationException(PowerResource.ValidationError_ApplicationIdMissing);
            }
            if (!cloneReportRequest.CloneReports.Any())
            {
                throw new ValidationException(PowerResource.ValidationError_ReportsMissingForClone);
            }
            if (string.IsNullOrWhiteSpace(cloneReportRequest.ParentWorkSpace))
            {
                throw new ValidationException(PowerResource.ValidationError_ParentWorkSpaceMissingForClone);
            }
            if (string.IsNullOrWhiteSpace(cloneReportRequest.ClientWorkSpace))
            {
                throw new ValidationException(PowerResource.ValidationError_ClientWorkSpaceMissingForClone);
            }
            if (cloneReportRequest.CloneReports.FirstOrDefault(s => string.IsNullOrWhiteSpace(s.ParentReportName))!=null)
            {
                throw new ValidationException(PowerResource.ValidationError_ParentReportsNameMissingForClone);
            }
            UserData = cloneReportRequest.Credential;

            if (!await AuthenticateAsync())
            {
                throw new ValidationException(PowerResource.ValidationError_AuthenticationError);   
            }
            return await Clone(cloneReportRequest);
          
        }
        private async Task<CloneReportResponse[]> Clone(CloneReportRequest cloneReportRequest)
        {

            using (var pClient = new PowerBIClient(new Uri(POWER_BI_API_URL), PTokenCredentials))
            {


            }
            return new CloneReportResponse[] { };
        }
        public EmbedConfig EmbedReport(UserData userData)
        {
            base.UserData = userData;
            var data = Task.Run(async () => await AuthenticateAsync()).ConfigureAwait(false);
            data.GetAwaiter().GetResult();
            return null;
        }
        public async Task<bool> CreateGroup(GroupCreateRequest groupCreateRequest)
        {
            base.UserData = groupCreateRequest.Credential;
            await AuthenticateAsync();
            using (var pClient = new PowerBIClient(new Uri(POWER_BI_API_URL), PTokenCredentials))
            {
                var ddx = await pClient.Groups.GetGroupsWithHttpMessagesAsync();
                var group = await pClient.Groups.CreateGroupWithHttpMessagesAsync(
                    new GroupCreationRequest {Name = groupCreateRequest.GroupName});

                if (groupCreateRequest.Members.Any())
                {
                    foreach (var s in groupCreateRequest.Members)
                    {
                        {
                            await pClient.Groups.AddGroupUserWithHttpMessagesAsync(group.Body.Id,
                                new GroupUserAccessRight
                                {
                                    EmailAddress = s.MemberEmail, GroupUserAccessRightProperty = s.GroupUserAccessRight
                                });
                        }
                        ;
                    }
                }
            }

            return false;
        }

       
    }
}
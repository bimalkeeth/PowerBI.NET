using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
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
        public PowerService()
        {
            
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
            try
            {
                await AuthenticateAsync();
                using (var pClient = new PowerBIClient(new Uri(POWER_BI_API_URL), PTokenCredentials))
                {

                    var groups = await pClient.Groups.GetGroupsWithHttpMessagesAsync();
                    var group = groups.Body.Value.FirstOrDefault(s => s.Name == cloneReportRequest.ParentWorkSpace);
                    if (group == null)
                    {
                        throw new ValidationException(PowerResource.ValidationErrorParentGroupNotFoundError);
                    }

                    var clientGroup = groups.Body.Value.FirstOrDefault(s => s.Name == cloneReportRequest.ClientWorkSpace);
                    if (clientGroup == null)
                    {
                        throw new ValidationException(PowerResource.ValidationErrorClientGroupNotFoundError);
                    }

                    var reports = await pClient.Reports.GetReportsInGroupAsync(group.Id);
                    if (reports.Value.Any())
                    {
                        foreach (var cloneReport in cloneReportRequest.CloneReports)
                        {
                           var parentReport= reports.Value.FirstOrDefault(s => s.Name == cloneReport.ParentReportName);
                           if (parentReport != null)
                           {
                               var export= await pClient.Reports.ExportReportInGroupAsync(@group.Id, parentReport.Id);
                               var import = await TryUploadAsync(pClient, clientGroup.Id, export, cloneReport.CloneReportName);
                               var reportDatasetId = import.Datasets.First().Id;
                               try
                               {
                                   var parameter = new Dictionary<string, string> {{"ConnectionUrl", cloneReport.WebApiEndPoint}};

                                   var reportParameters = await pClient.Datasets.GetParametersInGroupAsync(clientGroup.Id, reportDatasetId);
                                   if (reportParameters!=null && reportParameters.Value.Any())
                                   {
                                       await SetReportParameters(pClient, cloneReportRequest.ClientWorkSpace, reportDatasetId, reportParameters.Value,parameter );
                                   }

                                   var refresh=await pClient.Datasets.RefreshDatasetInGroupWithHttpMessagesAsync(@group.Id, reportDatasetId);
                                   await pClient.Reports.RebindReportInGroupWithHttpMessagesAsync(clientGroup.Id,import.Id, new RebindReportRequest{DatasetId= reportDatasetId});
                               }
                               catch (Exception e){throw e;}
                           }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
               throw new ServerException(exp.Message);
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
                    }
                }
            }
            return false;
        }

       
    }
}
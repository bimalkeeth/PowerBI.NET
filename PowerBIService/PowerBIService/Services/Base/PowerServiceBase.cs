using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using PowerBIService.Common;

namespace PowerBIService.Services.Base
{

    public abstract class PowerServiceBase
    {
        #region Base Variables
        protected UserCredentials UserCredential { get; set; }
        protected AuthenticationResult TokenResult { get; set; }
        protected TokenCredentials PTokenCredentials { get; set;}
        protected  EmbedConfig EmbedConfiguration { get; set; }=new EmbedConfig();
        
        
        protected static string POWER_BI_API_URL = "https://api.powerbi.com";
        private static string POWER_BI_AUTHORITY_URL ="https://login.microsoftonline.com/common/oauth2/authorize/";
        private static string POWER_BI_AUTHORITY_API_URL ="https://login.windows.net/common/oauth2/authorize/";
        private static string POWER_BI_RESOURCE_URL = "https://analysis.windows.net/powerbi/api";
       
        #endregion
        #region Authontication
        protected  async Task<bool> AuthenticateAsync()
        {
            var tc = new TokenCache();
            var credential = new UserPasswordCredential(UserCredential.UserName, UserCredential.Password);
            var authenticationContext = new AuthenticationContext(POWER_BI_AUTHORITY_URL,false, tc);
            TokenResult = await authenticationContext.AcquireTokenAsync(POWER_BI_RESOURCE_URL, UserCredential.ApplicationId, credential);
            PTokenCredentials=new TokenCredentials(TokenResult.AccessToken,"Bearer");
            return true;
        }
        
        private async Task<string> GetPowerBiAccessToken()
        {
            var powerBiAccountCredentials = new UserPasswordCredential(UserCredential.UserName, UserCredential.Password);
            var authenticationContext = new AuthenticationContext(POWER_BI_AUTHORITY_API_URL);
            var authenticationResult = await authenticationContext.AcquireTokenAsync(POWER_BI_RESOURCE_URL, UserCredential.ApplicationId, powerBiAccountCredentials);
            return authenticationResult.AccessToken;
        }
        
        
        #endregion

        /// <summary>
        /// Send Api Request
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="httpMethod"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <exception cref="HttpOperationException"></exception>
        private async Task<HttpResponseMessage> SendAsync(string endpoint, string httpMethod, HttpContent content = null)
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), endpoint) { };

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(900000)
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json; charset=utf-8");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + await GetPowerBiAccessToken());
            if (content != null)
            {
                request.Content = content;
            }
            var response = await client.SendAsync(request, default(CancellationToken));
            if (!response.IsSuccessStatusCode)
            {
                var exc = new HttpOperationException()
                {
                    Response = new HttpResponseMessageWrapper(response, await response.Content.ReadAsStringAsync())
                };
                throw exc;
            }
            return response;
        }
        /// <summary>
        /// Setting up Content for Api Request
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="content"></param>
        /// <param name="datasetDisplayName"></param>
        /// <returns></returns>
        private async Task<Import> PostImportAsync(string workspaceId, Stream content, string datasetDisplayName)
        {
            var displayNameQueryParameter = (!string.IsNullOrEmpty(datasetDisplayName))
                ? FormattableString.Invariant($"?datasetDisplayName={Uri.EscapeDataString(datasetDisplayName)}")
                : string.Empty;
            string postImportEndpoint = FormattableString.Invariant($"https://api.powerbi.com/v1.0/myorg/groups/{workspaceId}/imports{displayNameQueryParameter}");

            var contentDispositionHeaderValue = new ContentDispositionHeaderValue("form-data") {Name = "file0"};
            HttpContent fileStreamContent = new StreamContent(content);
            fileStreamContent.Headers.ContentDisposition = contentDispositionHeaderValue;

            var multiPartContent = new MultipartFormDataContent {fileStreamContent};

            var response = await SendAsync(postImportEndpoint, "POST", multiPartContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            return SafeJsonConvert.DeserializeObject<Import>(responseContent);
        }
        /// <summary>
        /// Upload Report to Required workspace
        /// </summary>
        /// <param name="client"></param>
        /// <param name="workspaceId"></param>
        /// <param name="content"></param>
        /// <param name="datasetDisplayName"></param>
        /// <returns></returns>
        protected async Task<Import> TryUploadAsync(IPowerBIClient client, string workspaceId, Stream content, string datasetDisplayName)
        {
            var import = await PostImportAsync(workspaceId, content, datasetDisplayName);
            while (import.ImportState != "Succeeded" && import.ImportState != "Failed")
            {
                import = await client.Imports.GetImportByIdInGroupAsync(workspaceId, import.Id);
            }
            if (import.ImportState != "Succeeded")
            {
                return null;
            }
            return import;
        }
        protected static async Task SetReportParameters(IPowerBIClient client, string tenantWorkspaceId, string reportDatasetId, IEnumerable<DatasetParameter> reportParameters, IDictionary<string, string> parameters)
        {
            if (reportParameters != null && reportParameters.Any())
            {
                if (((parameters == null || !parameters.Any()) && reportParameters.Any(x => x.IsRequired)) ||
                    ((parameters != null || parameters.Any()) && reportParameters.Any(x => x.IsRequired && !parameters.Keys.Contains(x.Name))))
                {
                    throw new Exception("RequiredParameterValueMissing");
                }

                if (parameters != null && parameters.Any())
                {
                    var updateParameterList = new List<UpdateDatasetParameterDetails>();
                    foreach (var reportParameter in reportParameters)
                    {
                        if (parameters.ContainsKey(reportParameter.Name))
                        {
                            updateParameterList.Add(new UpdateDatasetParameterDetails { Name = reportParameter.Name, NewValue = parameters[reportParameter.Name] });
                        }
                    }
                    if (updateParameterList != null && updateParameterList.Any())
                    {
                        await client.Datasets.UpdateParametersInGroupAsync(tenantWorkspaceId, reportDatasetId, new UpdateDatasetParametersRequest
                            {
                                UpdateDetails = updateParameterList
                            }
                        );
                    }
                }
            }
        }


       
    }
}
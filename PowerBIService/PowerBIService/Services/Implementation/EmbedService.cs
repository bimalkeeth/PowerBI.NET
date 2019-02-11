using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using PowerBIService.Common;

namespace PowerBIService.Services.Implementation
{
    public class EmbedService
    {
        private static readonly string AuthorityUrl = "https://login.microsoftonline.com/common/oauth2/authorize";
        private static readonly string ResourceUrl = "https://analysis.windows.net/powerbi/api";
        private static readonly string ApplicationId = "66bec1b2-4684-4a08-9f2b-b67216d4695a";
        private static readonly string ApiUrl = "https://api.powerbi.com/";
        private static readonly string WorkspaceId = "c7461d03-6cbb-4e93-bf67-343314792907";
        private static readonly string ReportId = "d6de6e12-4bbf-48c9-bb09-dccedb9fb60b";
        private static readonly string Username = "bkaluarachchi@assetic.com";
        private static readonly string Password = "Scala@1234";

        public EmbedConfig EmbedConfig
        {
            get { return m_embedConfig; }
        }

        public TileEmbedConfig TileEmbedConfig
        {
            get { return m_tileEmbedConfig; }
        }

        private EmbedConfig m_embedConfig;
        private TileEmbedConfig m_tileEmbedConfig;
        private TokenCredentials m_tokenCredentials;

        public EmbedService()
        {
            m_tokenCredentials = null;
            m_embedConfig = new EmbedConfig();
            m_tileEmbedConfig = new TileEmbedConfig();
        }

        private static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[input.Length];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        private async Task<HttpResponseMessage> SendAsync(string endpoint, string httpMethod,
            HttpContent content = null)
        {
            var request = new HttpRequestMessage(new HttpMethod(httpMethod), endpoint) { };

            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(900000)
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json; charset=utf-8");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + (await GetPowerBiAccessToken()));

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

        private async Task<Import> PostImportAsync(string workspaceId, Stream content, string datasetDisplayName)
        {
            var displayNameQueryParameter = (!string.IsNullOrEmpty(datasetDisplayName))
                ? FormattableString.Invariant($"?datasetDisplayName={Uri.EscapeDataString(datasetDisplayName)}")
                : string.Empty;
            string postImportEndpoint = FormattableString.Invariant(
                $"https://api.powerbi.com/v1.0/myorg/groups/{workspaceId}/imports{displayNameQueryParameter}");


            var contentDispositionHeaderValue = new ContentDispositionHeaderValue("form-data") {Name = "file0"};
            HttpContent fileStreamContent = new StreamContent(content);
            fileStreamContent.Headers.ContentDisposition = contentDispositionHeaderValue;

            var multiPartContent = new MultipartFormDataContent {fileStreamContent};

            var response = await SendAsync(postImportEndpoint, "POST", multiPartContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            return SafeJsonConvert.DeserializeObject<Import>(responseContent);
        }

        private async Task<Import> TryUploadAsync(IPowerBIClient client, string workspaceId, Stream content,
            string datasetDisplayName)
        {
            // We use the API because at Import method the SDK fails when trying directly upload an exported readonly stream (because it now checks for length).
            // In order to avoid the stream transformation, we prefer to use the API.
            var import = await PostImportAsync(workspaceId, content, datasetDisplayName);
            // Polling the import to check when the import has succeeded.
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

        public async Task<bool> CloneReport(string username, string roles)
        {

            var data = await GetPowerBiAccessToken();
            var getCredentialsResult = await GetTokenCredentials();
            if (!getCredentialsResult)
            {
                return false;
            }

            try
            {
                using (var client = new PowerBIClient(new Uri(ApiUrl), m_tokenCredentials))
                {
                    var ddx = await client.Groups.GetGroupsWithHttpMessagesAsync();
                    var group = ddx.Body.Value.FirstOrDefault(s => s.Name == "My Workspace");

                    var reports = await client.Reports.GetReportsInGroupAsync(group.Id);

                    var dataset =
                        await client.Datasets.GetDatasetByIdInGroupWithHttpMessagesAsync(group.Id,
                            reports.Value.FirstOrDefault()?.DatasetId);
                    var dataSourceOrg = await client.Datasets.GetDatasourcesAsync(group.Id, dataset.Body.Id);
                    var report = reports.Value.FirstOrDefault();

                    var export = await client.Reports.ExportReportInGroupAsync(@group.Id, report.Id);


                    //c7461d03-6cbb-4e93-bf67-343314792907
                    await TryUploadAsync(client, "c7461d03-6cbb-4e93-bf67-343314792907", export, "CloneReportTwo");



                    //                   using (var stream = new MemoryStream())
                    //                   {
                    //                       CopyStream(export.Body, stream);
                    //                       var response = await client.Imports.PostImportFileWithHttpMessage(stream, "newDataSet", "Overwrite");
                    //                   }
                    //                   
                    //                   
                    //                   var rep=await client.Reports.CloneReportInGroupWithHttpMessagesAsync(@group.Id, report.Id, new CloneReportRequest
                    //                   {
                    //                        Name = "SepalReportCloneSecond",
                    //                        TargetWorkspaceId = @group.Id,
                    //                        TargetModelId = dataset.Body.Id
                    //                   });

                }
            }
            catch (HttpOperationException exc)
            {
                m_embedConfig.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}",
                    exc.Response.StatusCode, (int) exc.Response.StatusCode, exc.Response.Content,
                    exc.Response.Headers["RequestId"].FirstOrDefault());
                return false;
            }



            return false;
        }

        public async Task<bool> UpdateDataSource(string username, string roles)
        {

            // Get token credentials for user
            var getCredentialsResult = await GetTokenCredentials();
            if (!getCredentialsResult)
            {
                return false;
            }

            try
            {
                using (var client = new PowerBIClient(new Uri(ApiUrl), m_tokenCredentials))
                {
                    var ddx = await client.Groups.GetGroupsWithHttpMessagesAsync();
                    var group = ddx.Body.Value.FirstOrDefault(s => s.Name == "My Workspace");

                    var reports = await client.Reports.GetReportsInGroupAsync(group.Id);

                    var dataset =
                        await client.Datasets.GetDatasetByIdInGroupWithHttpMessagesAsync(group.Id,
                            reports.Value.FirstOrDefault()?.DatasetId);
                    var dataSourceOrg = await client.Datasets.GetDatasourcesAsync(group.Id, dataset.Body.Id);



                    var data = await client.Datasets.GetParametersInGroupWithHttpMessagesAsync(group.Id,
                        dataset.Body.Id);

                    var dd = data.Body.Value;

                    var response = await client.Datasets.UpdateParametersInGroupWithHttpMessagesAsync(@group.Id,
                        dataset.Body.Id,
                        new UpdateDatasetParametersRequest
                        {
                            UpdateDetails = new List<UpdateDatasetParameterDetails>
                            {
                                new UpdateDatasetParameterDetails
                                {
                                    Name = "ConnectionUrl",
                                    NewValue =
                                        "https://gist.githubusercontent.com/curran/a08a1080b88344b0c8a7/raw/d546eaee765268bf2f487608c537c05e22e4b221/iris.csv"
                                }
                            }
                        });



                    var refresh =
                        await client.Datasets.RefreshDatasetInGroupWithHttpMessagesAsync(@group.Id, dataset.Body.Id);
                    var report = reports.Value.FirstOrDefault();
                    await client.Reports.RebindReportInGroupWithHttpMessagesAsync(group.Id, report.Id,
                        new RebindReportRequest {DatasetId = dataset.Body.Id});


                    data = await client.Datasets.GetParametersInGroupWithHttpMessagesAsync(group.Id, dataset.Body.Id);

                    dd = data.Body.Value;



                }
            }
            catch (HttpOperationException exc)
            {
                m_embedConfig.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}",
                    exc.Response.StatusCode, (int) exc.Response.StatusCode, exc.Response.Content,
                    exc.Response.Headers["RequestId"].FirstOrDefault());
                return false;
            }

            return true;
        }





        public async Task<bool> EmbedReport(string username, string roles)
        {

            // Get token credentials for user
            var getCredentialsResult = await GetTokenCredentials();
            if (!getCredentialsResult)
            {
                // The error message set in GetTokenCredentials
                return false;
            }

            try
            {
                // Create a Power BI Client object. It will be used to call Power BI APIs.
                using (var client = new PowerBIClient(new Uri(ApiUrl), m_tokenCredentials))
                {
                    // Get a list of reports.
                    var reports = await client.Reports.GetReportsInGroupAsync(WorkspaceId);

                    // No reports retrieved for the given workspace.
                    if (reports.Value.Count() == 0)
                    {
                        m_embedConfig.ErrorMessage = "No reports were found in the workspace";
                        return false;
                    }

                    Report report;
                    if (string.IsNullOrWhiteSpace(ReportId))
                    {
                        // Get the first report in the workspace.
                        report = reports.Value.FirstOrDefault();
                    }
                    else
                    {
                        report = reports.Value.FirstOrDefault(r =>
                            r.Id.Equals(ReportId, StringComparison.InvariantCultureIgnoreCase));
                    }

                    if (report == null)
                    {
                        m_embedConfig.ErrorMessage =
                            "No report with the given ID was found in the workspace. Make sure ReportId is valid.";
                        return false;
                    }

                    var datasets = await client.Datasets.GetDatasetByIdInGroupAsync(WorkspaceId, report.DatasetId);


                    m_embedConfig.IsEffectiveIdentityRequired = datasets.IsEffectiveIdentityRequired;
                    m_embedConfig.IsEffectiveIdentityRolesRequired = datasets.IsEffectiveIdentityRolesRequired;
                    GenerateTokenRequest generateTokenRequestParameters;
                    // This is how you create embed token with effective identities
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var rls = new EffectiveIdentity(username, new List<string> {report.DatasetId});
                        if (!string.IsNullOrWhiteSpace(roles))
                        {
                            var rolesList = new List<string>();
                            rolesList.AddRange(roles.Split(','));
                            rls.Roles = rolesList;
                        }

                        // Generate Embed Token with effective identities.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view",
                            identities: new List<EffectiveIdentity> {rls});
                    }
                    else
                    {
                        // Generate Embed Token for reports without effective identities.
                        generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                    }

                    var tokenResponse =
                        await client.Reports.GenerateTokenInGroupAsync(WorkspaceId, report.Id,
                            generateTokenRequestParameters);

                    if (tokenResponse == null)
                    {
                        m_embedConfig.ErrorMessage = "Failed to generate embed token.";
                        return false;
                    }

                    // Generate Embed Configuration.
                    m_embedConfig.EmbedToken = tokenResponse;
                    m_embedConfig.EmbedUrl = report.EmbedUrl;
                    m_embedConfig.Id = report.Id;
                }
            }
            catch (HttpOperationException exc)
            {
                m_embedConfig.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}",
                    exc.Response.StatusCode, (int) exc.Response.StatusCode, exc.Response.Content,
                    exc.Response.Headers["RequestId"].FirstOrDefault());
                return false;
            }

            return true;
        }

        public async Task<bool> EmbedDashboard()
        {
            // Get token credentials for user
            var getCredentialsResult = await GetTokenCredentials();
            if (!getCredentialsResult)
            {
                // The error message set in GetTokenCredentials
                return false;
            }

            try
            {
                // Create a Power BI Client object. It will be used to call Power BI APIs.
                using (var client = new PowerBIClient(new Uri(ApiUrl), m_tokenCredentials))
                {
                    // Get a list of dashboards.
                    var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(WorkspaceId);

                    // Get the first report in the workspace.
                    var dashboard = dashboards.Value.FirstOrDefault();

                    if (dashboard == null)
                    {
                        m_embedConfig.ErrorMessage = "Workspace has no dashboards.";
                        return false;
                    }

                    // Generate Embed Token.
                    var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                    var tokenResponse = await client.Dashboards.GenerateTokenInGroupAsync(WorkspaceId, dashboard.Id,
                        generateTokenRequestParameters);

                    if (tokenResponse == null)
                    {
                        m_embedConfig.ErrorMessage = "Failed to generate embed token.";
                        return false;
                    }

                    // Generate Embed Configuration.
                    m_embedConfig = new EmbedConfig()
                    {
                        EmbedToken = tokenResponse,
                        EmbedUrl = dashboard.EmbedUrl,
                        Id = dashboard.Id
                    };

                    return true;
                }
            }
            catch (HttpOperationException exc)
            {
                m_embedConfig.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}",
                    exc.Response.StatusCode, (int) exc.Response.StatusCode, exc.Response.Content,
                    exc.Response.Headers["RequestId"].FirstOrDefault());
                return false;
            }
        }

        public async Task<bool> EmbedTile()
        {
            // Get token credentials for user
            var getCredentialsResult = await GetTokenCredentials();
            if (!getCredentialsResult)
            {
                // The error message set in GetTokenCredentials
                m_tileEmbedConfig.ErrorMessage = m_embedConfig.ErrorMessage;
                return false;
            }

            try
            {
                // Create a Power BI Client object. It will be used to call Power BI APIs.
                using (var client = new PowerBIClient(new Uri(ApiUrl), m_tokenCredentials))
                {
                    // Get a list of dashboards.
                    var dashboards = await client.Dashboards.GetDashboardsInGroupAsync(WorkspaceId);

                    // Get the first report in the workspace.
                    var dashboard = dashboards.Value.FirstOrDefault();

                    if (dashboard == null)
                    {
                        m_tileEmbedConfig.ErrorMessage = "Workspace has no dashboards.";
                        return false;
                    }

                    var tiles = await client.Dashboards.GetTilesInGroupAsync(WorkspaceId, dashboard.Id);

                    // Get the first tile in the workspace.
                    var tile = tiles.Value.FirstOrDefault();

                    // Generate Embed Token for a tile.
                    var generateTokenRequestParameters = new GenerateTokenRequest(accessLevel: "view");
                    var tokenResponse = await client.Tiles.GenerateTokenInGroupAsync(WorkspaceId, dashboard.Id, tile.Id,
                        generateTokenRequestParameters);

                    if (tokenResponse == null)
                    {
                        m_tileEmbedConfig.ErrorMessage = "Failed to generate embed token.";
                        return false;
                    }

                    // Generate Embed Configuration.
                    m_tileEmbedConfig = new TileEmbedConfig()
                    {
                        EmbedToken = tokenResponse,
                        EmbedUrl = tile.EmbedUrl,
                        Id = tile.Id,
                        dashboardId = dashboard.Id
                    };

                    return true;
                }
            }
            catch (HttpOperationException exc)
            {
                m_embedConfig.ErrorMessage = string.Format("Status: {0} ({1})\r\nResponse: {2}\r\nRequestId: {3}",
                    exc.Response.StatusCode, (int) exc.Response.StatusCode, exc.Response.Content,
                    exc.Response.Headers["RequestId"].FirstOrDefault());
                return false;
            }
        }

        /// <summary>
        /// Check if web.config embed parameters have valid values.
        /// </summary>
        /// <returns>Null if web.config parameters are valid, otherwise returns specific error string.</returns>
        private string GetWebConfigErrors()
        {
            // Application Id must have a value.
            if (string.IsNullOrWhiteSpace(ApplicationId))
            {
                return
                    "ApplicationId is empty. please register your application as Native app in https://dev.powerbi.com/apps and fill client Id in web.config.";
            }

            // Application Id must be a Guid object.
            Guid result;
            if (!Guid.TryParse(ApplicationId, out result))
            {
                return
                    "ApplicationId must be a Guid object. please register your application as Native app in https://dev.powerbi.com/apps and fill application Id in web.config.";
            }

            // Workspace Id must have a value.
            if (string.IsNullOrWhiteSpace(WorkspaceId))
            {
                return "WorkspaceId is empty. Please select a group you own and fill its Id in web.config";
            }

            // Workspace Id must be a Guid object.
            if (!Guid.TryParse(WorkspaceId, out result))
            {
                return
                    "WorkspaceId must be a Guid object. Please select a workspace you own and fill its Id in web.config";
            }

            // Must fill tenant Id in authorityUrl
            if (AuthorityUrl.Contains("Fill_Tenant_ID"))
            {
                return "Invalid AuthorityUrl. Please fill Tenant ID in AuthorityUrl under web.config";
            }

            // Username must have a value.
            if (string.IsNullOrWhiteSpace(Username))
            {
                return "Username is empty. Please fill Power BI username in web.config";
            }

            // Password must have a value.
            if (string.IsNullOrWhiteSpace(Password))
            {
                return "Password is empty. Please fill password of Power BI username in web.config";
            }

            return null;
        }

        private async Task<string> GetPowerBiAccessToken()
        {
            var powerBiAccountCredentials =
                new UserPasswordCredential(Username, Password); //Microsoft.Azure.Management.ResourceManager
            var authenticationContext = new AuthenticationContext("https://login.windows.net/common/oauth2/authorize/");
            var authenticationResult = await authenticationContext.AcquireTokenAsync(
                "https://analysis.windows.net/powerbi/api", "66bec1b2-4684-4a08-9f2b-b67216d4695a",
                powerBiAccountCredentials);
            return authenticationResult.AccessToken;
        }

        private async Task<AuthenticationResult> DoAuthentication()
        {

            TokenCache TC = new TokenCache();

            var authenticationContext = new AuthenticationContext(AuthorityUrl, false, TC);
            AuthenticationResult authenticationResult = null;

            // Authentication using master user credentials
            var credential = new UserPasswordCredential(Username, Password);
            //authenticationResult = await authenticationContext.AcquireTokenAsync(ResourceUrl, ApplicationId, credential);

            authenticationResult =
                await authenticationContext.AcquireTokenAsync(ResourceUrl, "66bec1b2-4684-4a08-9f2b-b67216d4695a",
                    credential);


            return authenticationResult;
        }

        private async Task<bool> GetTokenCredentials()
        {
            // var result = new EmbedConfig { Username = username, Roles = roles };
            var error = GetWebConfigErrors();
            if (error != null)
            {
                m_embedConfig.ErrorMessage = error;
                return false;
            }

            // Authenticate using created credentials
            AuthenticationResult authenticationResult = null;
            try
            {
                authenticationResult = await DoAuthentication();
            }
            catch (AggregateException exc)
            {
                m_embedConfig.ErrorMessage = exc.InnerException.Message;
                return false;
            }

            if (authenticationResult == null)
            {
                m_embedConfig.ErrorMessage = "Authentication Failed.";
                return false;
            }

            m_tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");



            return true;
        }
    }
}


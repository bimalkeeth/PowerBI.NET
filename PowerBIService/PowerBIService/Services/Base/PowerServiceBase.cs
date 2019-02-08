using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using PowerBIService.Common;

namespace PowerBIService.Services.Base
{

   
    public abstract class PowerServiceBase
    {
        #region Base Variables
        protected UserData UserData { get; set; }
        protected AuthenticationResult TokenResult { get; set; }
        protected TokenCredentials PTokenCredentials { get; set;}
        
        protected  EmbedConfig EmbedConfiguration { get; set; }=new EmbedConfig();
        
        
        protected static string POWER_BI_API_URL = "https://api.powerbi.com";

        private static string POWER_BI_AUTHORITY_URL ="https://login.microsoftonline.com/common/oauth2/authorize/"; //"https://login.microsoftonline.com/"; //"https://login.windows.net/common/oauth2/authorize/";
        private static string POWER_BI_AUTHORITY_TOKEN_URL = "https://login.windows.net/470cec91-5a0e-47c7-87a9-2fcaf82d5d90/oauth2/token";
        private static string POWER_BI_RESOURCE_URL = "https://analysis.windows.net/powerbi/api";
       
        #endregion
        #region Authontication
        
        
        protected  async Task<bool> AuthenticateAsync()
        {
            var TC = new TokenCache();
            var credential = new UserPasswordCredential(UserData.UserName, UserData.Password);
            var authenticationContext = new AuthenticationContext(POWER_BI_AUTHORITY_URL,false, TC);
           
            TokenResult = await authenticationContext.AcquireTokenAsync("https://analysis.windows.net/powerbi/api", "66bec1b2-4684-4a08-9f2b-b67216d4695a", credential);
            PTokenCredentials=new TokenCredentials(TokenResult.AccessToken,"Bearer");
            return true;
        }
        #endregion
        
        
    }
}
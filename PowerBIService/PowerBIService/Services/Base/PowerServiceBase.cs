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
        private AuthenticationResult TokenResult { get; set; }
        protected TokenCredentials PTokenCredentials { get; set;}
        
        
        
        protected static string POWER_BI_API_URL = "https://api.powerbi.com";

        private static string POWER_BI_AUTHORITY_URL ="https://login.microsoftonline.com/common/oauth2/authorize/";//"https://login.microsoftonline.com/"; //"https://login.windows.net/common/oauth2/authorize/";
        private static string POWER_BI_AUTHORITY_TOKEN_URL = "https://login.windows.net/470cec91-5a0e-47c7-87a9-2fcaf82d5d90/oauth2/token";
        private static string POWER_BI_RESOURCE_URL = "https://analysis.windows.net/powerbi/api";
       
        #endregion
        #region Authontication
        protected  async Task<bool> AuthenticateAsync()
        {
            TokenCache TC = new TokenCache();
            var clientCredential = new ClientCredential(UserData.ApplicationId, UserData.SecretId);
            AuthenticationContext ctx = null;
            
            if (!string.IsNullOrEmpty(UserData.TenantId))
            {
                ctx=new AuthenticationContext(POWER_BI_AUTHORITY_URL+UserData.TenantId,false,TC);
            }
            else
            {
                ctx=new AuthenticationContext(POWER_BI_AUTHORITY_URL+"/common");
                if (ctx.TokenCache.Count > 0)
                {
                    var cacheToken = ctx.TokenCache.ReadItems().First().TenantId;
                    ctx = new AuthenticationContext(POWER_BI_AUTHORITY_URL+cacheToken ,false,TC);
                }
            }
            try
            {
                TokenResult = await ctx.AcquireTokenAsync(POWER_BI_RESOURCE_URL,clientCredential);
                PTokenCredentials=new TokenCredentials(TokenResult.AccessToken,TokenResult.AccessTokenType);
                
                
               return true;
            }
            catch (AdalSilentTokenAcquisitionException silentExp)
            {
                throw silentExp;
            }
            catch (Exception genExp)
            {
                throw genExp;
            }
        }
        #endregion
        
        
    }
}
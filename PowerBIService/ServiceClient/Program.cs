using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PowerBIService.Common;
using ServiceCommon.Contract;

namespace ServiceClient
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var service=new PowerBIService.Services.Implementation.PowerService();
            var dd= Task.Run(async () => await service.CloneReports(new CloneReportRequest
            {
               CloneReports = new List<CloneReport>
               {
                   new CloneReport
                   {
                       ParentReportId = "SepalReport2",
                       CloneReportName = "NewCloneReport",
                       WebApiEndPoint = "https://gist.githubusercontent.com/curran/a08a1080b88344b0c8a7/raw/d546eaee765268bf2f487608c537c05e22e4b221/iris.csv"
                   }

               }.ToArray(),
               Credential = new UserCredentials
               {
                    TenantId = "470cec91-5a0e-47c7-87a9-2fcaf82d5d90",
                    SecretId =  "82(t[}]Ee+y&+GvT8[tjh+;U9[|x;",
                    ApplicationId ="66bec1b2-4684-4a08-9f2b-b67216d4695a",
                    Password = "Scala@1234",
                    UserName = "bkaluarachchi@assetic.com"
                },
               ClientWorkSpaceId = "SepalCloner",
               ParentWorkSpaceId = "My Workspace"
               
            })).ConfigureAwait(false);
            dd.GetAwaiter().GetResult(); 
            Console.WriteLine("Hello World!");
        }
    }
}
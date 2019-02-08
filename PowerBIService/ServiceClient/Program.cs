using System;
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

//         service.EmbedReport(new UserData
//         {
//             TenantId = "470cec91-5a0e-47c7-87a9-2fcaf82d5d90",
//             SecretId =  "ALYbNqinTaY8IU+q4uxWYgvGSdbBFyHeVJqfx1mb910=",
//             ApplicationId = "64f6409f-9683-42b5-9949-77a91767838e"
//         });



            var dd= Task.Run(async () => await service.CreateGroup(new GroupCreateRequest
            {
                GroupName = "TestGroup",
                Credential = new UserData
                {
                    TenantId = "470cec91-5a0e-47c7-87a9-2fcaf82d5d90",
                    SecretId =  "5gKtFOHQjj-!YUj]ptH",
                    ApplicationId ="66bec1b2-4684-4a08-9f2b-b67216d4695a",
                    Password = "Scala@1234",
                    UserName = "bkaluarachchi@assetic.com"
                }
               
            })).ConfigureAwait(false);
            dd.GetAwaiter().GetResult(); 
         
//           service.EmbedReport(new UserData
//           {
//               ApiUrl = "https://api.powerbi.com/",
//               ReportId ="" 
//               
//           });
//           
           
           
            Console.WriteLine("Hello World!");
        }
    }
}
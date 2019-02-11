using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using PowerBIService.Common;
using WebClientDemo.Models;
using WebGrease.Css.Extensions;

namespace WebClientDemo.Controllers
{
    public class HomeController : Controller
    {
        private UserData Credential;

        public HomeController()
        {
            Credential=  new UserData
            {
                TenantId = "470cec91-5a0e-47c7-87a9-2fcaf82d5d90",
                SecretId = "82(t[}]Ee+y&+GvT8[tjh+;U9[|x;",
                ApplicationId = "66bec1b2-4684-4a08-9f2b-b67216d4695a",
                Password = "Scala@1234",
                UserName = "bkaluarachchi@assetic.com"
            };
        }
        
        public ActionResult Index()
        {
           ViewBag.Title = "Home Page";
         
           var service=new PowerBIService.Services.Implementation.PowerService();
           var groups = Task.Run(async () => await service.GetAllGroups(Credential)).ConfigureAwait(false);
           var data=groups.GetAwaiter().GetResult();
           var list = new List<SelectListItem>();
           data.ForEach(s =>
           {
               list.Add(new SelectListItem{Value = s.Id,Text = s.Name});
           });
           var dataVm = new GroupsVM {GroupFromList = list.ToArray(),GroupToList =list.ToArray() };
           return View(dataVm);
        }
    }
}

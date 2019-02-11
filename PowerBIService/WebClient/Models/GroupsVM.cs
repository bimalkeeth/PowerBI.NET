using System.Web.Mvc;

namespace WebClient.Models
{
    public class GroupsVM
    {
        public SelectListItem[] GroupFromList { get; set; }
        public SelectListItem[] GroupToList { get; set; }
    }
}
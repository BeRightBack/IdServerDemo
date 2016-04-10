using cloudscribe.Web.Pagination;
using IdServerDemo.Models;

namespace IdServerDemo.ViewModels
{
    public class UsersViewModel
    {
        public UsersViewModel()
        {
            Paging = new PaginationSettings();
        }
        public string Query { get; set; } = string.Empty;

        public IPagedList<ApplicationUser> Users { get; set; } = null;

        public PaginationSettings Paging { get; set; }
    }
}

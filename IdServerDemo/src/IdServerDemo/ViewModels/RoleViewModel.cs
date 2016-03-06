using System.ComponentModel.DataAnnotations;

namespace IdServerDemo.ViewModels
{
    public class RoleViewModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

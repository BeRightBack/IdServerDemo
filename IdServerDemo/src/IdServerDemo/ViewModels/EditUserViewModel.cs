using Microsoft.AspNet.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdServerDemo.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }


        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "BirthDate (MM/dd/yyyy)")]
        public DateTime BirthDate { get; set; }

        // Add the new address properties:
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }

        // Use a sensible display name for views:
        [Display(Name = "PostalCode")]
        public string PostalCode { get; set; }

        public IEnumerable<SelectListItem> RolesList { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}

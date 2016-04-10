using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdServerDemo.ViewModels.Manage
{
    public class ManageUserViewModel
    {
        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public int Id { get; set; }

        [Required(ErrorMessage = "An Email is required")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A UserName is required")]
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Your First Name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Your Last Name  is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Your BirthDate is required")]
        [DataType(DataType.Date)]
        [Display(Name = "BirthDate (MM/dd/yyyy)")]
        public DateTime BirthDate { get; set; }

        // Add the new address properties:
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        [Required(ErrorMessage = "Your Country is required")]
        [Display(Name = "Country")]
        public string Country { get; set; }

        // Use a sensible display name for views:
        [Display(Name = "PostalCode")]
        public string PostalCode { get; set; }

        public bool BrowserRemembered { get; set; }
    }
}

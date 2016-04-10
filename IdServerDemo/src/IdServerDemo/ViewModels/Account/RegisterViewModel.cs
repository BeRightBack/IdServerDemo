using System;
using System.ComponentModel.DataAnnotations;

namespace IdServerDemo.ViewModels.Account
{
    public class RegisterViewModel
    {
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

        [Required(ErrorMessage = "A Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

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

        public bool EmailConfirmed { get; set; } = false;
    }
}

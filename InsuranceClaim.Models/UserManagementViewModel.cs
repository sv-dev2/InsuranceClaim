using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class UserManagementViewModel
    {
        public int Id { get; set; }
        public decimal CustomerId { get; set; }
        public string UserID { get; set; }
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Please enter email address.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string EmailAddress { get; set; }
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Please enter Country Code and phone number.")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number.")]
        public string PhoneNumber { get; set; }
        public string Branch { get; set; }
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Please enter first name.")]
        [MaxLength(30, ErrorMessage = "First name must be less than 30 characters long.")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Please enter last name.")]
        [MaxLength(30, ErrorMessage = "Last name must be less than 30 characters long.")]
        public string LastName { get; set; }
        [Display(Name = "Address1")]
        [Required(ErrorMessage = "Please enter address 1.")]
        [MaxLength(100, ErrorMessage = "Address 1 must be less than 100  characters long.")]
        public string AddressLine1 { get; set; }
        [Display(Name = "Address2")]
        [Required(ErrorMessage = "Please enter address 2.")]
        [MaxLength(100, ErrorMessage = "Address 2 must be less than 100  characters long.")]
        public string AddressLine2 { get; set; }
        [Display(Name = "City")]
        [Required(ErrorMessage = "Please enter city.")]
        [MaxLength(25, ErrorMessage = "City must be less than 25 characters long.")]
        public string City { get; set; }
        [Display(Name = "National Identification Number")]
        [Required(ErrorMessage = "Please enter National Identification Number.")]
        [MaxLength(25, ErrorMessage = "State must be less than 25 characters long.")]
        public string NationalIdentificationNumber { get; set; }
        [Display(Name = "ZipCode")]
        //[Required(ErrorMessage = "Please enter zip code.")]
        //[RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid Zip.")]
        public string Zipcode { get; set; }
        [Display(Name = "Country")]
        //[Required(ErrorMessage = "Please enter country.")]
        [MaxLength(25, ErrorMessage = "Country must be less than 25 characters long.")]
        public string Country { get; set; }
        [Display(Name = "Date Of Birth")]
        [Required(ErrorMessage = "Please enter date Of birth .")]
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool? IsWelcomeNoteSent { get; set; }
        public bool? IsPolicyDocSent { get; set; }
        public bool? IsLicenseDiskNeeded { get; set; }
        public bool? IsOTPConfirmed { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        [Required(ErrorMessage = "Please enter Country Code and phone number.")]
        [RegularExpression(@"^\+\d{1,3}$", ErrorMessage = "Not a valid Country Code .")]
        public string CountryCode { get; set; }
        public string role { get; set; }

        public int WorkTypeId { get; set; }

        //public bool IsUpdatePresonal { get; set; }


    }
}

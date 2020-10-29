using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace InsuranceClaim.Models
{
    public class CustomerModel
    {
        public int Id { get; set; }
        public decimal CustomerId { get; set; }
        public string UserID { get; set; }
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Please Enter Email Address.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string EmailAddress { get; set; }
        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Please Enter Country Code and Phone Number.")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "Enter Valid Phone Number")]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number.")]
        public string PhoneNumber { get; set; }
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Please Enter First Name.")]
        [MaxLength(30, ErrorMessage = "First name must be less than 30 characters long.")]
        public string FirstName { get; set; }
        [Display(Name = "Surname")]
        [Required(ErrorMessage = "Please Enter Last Name.")]
        [MaxLength(30, ErrorMessage = "Last name must be less than 30 characters long.")]
        public string LastName { get; set; }
        [Display(Name = "Address")]
        [Required(ErrorMessage = "Please Enter Address 1.")]
        [MaxLength(100, ErrorMessage = "Address 1 must be less than 100  characters long.")]
        public string AddressLine1 { get; set; }
        [Display(Name = "Surburb")]
        [Required(ErrorMessage = "Please Enter Address 2.")]
        [MaxLength(100, ErrorMessage = "Address 2 must be less than 100  characters long.")]
        public string AddressLine2 { get; set; }
        public string Branch { get; set; }
        public string HdnBrach { get; set; }
        [Display(Name = "City")]
        [Required(ErrorMessage = "Please Enter City.")]
        [MaxLength(25, ErrorMessage = "City must be less than 25 characters long.")]
        public string City { get; set; }
        [Display(Name = "National Identification Number")]
        [Required(ErrorMessage = " Please Enter National Identification Number")]
       
        // [RegularExpression(@"^([0-9]{2}-[0-9]{6,7}[a-zA-Z]{1}[0-9]{2})$", ErrorMessage = "Not a Valid Identification Number")]        
        public string NationalIdentificationNumber { get; set; }
        [Display(Name = "Zip Code")]
        //[Required(ErrorMessage = "Please enter zip code.")]
        //[RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid Zip.")]
        public string Zipcode { get; set; }
        [Display(Name = "Country")]
        //[Required(ErrorMessage = "Please enter country.")]
        [MaxLength(25, ErrorMessage = "Country must be less than 25 characters long.")]
        public string Country { get; set; }
        [Display(Name = "Date Of Birth")]
        [Required(ErrorMessage = "Please Enter Date Of Birth.")]
        public DateTime? DateOfBirth { get; set; }
        [Required(ErrorMessage = "Please Select Gender.")]
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
        //[Required(ErrorMessage = "Please enter Country Code and phone number.")]
        //[RegularExpression(@"^\+\d{1,3}$", ErrorMessage = "Not a valid Country Code .")]
        public string CountryCode { get; set; }
        public string role { get; set; }
        public bool IsCustomEmail { get; set; }
        public string UserRoleName { get; set; }
        public string ErrorMsg { get; set; }

        public string AgentLogo { get; set; }

        public string AgentWhatsapp {get; set;}

        public string AgentBranch { get; set; }

        public string Profile { get; set; }

        /// <summary>
        /// corporate opttion to agent
        /// </summary>
        /// 
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Company Email")]
        public string CompanyEmail { get; set; }

        [Display(Name = "Company Address")]
        public string CompanyAddress { get; set; }

        [Display(Name = "Company Phone")]
        public string CompanyPhone { get; set; }

        [Display(Name = "Company City")]
        public string CompanyCity { get; set; }

        [Display(Name = "BusinessId ")]
        public string CompanyBusinessId { get; set; }
        public bool IsCorporate { get; set; }

        public string Corporate { get; set; }

        [Display(Name = "Location")]
        public int WorkTypeId { get; set; }
      
        public string WorkDesc { get; set; }


    }
}

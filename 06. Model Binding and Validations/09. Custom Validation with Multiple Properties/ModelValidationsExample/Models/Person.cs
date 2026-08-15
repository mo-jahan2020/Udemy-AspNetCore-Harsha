using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using ModelValidationsExample.CustomValidators;

namespace ModelValidationsExample.Models
{
    public class Person
    {
        [Required(ErrorMessage = "{0} can't be empty or null")]
        [Display(Name = "Person Name")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "{0} should be between {2} and {1} characters long")]
        [RegularExpression("^[A-Za-z. ]+$", ErrorMessage = "{0} should contain only alphabets, space and dot (.)")]
        public string? PersonName { get; set; }//ali hadi


        [EmailAddress(ErrorMessage = "{0} should be a proper email address")]
        [Required(ErrorMessage = "{0} can't be blank")]
        public string? Email { get; set; }//a@a.com


        [Phone(ErrorMessage = "{0} should contain 10 digits")]
        //[ValidateNever]
        public string? Phone { get; set; }//9187116544


        [Required(ErrorMessage = "{0} can't be blank")]
        public string? Password { get; set; }//Ali110@


        [Required(ErrorMessage = "{0} can't be blank")]
        [Compare("Password", ErrorMessage = "{0} and {1} do not match")]
        [Display(Name = "Re-enter Password")]
        public string? ConfirmPassword { get; set; }//Ali110@


        [Range(0, 999.99, ErrorMessage = "{0} should be between ${1} and ${2}")]
        public double? Price { get; set; }//10


        //[MinimumYearValidator(2005, ErrorMessage = "Date of Birth should not be newer than Jan 01, {0}")]
        [MinimumYearValidator(2005)]
        public DateTime? DateOfBirth { get; set; }//2004/01/29 12:09:00


        public DateTime? FromDate { get; set; }//2004/01/21

        [DateRangeValidator("FromDate", ErrorMessage = "'From Date' should be older than or equal to 'To date'")]
        public DateTime? ToDate { get; set; }//2005/01/21

        public override string ToString()
        {
            return $"Person object - Person name: {PersonName}, Email: {Email}, Phone: {Phone}, Password: {Password}, Confirm Password: {ConfirmPassword}, Price: {Price}, DateOfBirth: {DateOfBirth}, FromDate:{FromDate} , ToDate:{ToDate}";
        }
    }
}

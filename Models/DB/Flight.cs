using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace agancywebProject.Models.DB
{
    public class Flight : IValidatableObject
    {
        [Key]
        public int Flight_Id { get; set; }

        [Display(Name = "مبدا ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public string? Origin { get; set; }

        [Display(Name = "مقصد  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public string? distination { get; set; }

        [Display(Name = "تاریخ رفت  ")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public DateTime startdate { get; set; }

        [Display(Name = "قیمت  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public int price { get; set; }

        [Display(Name = "ظرفیت  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public int capacity { get; set; }

        [Display(Name = "ساعت رفت  ")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public DateTime entrytime { get; set; }

        [Display(Name = "کلاس پرواز  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public string? flightClass { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (price <= 0)
            {
                yield return new ValidationResult(
                    Errormsg.Msg(validationContext, "قیمت باید بیشتر از صفر باشد."),
                    new[] { nameof(price) });
            }

            if (capacity <= 0)
            {
                yield return new ValidationResult(
                    Errormsg.Msg(validationContext, "ظرفیت باید حداقل ۱ باشد."),
                    new[] { nameof(capacity) });
            }
        }
    }
}
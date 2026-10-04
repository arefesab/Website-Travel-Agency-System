using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace agancywebProject.Models.DB
{
    public class Hotel : IValidatableObject
    {
        [Key]
        public int Hotel_Id { get; set; }

        [Display(Name = "نام هتل  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public string? name { get; set; }

        [Display(Name = "استان  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public string? city { get; set; }

        [Display(Name = "تاریخ خالی بودن از  ")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public DateTime startdate { get; set; }

        [Display(Name = "تاریخ خالی بودن تا  ")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public DateTime finishdate { get; set; }

        [Display(Name = "قیمت (هر شب، تومان)  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public int price { get; set; }

        [Display(Name = "آدرس  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public string? address { get; set; }

        [Display(Name = "ستاره  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public int star { get; set; }

        [Display(Name = "وعده غذایی  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public string? meal { get; set; }

        public static readonly (string Value, string Label)[] MealOptions = new[]
        {
            ("breakfast", "صبحانه"),
            ("lunch", "ناهار"),
            ("dinner", "شام"),
            ("snack", "میان‌وعده"),
        };

        [NotMapped]
        public string MealDisplay => FormatMealDisplay(meal);

        public static string FormatMealDisplay(string? meal)
        {
            if (string.IsNullOrWhiteSpace(meal)) return string.Empty;

            var labelByValue = MealOptions.ToDictionary(o => o.Value, o => o.Label);
            var selected = meal
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(v => labelByValue.TryGetValue(v, out var label) ? label : v);

            return string.Join("، ", selected);
        }

        [Display(Name = "توضیحات هتل  ")]
        public string? description { get; set; }

        [Display(Name = "امکانات هتل (هر خط یک مورد)  ")]
        public string? amenities { get; set; }

        [Display(Name = "لینک عکس‌های هتل (هر خط یک لینک)  ")]
        public string? photoUrls { get; set; }

        [Display(Name = "لینک عکس‌های اتاق (هر خط یک لینک)  ")]
        public string? roomPhotoUrls { get; set; }

        [Display(Name = "نام اتاق  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public string? roomName { get; set; }

        [Display(Name = "ظرفیت اتاق (نفر)  ")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        public int roomCapacity { get; set; }

        [Display(Name = " تخفیف (اختیاری)  ")]
        [Range(0, 90, ErrorMessage = "درصد تخفیف باید بین ۰ تا ۹۰ باشد.")]
        public int? DiscountPercent { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (finishdate <= startdate)
            {
                yield return new ValidationResult(
                    Errormsg.Msg(validationContext, "تاریخ پایان باید بعد از تاریخ شروع باشد."),
                    new[] { nameof(finishdate) });
            }

            if (price <= 0)
            {
                yield return new ValidationResult(
                    Errormsg.Msg(validationContext, "قیمت باید بیشتر از صفر باشد."),
                    new[] { nameof(price) });
            }

            if (star < 1 || star > 5)
            {
                yield return new ValidationResult(
                    Errormsg.Msg(validationContext, "تعداد ستاره باید بین ۱ تا ۵ باشد."),
                    new[] { nameof(star) });
            }

            if (roomCapacity < 1)
            {
                yield return new ValidationResult(
                    Errormsg.Msg(validationContext, "ظرفیت اتاق باید حداقل ۱ نفر باشد."),
                    new[] { nameof(roomCapacity) });
            }
        }
    }
}

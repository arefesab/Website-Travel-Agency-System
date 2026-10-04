using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace agancywebProject.Models.DB
{
    [Microsoft.EntityFrameworkCore.Index(nameof(Authority))]
    public class FlightBooking : IValidatableObject
    {
        [Key]
        public int FlightBooking_Id { get; set; }

        [Required]
        public int Flight_Id { get; set; }

        [ForeignKey(nameof(Flight_Id))]
        public Flight? Flight { get; set; }

        [Display(Name = "تعداد صندلی")]
        [Range(1, 50, ErrorMessage = "تعداد صندلی باید بین ۱ تا ۵۰ باشد")]
        public int SeatCount { get; set; } = 1;

        [Display(Name = "نام و نام خانوادگی")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "شماره موبایل")]
        [Required(AllowEmptyStrings = false, ErrorMessage = Errormsg.RequairedMsg)]
        [RegularExpression(@"^0?9\d{9}$", ErrorMessage = "شماره موبایل را به‌صورت صحیح وارد کنید (مثلا 09121234567)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "توضیحات / درخواست خاص")]
        [StringLength(500)]
        public string? Note { get; set; }

        [Display(Name = "مبلغ کل (تومان)")]
        public int TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [StringLength(100)]
        public string? Authority { get; set; }

        public string? RefId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        public DateTime? LockedAt { get; set; }

        [NotMapped]
        public bool IsLockExpired =>
            Status == BookingStatus.Locked && (LockedAt == null || LockedAt.Value < BookingRules.LockCutoff());

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SeatCount <= 0)
            {
                yield return new ValidationResult(
                    Errormsg.Msg(validationContext, "تعداد صندلی باید حداقل ۱ باشد."),
                    new[] { nameof(SeatCount) });
            }
        }
    }
}

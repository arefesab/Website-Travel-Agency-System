using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace agancywebProject.Models.DB
{
    public enum BookingStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Cancelled = 3,
        Locked = 4
    }

    [Microsoft.EntityFrameworkCore.Index(nameof(Authority))]
    [Microsoft.EntityFrameworkCore.Index(nameof(Hotel_Id), nameof(CheckIn), nameof(CheckOut))]
    public class Booking : IValidatableObject
    {
        [Key]
        public int Booking_Id { get; set; }

        [Required]
        public int Hotel_Id { get; set; }

        [ForeignKey(nameof(Hotel_Id))]
        public Hotel? Hotel { get; set; }

        [Display(Name = "تاریخ ورود")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime CheckIn { get; set; }

        [Display(Name = "تاریخ خروج")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime CheckOut { get; set; }

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

        [Display(Name = "تعداد شب")]
        public int Nights { get; set; }

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
            if (CheckOut <= CheckIn)
            {
                yield return new ValidationResult(
                    Errormsg.Msg(validationContext, "تاریخ خروج باید بعد از تاریخ ورود باشد."),
                    new[] { nameof(CheckOut) });
            }
        }
    }
}


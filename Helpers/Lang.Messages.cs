using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace agancywebProject.Helpers
{
    public static partial class Lang
    {
        public static string Msg(HttpContext? context, string fa, params object[] args)
        {
            var text = fa;
            if (context != null && IsEnglish(context) && TryEnglish(fa, out var en))
            {
                text = en;
            }
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }

        public static string FlightClass(HttpContext context, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            if (!IsEnglish(context)) return value;

            var key = Norm(value).Replace(" ", string.Empty);
            switch (key)
            {
                case "اکونومی": return "Economy";
                case "بیزینس":
                case "بیزینسکلاس": return "Business class";
                case "فرست":
                case "فرستکلاس": return "First class";
                default: return Fin(context, value);
            }
        }

        public static bool TryEnglish(string? fa, out string en)
        {
            en = string.Empty;
            if (string.IsNullOrWhiteSpace(fa)) return false;

            var key = fa.Trim();
            if (EnglishMessages.TryGetValue(key, out var exact))
            {
                en = exact;
                return true;
            }

            foreach (var (pattern, replacement) in DynamicRules)
            {
                var m = pattern.Match(key);
                if (m.Success)
                {
                    en = m.Result(replacement);
                    return true;
                }
            }
            return false;
        }

        private static readonly (Regex pattern, string replacement)[] DynamicRules =
        {
            (new Regex(@"^خطا در اتصال به درگاه پرداخت: (.*)$", RegexOptions.Singleline), "Error connecting to the payment gateway: $1"),
            (new Regex(@"^خطا در تایید پرداخت: (.*)$", RegexOptions.Singleline), "Error verifying the payment: $1"),
            (new Regex(@"^درگاه پرداخت درخواست را نپذیرفت \(کد (.*)\)\.$"), "The payment gateway rejected the request (code $1)."),
            (new Regex(@"^پرداخت تایید نشد \(کد (.*)\)\.$"), "The payment could not be verified (code $1)."),
        };

        private static readonly Dictionary<string, string> EnglishMessages = new Dictionary<string, string>
        {
            ["{0} را وارد نمایید"] = "Please enter {0}",
            ["شماره موبایل را به‌صورت صحیح وارد کنید (مثلا 09121234567)"] = "Please enter a valid mobile number (e.g. 09121234567)",
            ["تعداد صندلی باید بین ۱ تا ۵۰ باشد"] = "Seat count must be between 1 and 50",

            ["آدرس"] = "Address",
            ["استان"] = "City",
            ["امکانات هتل (هر خط یک مورد)"] = "Hotel amenities (one per line)",
            ["تاریخ خالی بودن از"] = "Available from",
            ["تاریخ خالی بودن تا"] = "Available until",
            ["تاریخ خروج"] = "Check-out date",
            ["تاریخ رفت"] = "Departure date",
            ["تاریخ ورود"] = "Check-in date",
            ["تعداد شب"] = "Number of nights",
            ["تعداد صندلی"] = "Number of seats",
            ["توضیحات / درخواست خاص"] = "Notes / special request",
            ["توضیحات هتل"] = "Hotel description",
            ["ساعت رفت"] = "Departure time",
            ["ستاره"] = "Stars",
            ["شماره موبایل"] = "Phone number",
            ["ظرفیت"] = "Capacity",
            ["ظرفیت اتاق (نفر)"] = "Room capacity (persons)",
            ["قیمت"] = "Price",
            ["قیمت (هر شب، تومان)"] = "Price (per night, Toman)",
            ["لینک عکس‌ها (هر خط یک لینک)"] = "Photo links (one per line)",
            ["مبدا"] = "Origin",
            ["مبلغ کل (تومان)"] = "Total price (Toman)",
            ["مقصد"] = "Destination",
            ["نام اتاق"] = "Room name",
            ["نام هتل"] = "Hotel name",
            ["نام و نام خانوادگی"] = "Full name",
            ["وعده غذایی"] = "Meals",
            ["کلاس پرواز"] = "Flight class",

            ["تاریخ پایان باید بعد از تاریخ شروع باشد."] = "The end date must be after the start date.",
            ["قیمت باید بیشتر از صفر باشد."] = "Price must be greater than zero.",
            ["تعداد ستاره باید بین ۱ تا ۵ باشد."] = "Stars must be between 1 and 5.",
            ["ظرفیت اتاق باید حداقل ۱ نفر باشد."] = "Room capacity must be at least 1 person.",
            ["ظرفیت باید حداقل ۱ باشد."] = "Capacity must be at least 1.",
            ["تعداد صندلی باید حداقل ۱ باشد."] = "Seat count must be at least 1.",
            ["تاریخ خروج باید بعد از تاریخ ورود باشد."] = "Check-out date must be after check-in date.",

            ["نام کاربری یا رمز عبور اشتباه است"] = "Incorrect username or password",
            ["هیچ فایلی ارسال نشده است."] = "No file was uploaded.",
            ["فرمت فایل‌ها پشتیبانی نمی‌شود (فقط jpg, jpeg, png, webp, gif)."] = "Unsupported file format (only jpg, jpeg, png, webp, gif).",
            ["این هتل رزرو پرداخت‌شده یا در حال پرداخت دارد و قابل حذف نیست."] = "This hotel has paid or in-progress bookings and cannot be deleted.",
            ["این پرواز رزرو پرداخت‌شده یا در حال پرداخت دارد و قابل حذف نیست."] = "This flight has paid or in-progress bookings and cannot be deleted.",

            ["تاریخ برگشت باید بعد از تاریخ رفت باشد."] = "Return date must be after departure date.",
            ["تاریخ نمی‌تواند در گذشته باشد."] = "The date cannot be in the past.",
            ["تاریخ ورود نمی‌تواند در گذشته باشد."] = "Check-in date cannot be in the past.",
            ["تاریخ رفت نمی‌تواند در گذشته باشد."] = "Departure date cannot be in the past.",
            ["تاریخ برگشت نمی‌تواند در گذشته باشد."] = "Return date cannot be in the past.",

            ["تاریخ ورود نمی‌تواند در گذشته باشد."] = "Check-in date cannot be in the past.",
            ["بازه انتخابی باید داخل تاریخ خالی بودن این هتل باشد."] = "The selected period must be within this hotel's available dates.",
            ["این بازه قبلا توسط شخص دیگری رزرو شده. لطفا بازه دیگری از تاریخ‌های خالی را انتخاب کنید."] = "This period has already been booked by someone else. Please choose another available period.",
            ["مبلغ کل این رزرو بیش از حد مجاز است. لطفا تعداد شب‌های کمتری انتخاب کنید."] = "The total price of this booking exceeds the allowed limit. Please choose fewer nights.",
            ["متاسفانه این بازه هم‌اکنون توسط شخص دیگری رزرو شد. لطفا بازه دیگری انتخاب کنید."] = "Unfortunately this period has just been booked by someone else. Please choose another period.",
            ["پرداخت شما انجام شد اما در این فاصله همین بازه توسط شخص دیگری رزرو شده است. لطفا با پشتیبانی تماس بگیرید تا مبلغ بازگردانده شود (کد پیگیری: {0})."] = "Your payment went through, but in the meantime this period was booked by someone else. Please contact support to get a refund (tracking code: {0}).",

            ["زمان این پرواز گذشته است و قابل رزرو نیست."] = "This flight has already departed and cannot be booked.",
            ["ظرفیت این پرواز تکمیل شده است."] = "This flight is fully booked.",
            ["متاسفانه ظرفیت این پرواز تکمیل شده است."] = "Unfortunately this flight is fully booked.",
            ["فقط {0} صندلی از این پرواز باقی مانده است."] = "Only {0} seat(s) are left on this flight.",
            ["مبلغ کل این رزرو بیش از حد مجاز است. لطفا تعداد صندلی کمتری انتخاب کنید."] = "The total price of this booking exceeds the allowed limit. Please choose fewer seats.",
            ["متاسفانه ظرفیت این پرواز هم‌اکنون توسط افراد دیگر تکمیل شد."] = "Unfortunately the remaining seats on this flight have just been taken by other passengers.",
            ["پرداخت شما انجام شد اما در این فاصله ظرفیت پرواز توسط افراد دیگر تکمیل شده است. لطفا با پشتیبانی تماس بگیرید تا مبلغ بازگردانده شود (کد پیگیری: {0})."] = "Your payment went through, but in the meantime the flight filled up. Please contact support to get a refund (tracking code: {0}).",

            ["اتصال به درگاه پرداخت برقرار نشد."] = "Could not connect to the payment gateway.",
            ["پرداخت توسط شما لغو شد."] = "The payment was cancelled.",
            ["پرداخت تایید نشد."] = "The payment could not be verified.",
            ["ارتباط با درگاه پرداخت برقرار نشد."] = "Could not connect to the payment gateway.",
            ["ارتباط با درگاه پرداخت برای تایید تراکنش برقرار نشد."] = "Could not reach the payment gateway to verify the transaction.",
        };
    }

    public class CookieStringLocalizer : IStringLocalizer
    {
        private readonly IHttpContextAccessor _accessor;

        public CookieStringLocalizer(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public LocalizedString this[string name] => Translate(name, Array.Empty<object>());

        public LocalizedString this[string name, params object[] arguments] => Translate(name, arguments);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            Enumerable.Empty<LocalizedString>();

        private LocalizedString Translate(string name, object[] arguments)
        {
            var context = _accessor.HttpContext;
            var text = name;
            var found = false;

            if (context != null && Lang.IsEnglish(context) && Lang.TryEnglish(name, out var en))
            {
                text = en;
                found = true;
            }

            var value = arguments != null && arguments.Length > 0 ? string.Format(text, arguments) : text;
            return new LocalizedString(name, value, resourceNotFound: !found);
        }
    }
}

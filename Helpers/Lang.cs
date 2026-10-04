using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace agancywebProject.Helpers
{
    public static partial class Lang
    {
        public const string CookieName = "site-lang";

        // The site's content is ALWAYS rendered in Persian (the source language).
        // English is provided by Google Website Translator (see Views/Shared/_GoogleTranslate.cshtml),
        // so the old hand-made translation / transliteration paths are switched off here.
        public static bool IsEnglish(HttpContext context)
        {
            return false;
        }

        // True when the visitor chose "English": the page is then translated by Google in the browser
        // and the layout is switched to LTR.
        public static bool IsTranslated(HttpContext context)
        {
            return context.Request.Cookies[CookieName] == "en";
        }

        public static string T(HttpContext context, string fa, string en)
        {
            return IsEnglish(context) ? en : fa;
        }

        public static string ToLatinDigits(string? s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (ch >= '\u06F0' && ch <= '\u06F9') sb.Append((char)('0' + (ch - '\u06F0')));
                else if (ch >= '\u0660' && ch <= '\u0669') sb.Append((char)('0' + (ch - '\u0660')));
                else sb.Append(ch);
            }
            return sb.ToString();
        }

        public static string Num(HttpContext context, string? s)
        {
            return IsEnglish(context) ? ToLatinDigits(s) : (s ?? string.Empty);
        }

        public static string Fin(HttpContext context, string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return IsEnglish(context) ? Transliterate(s) : s;
        }

        public static string Amenity(HttpContext context, string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (!IsEnglish(context)) return s;
            var key = Norm(s);
            if (Amenities.TryGetValue(key, out var en)) return en;
            return Transliterate(s);
        }

        public static string Meal(HttpContext context, string? meal, string faDisplay)
        {
            if (!IsEnglish(context)) return faDisplay;
            if (string.IsNullOrWhiteSpace(meal)) return string.Empty;
            var parts = new List<string>();
            foreach (var raw in meal.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries))
            {
                switch (raw)
                {
                    case "breakfast": parts.Add("Breakfast"); break;
                    case "lunch": parts.Add("Lunch"); break;
                    case "dinner": parts.Add("Dinner"); break;
                    case "snack": parts.Add("Snack"); break;
                    default: parts.Add(Transliterate(raw)); break;
                }
            }
            return string.Join(", ", parts);
        }

        private static string Norm(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var ch in ToLatinDigits(s))
            {
                if (ch == '\u200c' || ch == '\u200d') continue;
                if (ch == '\u064A') sb.Append('\u06CC');
                else if (ch == '\u0643') sb.Append('\u06A9');
                else sb.Append(ch);
            }
            return sb.ToString().Trim();
        }

        private static bool IsPersianLetter(char c)
        {
            return (c >= '\u0621' && c <= '\u063A') || (c >= '\u0641' && c <= '\u064A')
                   || c == '\u067E' || c == '\u0686' || c == '\u0698' || c == '\u06A9' || c == '\u06AF' || c == '\u06CC'
                   || c == '\u200c';
        }

        public static string Transliterate(string s)
        {
            s = ToLatinDigits(s);
            var result = new StringBuilder();
            var word = new StringBuilder();
            foreach (var ch in s)
            {
                if (IsPersianLetter(ch))
                {
                    word.Append(ch);
                    continue;
                }
                if (word.Length > 0) { result.Append(TransliterateWord(word.ToString())); word.Clear(); }
                if (ch == '\u060C') result.Append(',');
                else if (ch == '\u061F') result.Append('?');
                else result.Append(ch);
            }
            if (word.Length > 0) result.Append(TransliterateWord(word.ToString()));
            return result.ToString();
        }

        private static string TransliterateWord(string raw)
        {
            var w = Norm(raw);
            if (w.Length == 0) return string.Empty;
            if (Words.TryGetValue(w, out var known)) return known;
            return Capitalize(Fallback(w));
        }

        private static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        private static readonly Dictionary<char, string> Cons = new Dictionary<char, string>
        {
            ['ب'] = "b", ['پ'] = "p", ['ت'] = "t", ['ث'] = "s", ['ج'] = "j", ['چ'] = "ch", ['ح'] = "h",
            ['خ'] = "kh", ['د'] = "d", ['ذ'] = "z", ['ر'] = "r", ['ز'] = "z", ['ژ'] = "zh", ['س'] = "s",
            ['ش'] = "sh", ['ص'] = "s", ['ض'] = "z", ['ط'] = "t", ['ظ'] = "z", ['غ'] = "gh", ['ف'] = "f",
            ['ق'] = "gh", ['ک'] = "k", ['گ'] = "g", ['ل'] = "l", ['م'] = "m", ['ن'] = "n",
        };

        private static string Fallback(string w)
        {
            var toks = new List<KeyValuePair<string, bool>>();
            int n = w.Length;
            for (int i = 0; i < n; i++)
            {
                char ch = w[i];
                char prev = i > 0 ? w[i - 1] : '\0';
                char next = i < n - 1 ? w[i + 1] : '\0';
                bool prevIsVowelLetter = prev == 'ا' || prev == 'آ' || prev == 'و' || prev == 'ی';
                if (ch == 'ا' || ch == 'آ') toks.Add(new KeyValuePair<string, bool>("a", true));
                else if (ch == 'و')
                {
                    if (i == 0 || next == 'ا' || next == 'آ' || next == 'ی') toks.Add(new KeyValuePair<string, bool>("v", false));
                    else toks.Add(new KeyValuePair<string, bool>("u", true));
                }
                else if (ch == 'ی')
                {
                    if (i == 0 || prevIsVowelLetter) toks.Add(new KeyValuePair<string, bool>("y", false));
                    else toks.Add(new KeyValuePair<string, bool>("i", true));
                }
                else if (ch == 'ه')
                {
                    if (i == n - 1 && i > 0 && !prevIsVowelLetter) toks.Add(new KeyValuePair<string, bool>("eh", true));
                    else toks.Add(new KeyValuePair<string, bool>("h", false));
                }
                else if (ch == 'ع') { if (i == 0) toks.Add(new KeyValuePair<string, bool>("a", true)); }
                else if (ch == 'ئ') toks.Add(new KeyValuePair<string, bool>("e", true));
                else if (ch == 'ؤ') toks.Add(new KeyValuePair<string, bool>("o", true));
                else if (Cons.TryGetValue(ch, out var c)) toks.Add(new KeyValuePair<string, bool>(c, false));
                else if (ch == '\u0621' || ch == '\u0654' || ch == '\u0651' || ch == '\u064E' || ch == '\u064F' || ch == '\u0650' || ch == '\u0652') { }
                else toks.Add(new KeyValuePair<string, bool>(ch.ToString(), true));
            }

            var sb = new StringBuilder();
            int idx = 0;
            while (idx < toks.Count)
            {
                if (toks[idx].Value) { sb.Append(toks[idx].Key); idx++; continue; }
                int start = idx;
                while (idx < toks.Count && !toks[idx].Value) idx++;
                int len = idx - start;
                bool atWordStart = start == 0;
                for (int k = 1; k <= len; k++)
                {
                    sb.Append(toks[start + k - 1].Key);
                    if (k < len && ((atWordStart && k % 2 == 1) || (!atWordStart && k % 2 == 0))) sb.Append('e');
                }
            }
            return sb.ToString();
        }

        private static Dictionary<string, string> Make(params string[] pairs)
        {
            var d = new Dictionary<string, string>();
            for (int i = 0; i + 1 < pairs.Length; i += 2) d[Norm(pairs[i])] = pairs[i + 1];
            return d;
        }

        private static readonly Dictionary<string, string> Words = Make(
            "تهران", "Tehran", "اهواز", "Ahvaz", "اصفهان", "Isfahan", "بندرعباس", "Bandar Abbas", "مشهد", "Mashhad",
            "شیراز", "Shiraz", "کرمان", "Kerman", "عسلویه", "Asaluyeh", "کیش", "Kish", "تبریز", "Tabriz", "قشم", "Qeshm",
            "استانبول", "Istanbul", "پاریس", "Paris", "هامبورگ", "Hamburg", "تورنتو", "Toronto", "مادرید", "Madrid",
            "دبی", "Dubai", "لندن", "London", "فرانکفورت", "Frankfurt", "مونترآل", "Montreal", "آنکارا", "Ankara",
            "میلان", "Milan", "وین", "Vienna", "ونکوور", "Vancouver", "مالدیو", "Maldives", "فرانسه", "France",
            "ترکیه", "Turkey", "روسیه", "Russia", "کردستان", "Kurdistan", "رشت", "Rasht", "ساری", "Sari", "یزد", "Yazd",
            "قزوین", "Qazvin", "اراک", "Arak", "همدان", "Hamedan", "زاهدان", "Zahedan", "بوشهر", "Bushehr",
            "ارومیه", "Urmia", "سنندج", "Sanandaj", "کرمانشاه", "Kermanshah", "اردبیل", "Ardabil", "گرگان", "Gorgan",
            "قم", "Qom", "کرج", "Karaj", "چابهار", "Chabahar", "بابلسر", "Babolsar", "رامسر", "Ramsar", "ایران", "Iran",
            "هتل", "Hotel", "هتل‌ها", "Hotels", "مهمانسرا", "Guest House", "مهمانپذیر", "Guest House", "رستوران", "Restaurant",
            "پارسیان", "Parsian", "کوروش", "Kourosh", "داریوش", "Darioush", "ستاره", "Setareh", "بزرگ", "Bozorg",
            "گرند", "Grand", "پالاس", "Palace", "پردیس", "Pardis", "لاله", "Laleh", "عباسی", "Abbasi", "شایان", "Shayan",
            "ترنج", "Toranj", "شقایق", "Shaghayegh", "مرجان", "Marjan", "مهر", "Mehr", "آسمان", "Asman", "ونوس", "Venus",
            "البرز", "Alborz", "ویرا", "Vira", "هروی", "Heravi", "محتشم", "Mohtasham",
            "دریا", "Darya", "ساحل", "Sahel", "خلیج", "Khalij", "فارس", "Fars", "گلستان", "Golestan", "پارس", "Pars",
            "هما", "Homa", "آریا", "Arya", "نگین", "Negin", "ارم", "Eram", "الماس", "Almas", "طلایی", "Talaei",
            "نقره", "Noghreh", "سپید", "Sepid", "سفید", "Sefid", "نیلوفر", "Niloofar", "بهار", "Bahar", "تابستان", "Tabestan",
            "کاج", "Kaj", "چنار", "Chenar", "سرو", "Sarv", "اطلس", "Atlas", "رویال", "Royal", "پلاتین", "Platinum",
            "امیرکبیر", "Amirkabir", "فردوسی", "Ferdowsi", "حافظ", "Hafez", "سعدی", "Saadi", "انقلاب", "Enghelab",
            "آزادی", "Azadi", "ولیعصر", "Valiasr", "امام", "Emam", "خمینی", "Khomeini", "شهید", "Shahid", "دکتر", "Dr.",
            "شریعتی", "Shariati", "بهشتی", "Beheshti", "مطهری", "Motahhari", "جمهوری", "Jomhuri", "کریمخان", "Karimkhan",
            "خیابان", "Khiaban", "کوچه", "Kucheh", "بلوار", "Bolvar", "میدان", "Meydan", "پلاک", "Plak", "نبش", "Nabsh",
            "مرکزی", "Markazi", "ساحلی", "Sahli", "شهر", "Shahr", "استان", "Ostan", "جنب", "Janb", "روبروی", "Ruberuye",
            "بالاتر", "Balatar", "پایین‌تر", "Paintar", "ابتدای", "Ebteday", "انتهای", "Enteha-ye", "طبقه", "Tabagheh",
            "واحد", "Vahed", "منطقه", "Mantagheh", "شمالی", "Shomali", "جنوبی", "Jonubi", "شرقی", "Sharghi", "غربی", "Gharbi",
            "بین", "Beyn", "اول", "Avval", "دوم", "Dovvom", "سوم", "Sevvom", "چهارم", "Chaharom", "پنجم", "Panjom",
            "جزیره", "Jazireh", "بازار", "Bazar", "فرودگاه", "Foroodgah", "ترمینال", "Terminal", "ایستگاه", "Istgah",
            "پارک", "Park", "چهارراه", "Chaharrah", "سه‌راه", "Seh-rah", "بزرگراه", "Bozorgrah", "جاده", "Jadeh",
            "کنار", "Kenar", "مقابل", "Moghabel", "پشت", "Posht", "روبرو", "Ruberu", "نزدیک", "Nazdik", "و", "va",
            "اتاق", "Room", "تخته", "Bed", "سوئیت", "Suite", "استاندارد", "Standard",
            "لوکس", "Luxury", "دلوکس", "Deluxe", "خانوادگی", "Family", "دونفره", "Double", "یک‌نفره", "Single",
            "سه‌نفره", "Triple", "چهارنفره", "Quad", "اکونومی", "Economy", "بیزینس", "Business"
        );

        private static readonly Dictionary<string, string> Amenities = Make(
            "استخر", "Pool", "استخر سرپوشیده", "Indoor pool", "استخر روباز", "Outdoor pool",
            "باشگاه بدنسازی", "Gym", "باشگاه ورزشی", "Fitness center", "سالن ورزشی", "Fitness hall",
            "ترانسفر فرودگاهی", "Airport transfer", "ترانسفر", "Transfer", "وای فای", "Wi-Fi", "وایفای", "Wi-Fi",
            "وای‌فای", "Wi-Fi", "اینترنت", "Internet", "اینترنت رایگان", "Free internet", "وای فای رایگان", "Free Wi-Fi",
            "پارکینگ", "Parking", "پارکینگ رایگان", "Free parking", "صبحانه", "Breakfast", "صبحانه رایگان", "Free breakfast",
            "ناهار", "Lunch", "شام", "Dinner", "رستوران", "Restaurant", "کافی شاپ", "Coffee shop", "کافی‌شاپ", "Coffee shop",
            "کافه", "Cafe", "اسپا", "Spa", "سونا", "Sauna", "جکوزی", "Jacuzzi", "ماساژ", "Massage", "سرویس اتاق", "Room service",
            "روم سرویس", "Room service", "تهویه مطبوع", "Air conditioning", "کولر", "Air conditioner", "تلویزیون", "TV",
            "یخچال", "Fridge", "مینی بار", "Minibar", "مینی‌بار", "Minibar", "لابی", "Lobby", "آسانسور", "Elevator",
            "خشکشویی", "Dry cleaning", "لباسشویی", "Laundry", "مرکز تجاری", "Business center", "سالن کنفرانس", "Conference hall",
            "سالن همایش", "Conference hall", "زمین بازی کودکان", "Kids playground", "فضای بازی کودکان", "Kids play area",
            "ساحل خصوصی", "Private beach", "ساحل اختصاصی", "Private beach", "پذیرش ۲۴ ساعته", "24-hour reception",
            "پذیرش 24 ساعته", "24-hour reception", "پذیرش", "Reception", "امنیت ۲۴ ساعته", "24-hour security",
            "گاوصندوق", "Safe box", "سشوار", "Hair dryer", "اتو", "Iron", "چای ساز", "Tea maker", "کتری", "Kettle",
            "بالکن", "Balcony", "تراس", "Terrace", "منظره دریا", "Sea view", "منظره شهر", "City view", "فروشگاه", "Shop",
            "سوپرمارکت", "Supermarket", "صرافی", "Currency exchange", "آژانس مسافرتی", "Travel agency", "میز اطلاعات", "Information desk",
            "تاکسی", "Taxi", "تاکسی سرویس", "Taxi service", "دوچرخه", "Bicycle", "تنیس", "Tennis", "بیلیارد", "Billiards",
            "سینما", "Cinema", "مهدکودک", "Kindergarten", "نمازخانه", "Prayer room", "سالن غذاخوری", "Dining hall",
            "بوفه", "Buffet", "بوفه صبحانه", "Breakfast buffet", "قهوه", "Coffee", "چای", "Tea", "تلفن", "Telephone",
            "حمام", "Bathroom", "دوش", "Shower", "وان", "Bathtub", "پارک", "Park", "باغ", "Garden", "حیاط", "Yard"
        );
    }
}

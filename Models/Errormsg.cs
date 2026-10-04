using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace agancywebProject.Models
{
    public class Errormsg
    {
        public const string RequairedMsg = "{0} را وارد نمایید";

        public static string Msg(ValidationContext validationContext, string fa)
        {
            var accessor = validationContext.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
            return agancywebProject.Helpers.Lang.Msg(accessor?.HttpContext, fa);
        }
    }
}

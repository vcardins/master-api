#region



#endregion

using System.Text.RegularExpressions;

namespace MasterApi.Web.Extensions
{
    #region

    

    #endregion

    public static class StringExtensions
    {

        public static bool IsValidEmail(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var isValid = Regex.IsMatch(value, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            return isValid;
        }

        public static bool IsValidPhoneNumber(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var isValid = Regex.IsMatch(value, @"^[\\(]{0,1}([0-9]){3}[\\)]{0,1}[ ]?([^0-1]){1}([0-9]){2}[ ]?[-]?[ ]?([0-9]){4}[ ]*((x){0,1}([0-9]){1,5}){0,1}$", RegexOptions.IgnoreCase);
            return isValid;
        }
    }
}


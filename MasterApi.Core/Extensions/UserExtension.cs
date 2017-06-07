using System;
using System.Text.RegularExpressions;

namespace MasterApi.Core.Extensions
{

    public static class UserExtension
    {
        public static string GetPhotoPath(this string photoId, string username)
        {
            //var resolver = GlobalConfiguration.Configuration.DependencyResolver;
            //var _photoService = resolver.GetService(typeof(IPhotoSettings)) as IPhotoSettings;         
            //var cfg = ConfigurationManager.GetSection("Config//photos");
            //var photos = (IPhotoSettings)cfg.Photos;
            return !String.IsNullOrEmpty(photoId) ? photoId : null;
            //return !String.IsNullOrEmpty(photoId) ?
            //        String.Format("{0}/{1}", username, photoId) :
            //        "admin/avatar_default_medium.jpeg";
        }


        public static string GetFullName(this string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName)) return null;
            return string.Format("{0} {1}", firstName, !string.IsNullOrEmpty(lastName) ? lastName : string.Empty).Trim();
        }

        public static string GetDisplayName(this string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName)) return null;
            return string.Format("{0} {1}", firstName, !string.IsNullOrEmpty(lastName) ? lastName.Substring(0, 1) + "." : string.Empty).Trim();
        }

        public static string CleanPhoneNumber(this string phoneNumber)
        {
            var digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(phoneNumber, "");
        }
    }
}


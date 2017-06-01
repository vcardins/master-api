using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MasterApi.Services.Extensions
{
    public static class JsonExtensions
    {
        public static List<TCollection> GetCollectionFromJson<TCollection>(this TypeInfo type, string resourcePath)
        {
            var asm = type.Assembly;
            using (var s = asm.GetManifestResourceStream(resourcePath))
            {
                if (s == null) return null;
                using (var sr = new StreamReader(s))
                {
                    var json = sr.ReadToEnd();
                    var obj = (JArray)JsonConvert.DeserializeObject(json);
                    var converted = obj.ToObject<List<TCollection>>();
                    return converted;
                }
            }
        }

        public static Dictionary<string, string> GetDictionaryFromJson(this TypeInfo type, string resourcePath)
        {
            var asm = type.Assembly;
            var jsonFile = string.Format("{0}.{1}", asm.GetName().Name, resourcePath);
            using (var s = asm.GetManifestResourceStream(jsonFile))
            {
                if (s == null) return null;
                using (var sr = new StreamReader(s))
                {
                    var json = sr.ReadToEnd();
                    var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return obj;
                }
            }
        }

        public static Dictionary<string, string> ParseFromJson(this Assembly asm, string resourceFile)
        {
            using (var s = asm.GetManifestResourceStream(resourceFile))
            {
                if (s == null) return null;
                using (var sr = new StreamReader(s))
                {
                    var json = sr.ReadToEnd();
                    var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return obj;
                }
            }
        }
    }
}

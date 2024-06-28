using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace TarkovDataOrganizer;
public partial class TarkovData
{

    public static bool HasValidValue(dynamic obj, string propertyName)
    {
        var jo = (JObject)obj;
        if (jo == null)
            return false;
        var token = jo[propertyName];
        if (token == null)
            return false;
        if (token.Type == JTokenType.Null)
            return false;
        return true;
    }
    
    


    
}

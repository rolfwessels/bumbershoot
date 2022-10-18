using System.Text.Json;

namespace Bumbershoot.Utilities.Helpers
{
    public static class CastHelper
    {
        public static T2 DynamicCastTo<T2>(this object val)
        {
            return JsonSerializer.Deserialize<T2>(JsonSerializer.Serialize(val))!;
        }
    }
}
using System.Globalization;
using UnityEngine;

namespace Achieve.EventTracer
{
    internal static class TraceObjectIdentity
    {
        public static string Get(UnityEngine.Object source)
        {
            if (source == null) return string.Empty;
#if UNITY_6000_4_OR_NEWER
            return source.GetEntityId().ToString();
#else
            return source.GetInstanceID().ToString(CultureInfo.InvariantCulture);
#endif
        }
    }
}

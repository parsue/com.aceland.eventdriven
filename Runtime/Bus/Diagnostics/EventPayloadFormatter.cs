#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace AceLand.EventDriven.Bus.Diagnostics
{
    /// <summary>
    /// Editor-only payload stringifier for the Event Monitor (cache inspector + raise
    /// history). When a payload type declares its own <see cref="object.ToString"/> the
    /// override is used verbatim; otherwise the default <see cref="object.ToString"/>
    /// (which only yields the type name) is replaced with a reflective dump of the
    /// public fields / properties so custom class and struct payloads show their actual
    /// content instead of just the type name. Guarded by UNITY_EDITOR so it never ships
    /// in a player build.
    /// </summary>
    internal static class EventPayloadFormatter
    {
        private const int MaxMembers = 24;
        private const int MaxDepth = 2;
        private const int MaxStringLength = 256;

        public static string Format(object value)
        {
            if (value is null) return "null";
            try { return Describe(value, 0); }
            catch { return value.GetType().Name; }
        }

        private static string Describe(object value, int depth)
        {
            if (value is null) return "null";

            var type = value.GetType();

            // Primitives / strings / enums print directly.
            if (type.IsPrimitive || value is string || value is decimal || type.IsEnum)
                return Scalar(value);

            // A type that overrides ToString() is trusted to render itself.
            if (HasCustomToString(type))
                return Truncate(value.ToString());

            // Beyond max depth, fall back to the type name to avoid runaway recursion.
            if (depth >= MaxDepth)
                return type.Name;

            // Enumerable (but not string, handled above) → element list.
            if (value is IEnumerable enumerable)
                return DescribeEnumerable(enumerable, depth);

            // Reflective member dump for plain class / struct payloads.
            return DescribeMembers(value, type, depth);
        }

        private static bool HasCustomToString(Type type)
        {
            var m = type.GetMethod("ToString", Type.EmptyTypes);
            var declaring = m?.DeclaringType;
            return declaring != null && declaring != typeof(object) && declaring != typeof(ValueType);
        }

        private static string DescribeEnumerable(IEnumerable enumerable, int depth)
        {
            var sb = new StringBuilder();
            sb.Append('[');
            var count = 0;
            foreach (var item in enumerable)
            {
                if (count > 0) sb.Append(", ");
                if (count >= MaxMembers) { sb.Append('…'); break; }
                sb.Append(Describe(item, depth + 1));
                count++;
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static string DescribeMembers(object value, Type type, int depth)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            var written = 0;

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (written >= MaxMembers) { AppendEllipsis(sb); break; }
                AppendMember(sb, field.Name, SafeGet(() => field.GetValue(value)), depth, ref written);
            }

            if (written < MaxMembers)
            {
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.GetIndexParameters().Length != 0 || !prop.CanRead) continue;
                    if (written >= MaxMembers) { AppendEllipsis(sb); break; }
                    AppendMember(sb, prop.Name, SafeGet(() => prop.GetValue(value)), depth, ref written);
                }
            }

            sb.Append('}');
            // Nothing readable → fall back to the type name so the cell is not empty "{}".
            return written == 0 ? type.Name : sb.ToString();
        }

        private static void AppendMember(StringBuilder sb, string name, object memberValue, int depth, ref int written)
        {
            if (written > 0) sb.Append(", ");
            sb.Append(name).Append('=').Append(Describe(memberValue, depth + 1));
            written++;
        }

        private static void AppendEllipsis(StringBuilder sb)
        {
            if (sb.Length > 1) sb.Append(", ");
            sb.Append('…');
        }

        private static object SafeGet(Func<object> getter)
        {
            try { return getter(); }
            catch { return "<error>"; }
        }

        private static string Scalar(object value)
        {
            if (value is string s) return '"' + Truncate(s) + '"';
            return Truncate(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
        }

        private static string Truncate(string s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
            return s.Length <= MaxStringLength ? s : s.Substring(0, MaxStringLength) + "…";
        }
    }
}
#endif

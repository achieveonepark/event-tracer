using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Achieve.EventTracer
{
    internal static class PayloadSnapshot
    {
        public static string Serialize(object value, int maxCharacters)
        {
            var writer = new SnapshotWriter(maxCharacters);
            try { return writer.Serialize(value); }
            catch (SnapshotLimitException) { return "{\"$truncated\":true}"; }
            catch (Exception exception)
            {
                // Diagnostics should survive an unreadable DTO. Never retain the original object.
                return "{\"$error\":\"" + exception.GetType().Name + "\"}";
            }
        }

        private sealed class SnapshotLimitException : Exception { }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }

        private sealed class SnapshotWriter
        {
            private readonly StringBuilder text = new StringBuilder();
            private readonly HashSet<object> path = new HashSet<object>(new ReferenceComparer());
            private readonly int budget;
            public SnapshotWriter(int characterBudget) => budget = characterBudget;

            public string Serialize(object value)
            {
                Write(value, 0);
                return text.ToString();
            }

            private void Append(string value)
            {
                if (text.Length + value.Length > budget) throw new SnapshotLimitException();
                text.Append(value);
            }

            private void Quote(string value)
            {
                Append("\"");
                foreach (var character in value ?? string.Empty)
                {
                    switch (character)
                    {
                        case '"': Append("\\\""); break;
                        case '\\': Append("\\\\"); break;
                        case '\n': Append("\\n"); break;
                        case '\r': Append("\\r"); break;
                        case '\t': Append("\\t"); break;
                        default:
                            if (character < 32 || char.IsSurrogate(character))
                                Append("\\u" + ((int)character).ToString("x4", CultureInfo.InvariantCulture));
                            else Append(character.ToString());
                            break;
                    }
                }
                Append("\"");
            }

            private void Write(object value, int depth)
            {
                if (value == null) { Append("null"); return; }
                if (value is string stringValue) { Quote(stringValue); return; }
                if (value is char || value is Enum || value is Guid) { Quote(value.ToString()); return; }
                if (value is bool boolean) { Append(boolean ? "true" : "false"); return; }
                if (value is DateTime date) { Quote(date.ToString("O", CultureInfo.InvariantCulture)); return; }
                if (value is DateTimeOffset offset) { Quote(offset.ToString("O", CultureInfo.InvariantCulture)); return; }
                var type = value.GetType();
                if (IsNumber(type))
                {
                    var formatted = Convert.ToString(value, CultureInfo.InvariantCulture);
                    if (formatted == "NaN" || formatted == "Infinity" || formatted == "-Infinity") Quote(formatted);
                    else Append(formatted);
                    return;
                }
                if (value is UnityEngine.Object unityObject)
                {
                    if (unityObject == null) { Append("null"); return; }
                    Append("{\"objectId\":");
                    Quote(TraceObjectIdentity.Get(unityObject));
                    Append(",\"name\":"); Quote(unityObject.name);
                    Append(",\"type\":"); Quote(type.Name); Append("}");
                    return;
                }
                if (depth >= 4) { Quote("<max-depth>"); return; }
                var tracked = !type.IsValueType;
                if (tracked && !path.Add(value)) { Quote("<cycle>"); return; }
                try
                {
                    if (value is IDictionary dictionary) WriteDictionary(dictionary, depth);
                    else if (value is IEnumerable enumerable) WriteEnumerable(enumerable, depth);
                    else WriteObject(value, type, depth);
                }
                finally { if (tracked) path.Remove(value); }
            }

            private void WriteDictionary(IDictionary values, int depth)
            {
                Append("{");
                var count = 0;
                foreach (DictionaryEntry entry in values)
                {
                    if (count > 0) Append(",");
                    if (count++ >= 32) { Quote("$truncated"); Append(":true"); break; }
                    Quote(Convert.ToString(entry.Key, CultureInfo.InvariantCulture)); Append(":");
                    Write(entry.Value, depth + 1);
                }
                Append("}");
            }

            private void WriteEnumerable(IEnumerable values, int depth)
            {
                Append("[");
                var count = 0;
                foreach (var item in values)
                {
                    if (count > 0) Append(",");
                    if (count++ >= 32) { Quote("<truncated>"); break; }
                    Write(item, depth + 1);
                }
                Append("]");
            }

            private void WriteObject(object value, Type type, int depth)
            {
                var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                    .Where(member => member is FieldInfo || member is PropertyInfo property &&
                                     property.CanRead && property.GetMethod != null && property.GetMethod.IsPublic &&
                                     property.GetIndexParameters().Length == 0)
                    .GroupBy(member => member.Name).Select(group => group.First())
                    .OrderBy(member => member.Name, StringComparer.Ordinal).Take(33).ToArray();
                Append("{");
                for (var i = 0; i < members.Length; i++)
                {
                    if (i > 0) Append(",");
                    if (i == 32) { Quote("$truncated"); Append(":true"); break; }
                    var member = members[i];
                    Quote(member.Name); Append(":");
                    object memberValue;
                    try
                    {
                        memberValue = member is FieldInfo field ? field.GetValue(value) : ((PropertyInfo)member).GetValue(value);
                    }
                    catch (Exception exception) { memberValue = "<unreadable: " + exception.GetType().Name + ">"; }
                    Write(memberValue, depth + 1);
                }
                Append("}");
            }

            private static bool IsNumber(Type type)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte: case TypeCode.SByte: case TypeCode.Int16: case TypeCode.UInt16:
                    case TypeCode.Int32: case TypeCode.UInt32: case TypeCode.Int64: case TypeCode.UInt64:
                    case TypeCode.Single: case TypeCode.Double: case TypeCode.Decimal: return true;
                    default: return false;
                }
            }
        }
    }
}

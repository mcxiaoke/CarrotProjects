using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;
using Newtonsoft.Json;

namespace Carrot.Common.Extensions {

    static class ObjectExtCache<T> where T : new() {
        private static readonly Func<T, T> cloner;
        static ObjectExtCache() {
            ParameterExpression param = Expression.Parameter(typeof(T),
            "in");

            var bindings = from prop in typeof(T).GetProperties()
                           where prop.CanRead && prop.CanWrite
                           select (MemberBinding)Expression.Bind(prop,
                           Expression.Property(param, prop));

            cloner = Expression.Lambda<Func<T, T>>(
            Expression.MemberInit(
            Expression.New(typeof(T)), bindings), param).Compile();
        }
        public static T Clone(T obj) {
            return cloner(obj);
        }
    }

    public static class ObjectExtensions {
        public static T CloneMe<T>(this T obj) where T : new() {
            return ObjectExtCache<T>.Clone(obj);
        }

        /// <summary>
        /// Using json serialize and deserialize to deep copy object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static T CloneJson<T>(this T value, JsonSerializerSettings? settings = null) where T : class {
            Guard.IsNotNull(value, "source object is null");
            if (value.GetType() is Type objectType) {
                string cereal = JsonConvert.SerializeObject(value, settings);
                if (JsonConvert.DeserializeObject(cereal, objectType, settings) is object reusult) {
                    return (T)reusult;
                } else {
                    throw new ArgumentException("object deserialize failed");
                }

            } else {
                throw new ArgumentNullException("source object type is null");
            }
        }

        public static void CopyPropertiesFrom(this object self, object parent) {
            var fromProperties = parent.GetType().GetProperties();
            var toProperties = self.GetType().GetProperties();

            foreach (var fromProperty in fromProperties) {
                foreach (var toProperty in toProperties) {
                    if (fromProperty.Name == toProperty.Name && fromProperty.PropertyType == toProperty.PropertyType) {
                        toProperty.SetValue(self, fromProperty.GetValue(parent));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Copies all the matching properties and fields from 'source' to 'destination'
        /// </summary>
        /// <param name="source">The source object to copy from</param>
        /// <param name="destination">The destination object to copy to</param>
        public static void CopyPropsTo<T1, T2>(this T1 source, ref T2 destination) {
            if (source?.GetType() is Type st && destination?.GetType() is Type dt) {
                var sourceMembers = GetMembers(st);
                var destinationMembers = GetMembers(dt);

                // Copy data from source to destination
                foreach (var sourceMember in sourceMembers) {
                    if (!CanRead(sourceMember)) {
                        continue;
                    }
                    var destinationMember = destinationMembers.Find(x => string.Equals(x.Name, sourceMember.Name, StringComparison.OrdinalIgnoreCase));
                    if (destinationMember == null || !CanWrite(destinationMember)) {
                        continue;
                    }
                    SetObjectValue(ref destination, destinationMember, GetMemberValue(source, sourceMember));
                }
            }

        }

        private static void SetObjectValue<T>(ref T obj, System.Reflection.MemberInfo member, object? value) {
            if (obj != null) {
                // Boxing method used for modifying structures
                var boxed = obj?.GetType().IsValueType == true ? (object)obj : obj;
                SetMemberValue(ref boxed, member, value);
                obj = (T)boxed!;
            }
        }

        private static void SetMemberValue<T>(ref T obj, System.Reflection.MemberInfo member, object? value) {
            if (IsProperty(member)) {
                var prop = (System.Reflection.PropertyInfo)member;
                if (prop.SetMethod != null) {
                    prop.SetValue(obj, value);
                }
            } else if (IsField(member)) {
                var field = (System.Reflection.FieldInfo)member;
                field.SetValue(obj, value);
            }
        }

        private static object? GetMemberValue(object obj, System.Reflection.MemberInfo member) {
            if (IsProperty(member)) {
                var prop = (System.Reflection.PropertyInfo)member;
                return prop.GetValue(obj, prop.GetIndexParameters().Length == 1 ? new object[] { null! } : null);
            } else if (IsField(member)) {
                var field = (System.Reflection.FieldInfo)member;
                return field.GetValue(obj);
            }
            return null;
        }

        private static bool CanWrite(System.Reflection.MemberInfo member) {
            return IsProperty(member) ? ((System.Reflection.PropertyInfo)member).CanWrite : IsField(member);
        }

        private static bool CanRead(System.Reflection.MemberInfo member) {
            return IsProperty(member) ? ((System.Reflection.PropertyInfo)member).CanRead : IsField(member);
        }

        private static bool IsProperty(System.Reflection.MemberInfo member) {
            return IsType(member.GetType(), typeof(System.Reflection.PropertyInfo));
        }

        private static bool IsField(System.Reflection.MemberInfo member) {
            return IsType(member.GetType(), typeof(System.Reflection.FieldInfo));
        }

        private static bool IsType(System.Type type, System.Type targetType) {
            return type.Equals(targetType) || type.IsSubclassOf(targetType);
        }

        private static List<System.Reflection.MemberInfo> GetMembers(System.Type type) {
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic;
            var members = new List<System.Reflection.MemberInfo>();
            members.AddRange(type.GetProperties(flags));
            members.AddRange(type.GetFields(flags));
            return members;
        }
    }
}

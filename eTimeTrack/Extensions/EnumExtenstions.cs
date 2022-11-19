using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace eTimeTrack.Extensions
{
    public static class EnumExtensions
    {
        //http://stackoverflow.com/a/9276348
        private static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            Type type = enumVal.GetType();
            MemberInfo[] memInfo = type.GetMember(enumVal.ToString());
            object[] attributes = memInfo.Length > 0 ? memInfo[0].GetCustomAttributes(typeof(T), false) : null;
            return (attributes != null && attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static string GetDescription(this Enum enumValue)
        {
            DescriptionAttribute desc = GetAttributeOfType<DescriptionAttribute>(enumValue);
            return desc == null ? enumValue.ToString() : desc.Description;
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            DisplayAttribute desc = GetAttributeOfType<DisplayAttribute>(enumValue);
            return desc == null ? enumValue.ToString() : desc.Name;
        }
    }
}
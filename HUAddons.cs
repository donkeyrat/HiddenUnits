using System;
using System.Reflection;

namespace HiddenUnits
{
    public static class HUAddons
    {
        public static object GetField(Type type, object instance, string fieldName)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = type.GetField(fieldName, bindingAttr);
            return field.GetValue(instance);
        }

        public static void SetField<T>(object originalObject, string fieldName, T newValue)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = originalObject.GetType().GetField(fieldName, bindingAttr);
            field.SetValue(originalObject, newValue);
        }
    }
}
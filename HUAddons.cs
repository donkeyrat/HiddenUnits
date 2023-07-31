using System;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace HiddenUnits
{
    public static class HUAddons
    {
        public static object GetField(Type type, object instance, string fieldName)
        {
            var bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var field = type.GetField(fieldName, bindingAttr);
            return field.GetValue(instance);
        }

        public static void SetField<T>(object originalObject, string fieldName, T newValue)
        {
            var bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var field = originalObject.GetType().GetField(fieldName, bindingAttr);
            field.SetValue(originalObject, newValue);
        }
        
        public static string DeepString(this GameObject self)
        {
            var final = "\nGameObject '" + self.name + "':\n{\n\tComponents:\n\t{\n";
            final += String.Concat(from Component component in self.GetComponents<Component>() select ("\t\t" + component.GetType().Name + "\n"));
            final += "\t}\n";
            if (self.transform.childCount > 0)
            {
                final += "\tChildren:\n\t{\n";
                final += String.Concat(from Transform child in self.transform select (child.gameObject.DeepString().Replace("\n", "\n\t\t")));
                final += "\n\t}\n";
            }
            final += "}\n";
            return final;
        }

        public static T DeepCopyOf<T>(this T self, T from) where T : class
        {
            foreach (var fieldInfo in typeof(T).GetFields((BindingFlags)(-1)))
            {
                try
                {
                    fieldInfo.SetValue(self, fieldInfo.GetValue(from));
                }
                catch
                {
                }
            }
            foreach (var propertyInfo in typeof(T).GetProperties((BindingFlags)(-1)))
            {
                if (propertyInfo.CanWrite && propertyInfo.CanRead)
                {
                    try
                    {
                        propertyInfo.SetValue(self, propertyInfo.GetValue(from));
                    }
                    catch
                    {
                    }
                }
            }
            return self;
        }
    }
}
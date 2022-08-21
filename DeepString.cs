using System;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace HiddenUnits
{
    public static class PrintGameObject
    {
        public static string DeepString(this GameObject self)
        {
            string final = "\nGameObject '" + self.name + "':\n{\n\tComponents:\n\t{\n";
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
			foreach (FieldInfo fieldInfo in typeof(T).GetFields((BindingFlags)(-1)))
			{
				try
				{
					fieldInfo.SetValue(self, fieldInfo.GetValue(from));
				}
				catch
				{
				}
			}
			foreach (PropertyInfo propertyInfo in typeof(T).GetProperties((BindingFlags)(-1)))
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
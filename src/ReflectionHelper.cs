using System.Reflection;
using UnityEngine;
public static class ReflectionHelper
{
    public static T FindVariableByType<T>(object obj)
    {
        if (obj == null) return default;

        var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (typeof(T).IsAssignableFrom(field.FieldType))
            {
                return (T)field.GetValue(obj);
            }
        }

        return default;
    }
}
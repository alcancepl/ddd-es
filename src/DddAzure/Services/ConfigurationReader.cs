using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ddd.Services
{
    public class ConfigurationReader<T> where T : class, new()
    {
        public static T GetConfig()
        {   
            return Refresh(new T());            
        }

        static Dictionary<string, Action<T, string>> setters;

        static ConfigurationReader()
        {
            setters = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => p.CanWrite)
                .ToDictionary(k => k.Name, v => new Action<T, string>(v.SetValue));
        }

        static T Refresh(T instance)
        {
            foreach (var setter in setters)
            {
                var propertyName = setter.Key;
                var propertySetter = setter.Value;
                string value = null;
                try
                {
                    value = Microsoft.Azure.CloudConfigurationManager.GetSetting(propertyName);
                    propertySetter(instance, value);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Configuration reader cannot set configuration property {propertyName} to value '{value ?? "<NULL>"}'.", ex);
                }
            }
            return instance;
        }
    }
}

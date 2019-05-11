using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConsoleVariable
{
    public class CVariable
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Value => field.GetValue(null).ToString();
        private FieldInfo field;

        public CVariable(string name, FieldInfo field, string description)
        {
            Name = name;
            this.field = field;
            Description = Description;
        }

        public void SetValue(int value)
        {
            field.SetValue(null, value);
        }

        static CVariable()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var _class in assembly.GetTypes())
                {
                    if (!_class.IsClass)
                    {
                        continue;
                    }
                    foreach (var field in _class.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (!field.IsDefined(typeof(CVarAttribute), false))
                        {
                            continue;
                        }
                        if (field.FieldType != typeof(int))
                        {
                            continue;
                        }
                        var attr = field.GetCustomAttribute<CVarAttribute>(false);
                        var name = attr.Name != null ? attr.Name : _class.Name.ToLower() + "." + field.Name.ToLower();
                        var cvar = new CVariable(name, field, attr.Description);
                        Register(cvar);
                    }
                }
            }
        }

        private static Dictionary<string, CVariable> cvarMap = new Dictionary<string, CVariable>();

        private static void Register(CVariable cvar)
        {
            if (cvarMap.ContainsKey(cvar.Name))
            {
                // Duplicate CVars
                return;
            }
            cvarMap[cvar.Name] = cvar;
        }

        public static bool ContainsCVar(string name)
        {
            return cvarMap.ContainsKey(name);
        }

        public static string GetCVarValue(string name)
        {
            if (cvarMap.ContainsKey(name))
            {
                return cvarMap[name].Value;
            }
            return null;
        }

        public static void SetCVarValue(string name, int value)
        {
            if (cvarMap.ContainsKey(name))
            {
                cvarMap[name].SetValue(value);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class CVarAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public CVarAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}

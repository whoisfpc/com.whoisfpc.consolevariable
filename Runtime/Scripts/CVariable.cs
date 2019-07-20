using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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

        public bool SetValue(string valueString)
        {
            if (field.FieldType == typeof(int))
            {
                if (int.TryParse(valueString, out int value))
                {
                    field.SetValue(null, value);
                }
            }
            else if (field.FieldType == typeof(float))
            {
                if (float.TryParse(valueString, out float value))
                {
                    field.SetValue(null, value);
                }
            }
            return false;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        static void Init()
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
                        if (field.FieldType == typeof(int) || field.FieldType == typeof(float))
                        {
                            var attr = field.GetCustomAttribute<CVarAttribute>(false);
                            var name = attr.Name != null ? attr.Name : _class.Name.ToLower() + "." + field.Name.ToLower();
                            var cvar = new CVariable(name, field, attr.Description);
                            Register(cvar);
                        }
                    }
                }
            }
            cvarList.Sort((a, b) => {
                return a.Name.CompareTo(b.Name);
            });
        }

        private static Dictionary<string, CVariable> cvarMap = new Dictionary<string, CVariable>();
        private static List<CVariable> cvarList = new List<CVariable>();
        public static List<CVariable> CVarList { get => cvarList; }

        private static void Register(CVariable cvar)
        {
            if (cvarMap.ContainsKey(cvar.Name))
            {
                // Duplicate CVars
                return;
            }
            cvarMap[cvar.Name] = cvar;
            cvarList.Add(cvar);
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

        public static bool SetCVarValue(string name, string valueString)
        {
            if (cvarMap.ContainsKey(name))
            {
                return cvarMap[name].SetValue(valueString);
            }
            return false;
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

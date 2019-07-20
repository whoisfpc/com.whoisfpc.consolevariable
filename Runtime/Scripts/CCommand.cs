using System;
using System.Reflection;

namespace ConsoleVariable
{
    public class CCommand
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        private MethodInfo method;
        private Type[] paramTypes;

        public CCommand(string name, MethodInfo method, string description)
        {
            Name = name;
            this.method = method;
            Description = Description;
            var paramInfos = method.GetParameters();
            paramTypes = new Type[paramInfos.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                paramTypes[i] = paramInfos[i].ParameterType;
            }
        }

        private bool ConvertToParams(string[] paramStrings, object[] parameters)
        {
            for (int i = 0; i < paramStrings.Length; i++)
            {
                if (paramTypes[i] == typeof(float))
                {
                    if (float.TryParse(paramStrings[i], out float v))
                    {
                        parameters[i] = v;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (paramTypes[i] == typeof(int))
                {
                    if (int.TryParse(paramStrings[i], out int v))
                    {
                        parameters[i] = v;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (paramTypes[i] == typeof(string))
                {
                    parameters[i] = paramStrings[i];
                }
                else if (paramTypes[i] == typeof(bool))
                {
                    if (string.Compare(paramStrings[i], "TRUE", ignoreCase: true) == 0)
                    {
                        parameters[i] = true;
                    }
                    else if (string.Compare(paramStrings[i], "FALSE", ignoreCase: true) == 0)
                    {
                        parameters[i] = false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public bool RunCommand(string[] paramStrings)
        {
            if (paramStrings.Length != paramTypes.Length)
            {
                return false;
            }
            var parameters = new object[paramTypes.Length];
            if (ConvertToParams(paramStrings, parameters))
            {
                method.Invoke(null, parameters);
                return true;
            }
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CCmdAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public CCmdAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}

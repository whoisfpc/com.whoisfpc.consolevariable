using System;
using System.Reflection;

namespace ConsoleVariable
{
    using Result = Console.Result;
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
            Description = description;
            var paramInfos = method.GetParameters();
            paramTypes = new Type[paramInfos.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                paramTypes[i] = paramInfos[i].ParameterType;
            }
        }

        private Result ConvertToParams(string[] paramStrings, object[] parameters)
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
                        return new Result(false, $"the {i} arg type not match, need \"float\"");
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
                        return new Result(false, $"the {i} arg type not match, need \"int\"");
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
                        return new Result(false, $"the {i} arg type not match, need \"bool\"");
                    }
                }
                else
                {
                    return new Result(false, $"the {i} arg type not support, \"{paramTypes[i].ToString()}\"");
                }
            }
            return new Result(true);
        }

        public Result RunCommand(string[] paramStrings)
        {
            if (paramStrings.Length != paramTypes.Length)
            {
                return new Result(false, $"args length not match, require {paramTypes.Length} args, found {paramStrings.Length} args.");
            }
            var parameters = new object[paramTypes.Length];
            var convertResult = ConvertToParams(paramStrings, parameters);
            if (convertResult.success)
            {
                var o = method.Invoke(null, parameters);
                if (o is Result)
                {
                    return (Result)o;
                }
                return new Result(true);;
            }
            return convertResult;
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

using System;
using System.Reflection;

namespace ConsoleVariable
{
    using Result = Console.Result;
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
            Description = description;
        }

        public Result SetValue(string valueString)
        {
            if (field.FieldType == typeof(int))
            {
                if (int.TryParse(valueString, out int value))
                {
                    field.SetValue(null, value);
                    return new Result(true, $"set {Name} to {value}");
                }
                else
                {
                    return new Result(false, $"could not convert \"{valueString}\" to int");
                }
            }
            else if (field.FieldType == typeof(float))
            {
                if (float.TryParse(valueString, out float value))
                {
                    field.SetValue(null, value);
                    return new Result(true, $"set {Name} to {value}");
                }
                else
                {
                    return new Result(false, $"could not convert \"{valueString}\" to float");
                }
            }
            return new Result(false, $"not support cvar type \"{field.FieldType.ToString()}\"");
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

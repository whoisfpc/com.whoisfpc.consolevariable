using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;

namespace ConsoleVariable
{
    public class Console
    {
        public struct Result
        {
            public bool success;
            public string message;
            public Result(bool success, string message)
            {
                this.success = success;
                this.message = message;
            }
            public Result(bool success)
            {
                this.success = success;
                this.message = string.Empty;
            }
            public string ColoredString()
            {
                if (string.IsNullOrEmpty(message))
                {
                    return string.Empty;
                }
                if (success)
                {
                    return $"<color=\"#1B813E\">{message}</color>";
                }
                else
                {
                    return $"<color=\"#c00\">{message}</color>";
                }
            }
        }

        private static Console instance;
        public static Console Get()
        {
            if (instance == null)
            {
                instance = new Console();
            }
            return instance;
        }

        [CCmd("quit", "quit application")]
        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        [CCmd("help", "print cvar or ccmd help description.")]
        static Result Help(string name)
        {
            if (ccwrapperMap.ContainsKey(name))
            {
                var ccwrap = ccwrapperMap[name];
                if (ccwrap.type == CCWrapper.WrappedType.CCmd)
                {
                    return new Result(true, ccwrap.cmd.Description);
                }
                else
                {
                    return new Result(true, ccwrap.cvar.Description);
                }
            }
            return new Result(false, $"cound not found cvar or ccmd \"{name}\"");
        }

        public string ProcessCommand(string command)
        {
            var tokens = Regex.Split(command, @"[\t\s]+").Where(s => !string.IsNullOrEmpty(s)).ToArray();
            if (tokens.Length == 0)
            {
                return "command only contains white space.";
            }
            string coloredCmd = TokensToColoredCmd(tokens);
            if (ContainsCVar(tokens[0]))
            {
                var result = ProcessCVar(tokens);
                if (!string.IsNullOrWhiteSpace(result.message))
                {
                    return $"{coloredCmd}\n{result.ColoredString()}";
                }
                return coloredCmd;
            }
            else if (ContainsCCmd(tokens[0]))
            {
                var result = ProcessCCmd(tokens);
                if (!string.IsNullOrWhiteSpace(result.message))
                {
                    return $"{coloredCmd}\n{result.ColoredString()}";
                }
                return coloredCmd;
            }
            Result error = new Result(false, "cound not find ccmd or cvar \"{tokens[0]}\"");
            return $"{coloredCmd}\n{error.ToString()}";
        }

        private static string TokensToColoredCmd(string[] tokens)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"> <color=\"#51A8DD\">{tokens[0]}</color>");
            for (int i = 1; i < tokens.Length; i++)
            {
                sb.Append(" ");
                sb.Append(tokens[i]);
            }
            return sb.ToString();
        }

        private Result ProcessCVar(string[] tokens)
        {
            if (tokens.Length == 2)
            {
                return SetCVarValue(tokens[0], tokens[1]);
            }
            else if (tokens.Length == 1)
            {
                if (ContainsCVar(tokens[0]))
                {
                    var cvarValue = GetCVarValue(tokens[0]);
                    return new Result(true, $"{tokens[0]} = {cvarValue}");
                }
                else
                {
                    return new Result(false, $"cound not find cvar \"{tokens[0]}\"");
                }
            }
            else
            {
                return new Result(false, "too more args, use [cvar] [value] to set new value, or use [cvar] to print value");
            }
        }

        private Result ProcessCCmd(string[] tokens)
        {
            string[] args = new string[tokens.Length - 1];
            Array.Copy(tokens, 1, args, 0, args.Length);
            return RunCCmd(tokens[0], args);
        }

        // TODO: need improvement, use trie tree to accelerate find speed
        public void Autocomplete(string partialCommand, List<string> candidates)
        {
            candidates.Clear();
            for (int i = 0; i < ccwrapperList.Count; i++)
            {
                if (ccwrapperList[i].Name.StartsWith(partialCommand))
                {
                    candidates.Add(ccwrapperList[i].Name);
                }
            }
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
                    foreach (var method in _class.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        if (!method.IsDefined(typeof(CCmdAttribute), false))
                        {
                            continue;
                        }
                        var attr = method.GetCustomAttribute<CCmdAttribute>(false);
                        var cmd = new CCommand(attr.Name, method, attr.Description);
                        Register(cmd);
                    }
                }
            }
            ccwrapperList.Sort((a, b) => {
                return a.Name.CompareTo(b.Name);
            });
        }

        private static Dictionary<string, CCWrapper> ccwrapperMap = new Dictionary<string, CCWrapper>();
        private static List<CCWrapper> ccwrapperList = new List<CCWrapper>();

        private static void Register(CVariable cvar)
        {
            if (ccwrapperMap.ContainsKey(cvar.Name))
            {
                // duplicate name
                Debug.LogWarning($"duplicate cvar or ccmd name: {cvar.Name}");
                return;
            }
            var wrapper = new CCWrapper()
            {
                type = CCWrapper.WrappedType.CVar,
                cvar = cvar,
            };
            ccwrapperMap[wrapper.Name] = wrapper;
            ccwrapperList.Add(wrapper);
        }

        private static void Register(CCommand cmd)
        {
            if (ccwrapperMap.ContainsKey(cmd.Name))
            {
                // duplicate name
                Debug.LogWarning($"duplicate cvar or ccmd name: {cmd.Name}");
                return;
            }
            var wrapper = new CCWrapper()
            {
                type = CCWrapper.WrappedType.CCmd,
                cmd = cmd,
            };
            ccwrapperMap[wrapper.Name] = wrapper;
            ccwrapperList.Add(wrapper);
        }

        public static bool ContainsCVar(string name)
        {
            return ccwrapperMap.ContainsKey(name) && ccwrapperMap[name].type == CCWrapper.WrappedType.CVar;
        }

        public static string GetCVarValue(string name)
        {
            if (ContainsCVar(name))
            {
                return ccwrapperMap[name].cvar.Value;
            }
            return null;
        }

        public static Result SetCVarValue(string name, string valueString)
        {
            if (ContainsCVar(name))
            {
                return ccwrapperMap[name].cvar.SetValue(valueString);
            }
            return new Result(false, $"cound not find cvar \"{name}\"");
        }

        public static bool ContainsCCmd(string name)
        {
            return ccwrapperMap.ContainsKey(name) && ccwrapperMap[name].type == CCWrapper.WrappedType.CCmd;
        }

        public static Result RunCCmd(string name, string[] paramStrings)
        {
            if (ContainsCCmd(name))
            {
                return ccwrapperMap[name].cmd.RunCommand(paramStrings);
            }
            return new Result(false, $"cound not find ccmd \"{name}\"");
        }
    }
}

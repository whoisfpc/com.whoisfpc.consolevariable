using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConsoleVariable
{
    public class Console
    {
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

        public string ProcessCommand(string command)
        {
            var tokens = command.Split(' ');
            if (tokens.Length == 0)
            {
                return string.Format("> {0}", command);
            }
            if (ContainsCVar(tokens[0]))
            {
                return ProcessCVar(tokens);
            }
            else if (ContainsCCmd(tokens[0]))
            {
                return ProcessCCmd(tokens);
            }
            return string.Format("> {0}", command);
        }

        private string ProcessCVar(string[] tokens)
        {
            if (tokens.Length == 2 && SetCVarValue(tokens[0], tokens[1]))
            {
                var output = string.Format("> {tokens[0]} {tokens[1]}");
                return output;
            }
            else if (tokens.Length == 1 && ContainsCVar(tokens[0]))
            {
                var cvarValue = GetCVarValue(tokens[0]);
                var output = string.Format("> {0} = {1}", tokens[0], cvarValue);
                return output;
            }
            return "> params count error!";
        }

        private string ProcessCCmd(string[] tokens)
        {
            string[] args = new string[tokens.Length - 1];
            Array.Copy(tokens, 1, args, 0, args.Length);
            if (RunCCmd(tokens[0], args))
            {
                return $"> run {tokens[0]} success!";
            }
            return $"> run {tokens[0]} fail!";
        }

        // TODO: need improvement, use trie tree to accelerate find speed
        public void Autocomplete(string partialCommand, List<string> candidates)
        {
            candidates.Clear();
            for (int i = 0; i < ccwarpperList.Count; i++)
            {
                if (ccwarpperList[i].Name.StartsWith(partialCommand, StringComparison.InvariantCultureIgnoreCase))
                {
                    candidates.Add(ccwarpperList[i].Name);
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
            ccwarpperList.Sort((a, b) => {
                return a.Name.CompareTo(b.Name);
            });
        }

        private static Dictionary<string, CCWrapper> ccwarpperMap = new Dictionary<string, CCWrapper>();
        private static List<CCWrapper> ccwarpperList = new List<CCWrapper>();

        private static void Register(CVariable cvar)
        {
            if (ccwarpperMap.ContainsKey(cvar.Name))
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
            ccwarpperMap[wrapper.Name] = wrapper;
            ccwarpperList.Add(wrapper);
        }

        private static void Register(CCommand cmd)
        {
            if (ccwarpperMap.ContainsKey(cmd.Name))
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
            ccwarpperMap[wrapper.Name] = wrapper;
            ccwarpperList.Add(wrapper);
        }

        public static bool ContainsCVar(string name)
        {
            return ccwarpperMap.ContainsKey(name) && ccwarpperMap[name].type == CCWrapper.WrappedType.CVar;
        }

        public static string GetCVarValue(string name)
        {
            if (ContainsCVar(name))
            {
                return ccwarpperMap[name].cvar.Value;
            }
            return null;
        }

        public static bool SetCVarValue(string name, string valueString)
        {
            if (ContainsCVar(name))
            {
                return ccwarpperMap[name].cvar.SetValue(valueString);
            }
            return false;
        }

        public static bool ContainsCCmd(string name)
        {
            return ccwarpperMap.ContainsKey(name) && ccwarpperMap[name].type == CCWrapper.WrappedType.CCmd;
        }

        public static bool RunCCmd(string name, string[] paramStrings)
        {
            if (ContainsCCmd(name))
            {
                return ccwarpperMap[name].cmd.RunCommand(paramStrings);
            }
            return false;
        }
    }
}

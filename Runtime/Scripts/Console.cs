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

        public string ProcessCommand(string command)
        {
            var tokens = command.Split(' ');
            if (tokens.Length == 2 && CVariable.SetCVarValue(tokens[0], tokens[1]))
            {
                var output = string.Format("> {0}", command);
                return output;
            }
            else if (tokens.Length == 1 && CVariable.ContainsCVar(tokens[0]))
            {
                var cvarValue = CVariable.GetCVarValue(tokens[0]);
                var output = string.Format("> {0} = {1}", tokens[0], cvarValue);
                return output;
            }
            return string.Format("> {0}", command);
        }

        // TODO: need improvement, use trie tree to accelerate find speed
        public void Autocomplete(string partialCommand, List<string> candidates)
        {
            candidates.Clear();
            var cvarList = CVariable.CVarList;
            for (int i = 0; i < cvarList.Count; i++)
            {
                if (cvarList[i].Name.StartsWith(partialCommand, StringComparison.InvariantCultureIgnoreCase))
                {
                    candidates.Add(cvarList[i].Name);
                }
            }
        }
    }
}

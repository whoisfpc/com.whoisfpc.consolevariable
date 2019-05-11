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
            if (tokens.Length == 2 && int.TryParse(tokens[1], out var value))
            {
                CVariable.SetCVarValue(tokens[0], value);
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
    }
}

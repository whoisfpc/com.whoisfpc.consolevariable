using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

namespace ConsoleVariable
{
    public class ConsoleGUI : MonoBehaviour
    {
        public ConsoleInputField inputField;
        [Tooltip("KeyCode used to active or deactive console gui")]
        public KeyCode consoleKeyCode = KeyCode.BackQuote;
        public Transform rootPanel;
        public Text bufferArea;
        private List<string> stringLines = new List<string>();
        private static int maxTextBuffer = 100;
        private List<string> candicateCommands = new List<string>();

        void Awake()
        {
            inputField.onEndEdit.AddListener(OnSubmit);
        }

        void Start()
        {
            inputField.skipKey = consoleKeyCode;
        }

        void Update()
        {
            if (Input.GetKeyDown(consoleKeyCode))
            {
                SetOpen(!IsOpen());
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Console.Get().Autocomplete(inputField.text, candicateCommands);
                if (candicateCommands.Count == 1)
                {
                    inputField.text = candicateCommands[0];
                    inputField.caretPosition = candicateCommands[0].Length;
                }
                else if (candicateCommands.Count > 1)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < candicateCommands.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(' ');
                        }
                        sb.Append(candicateCommands[i]);
                    }
                    UpdateOutput(sb.ToString());
                }
            }
        }

        private void UpdateOutput(string s)
        {
            stringLines.Add(s);
            var count = Mathf.Min(maxTextBuffer, stringLines.Count);
            var start = stringLines.Count - count;
            bufferArea.text = string.Join("\n", stringLines.GetRange(start, count).ToArray());
        }

        private void OnSubmit(string value)
        {
            if (!Input.GetKey(KeyCode.Return) && !Input.GetKey(KeyCode.KeypadEnter))
                return;

            inputField.text = "";
            inputField.ActivateInputField();
            var output = Console.Get().ProcessCommand(value);
            UpdateOutput(output);
        }

        public bool IsOpen()
        {
            return rootPanel.gameObject.activeSelf;
        }

        public void SetOpen(bool open)
        {
            rootPanel.gameObject.SetActive(open);
            if (open)
            {
                inputField.ActivateInputField();
            }
        }
    }
}

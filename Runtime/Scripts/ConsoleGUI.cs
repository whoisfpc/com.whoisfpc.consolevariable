using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

        void Awake()
        {
            inputField.onEndEdit.AddListener(OnSubmit);
            inputField.skipKey = consoleKeyCode;
        }

        void Update()
        {
            if (Input.GetKeyDown(consoleKeyCode))
            {
                SetOpen(!IsOpen());
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

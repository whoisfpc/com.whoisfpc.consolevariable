using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ConsoleVariable
{
    public class ConsoleInputField : InputField
    {
        public KeyCode skipKey = KeyCode.BackQuote;
        private Event consoleProcessingEvent = new Event();

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (!isFocused)
                return;

            bool consumedEvent = false;
            while (Event.PopEvent(consoleProcessingEvent))
            {
                if (consoleProcessingEvent.rawType == EventType.KeyDown)
                {
                    consumedEvent = true;
                    EditState shouldContinue = EditState.Finish;
                    bool notHistoryCmdKeys = consoleProcessingEvent.keyCode != KeyCode.UpArrow
                        && consoleProcessingEvent.keyCode != KeyCode.DownArrow;
                    if (consoleProcessingEvent.keyCode != skipKey)
                    {
                        if (notHistoryCmdKeys)
                        {
                            shouldContinue = KeyPressed(consoleProcessingEvent);
                        }
                        else
                        {
                            shouldContinue = EditState.Continue;
                        }
                    }
                    if (shouldContinue == EditState.Finish)
                    {
                        DeactivateInputField();
                        break;
                    }
                }

                switch (consoleProcessingEvent.type)
                {
                    case EventType.ValidateCommand:
                    case EventType.ExecuteCommand:
                        switch (consoleProcessingEvent.commandName)
                        {
                            case "SelectAll":
                                SelectAll();
                                consumedEvent = true;
                                break;
                        }
                        break;
                }
            }

            if (consumedEvent)
                UpdateLabel();

            eventData.Use();
        }
    }
}

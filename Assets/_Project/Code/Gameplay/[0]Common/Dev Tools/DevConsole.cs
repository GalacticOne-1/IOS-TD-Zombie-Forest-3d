
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;


namespace Galactic1.Dev
{
    public class DevConsole : MonoBehaviour
    {
        public TMP_InputField inputField;
        public TMP_Text outputText; // для вывода результатов
        public KeyCode toggleKey = KeyCode.BackQuote; // `
    
        private Dictionary<string, IDevCommand> commands = new Dictionary<string, IDevCommand>();
        private bool visible;

        void Start()
        {
            RegisterDefaultCommands();
            Hide();
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (visible) Hide();
                else Show();
            }
        }

        void Show()
        {
            inputField.gameObject.SetActive(true);
            inputField.ActivateInputField();
            visible = true;
            Time.timeScale = 0f; // пауза игры, можно убрать
        }

        void Hide()
        {
            inputField.gameObject.SetActive(false);
            visible = false;
            Time.timeScale = 1f;
        }

        public void OnInputEndEdit(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            ExecuteCommand(text);
            inputField.text = "";
            inputField.ActivateInputField();
        }

        void ExecuteCommand(string input)
        {
            var parts = input.Split(' ');
            var cmdName = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            if (commands.TryGetValue(cmdName, out var command))
            {
                command.Execute(args);
            }
            else
            {
                Log($"Unknown command: {cmdName}");
            }
        }

        public void Log(string message)
        {
            if (outputText != null)
                outputText.text += "\n" + message;
            Debug.Log("[DevConsole] " + message);
        }

        public void RegisterCommand(IDevCommand command)
        {
            commands[command.Name.ToLower()] = command;
        }

        void RegisterDefaultCommands()
        {
            RegisterCommand(new HelpCommand(this, commands));
            RegisterCommand(new TimescaleCommand(this));
        }
    }

}
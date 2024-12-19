using System.Collections.Generic;

namespace BUTTLYSS
{
    public static class ButtlyssConsole
    {
        /// <summary>
        /// Attempts to determine buttlyss-related console commands
        /// </summary>
        /// <param name="message">Original message to parse for commands</param>
        /// <param name="chat">ChatBehaviour object that has the chat UI</param>
        /// <returns>Whether the message contained a command that is being handled</returns>
        public static bool TryHandleChatCommands(string message, ChatBehaviour chat) {
            // Standardize input for commands
            message = message.ToLowerInvariant();

            // Only care about buttlyss commands
            if(message.Length < 9)
                return false;
            if(!message.Contains("/buttlyss"))
                return false;

            // Base command
            if(message.Length == 9) {
                AppendChat(chat, "Available commands:", includeTitle: true);
                foreach(KeyValuePair<string,string> command in Properties.CommandInfo) {
                    AppendChat(chat, $"{command.Key} | {command.Value}");
                }

                AppendChat(chat, "\nUse '/buttlyss <Command>' to use a command");

                return true;
            }

            switch(message) {
                case "/buttlyss stop": {
                    AppendChat(chat, "Stopping vibrations", includeTitle: true);
                    Properties.EmergencyStop = true;
                    return true;
                }
                case "/buttlyss start": {
                    AppendChat(chat, "Starting vibrations", includeTitle: true);
                    Properties.EmergencyStop = false;
                    return true;
                }
                case "/buttlyss reload": {
                    AppendChat(chat, "Loading preferences from settings file", includeTitle: true);
                    Properties.Load();
                    return true;
                }
                case "/buttlyss reconnect": {
                    AppendChat(chat, "Reconnecting to buttplug server", includeTitle: true);
                    ButtplugManager.Instance.TryReconnectClient();
                    return true;
                }

                default: break;
            }

            return false;
        }

        /// <summary>
        /// Adds a new line into the local chatbox
        /// </summary>
        /// <param name="chat">ChatBehaviour object that has the chat UI element</param>
        /// <param name="text">Text to insert into the chat</param>
        /// <param name="includeTitle">Include the plugin name before the text</param>
        public static void AppendChat(ChatBehaviour chat, string text, bool includeTitle = false) {
            if(includeTitle)
                text = $"[{PluginInfo.NAME}] {text}";

            chat._chatText.text += $"\n{text}";
        }
    }
}
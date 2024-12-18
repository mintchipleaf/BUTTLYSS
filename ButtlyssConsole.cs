using System.Collections.Generic;

namespace BUTTLYSS
{
    public static class ButtlyssConsole
    {
        /// <summary>
        /// Attempts to determine buttlyss-related 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chat"></param>
        /// <returns></returns>
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
                default: break;
            }

            return false;
        }

        public static void AppendChat(ChatBehaviour chat, string text, bool includeTitle = false) {
            if(includeTitle)
                text = $"{PluginInfo.NAME}: {text}";

            chat._chatText.text += $"\n{text}";
        }
    }
}
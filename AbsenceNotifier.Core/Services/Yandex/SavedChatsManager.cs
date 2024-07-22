using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Services.FilesManager;

namespace AbsenceNotifier.Core.Services.Yandex
{
    public class SavedChatsManager : ISavedChatsManager
    {
        private readonly FileManager _fileManager;

        public SavedChatsManager()
        {
            _fileManager = new FileManager();
            var previuosDirectory = Directory.GetParent(Directory.GetCurrentDirectory());
            _fileManager.SetFilePath(Path.Combine(previuosDirectory?.FullName ?? "", "chats-ids.txt"));
        }

        public string GetChatId(string email)
        {
            var allFile = _fileManager.ReadAllLines();
            var keyValues = allFile.Split("\r\n");
            foreach (var keyVal in keyValues)
            {
                if (!string.IsNullOrWhiteSpace(keyVal))
                {
                    var split = keyVal.Split('=');
                    if (split.Length == 2)
                    {
                        var key = split[0];
                        var value = split[1];
                        if (key == email)
                        {
                            return value;
                        }
                    }
                }
            }
            return "";
        }

        public void SaveChatId(string chatId, string email)
        {
            _fileManager.WriteLine($"{email}={chatId}");
        }
    }
}

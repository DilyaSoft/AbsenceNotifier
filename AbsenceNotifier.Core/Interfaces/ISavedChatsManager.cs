namespace AbsenceNotifier.Core.Interfaces
{
    public interface ISavedChatsManager
    {
        public void SaveChatId(string chatId, string email);
        public string GetChatId(string email);
    }
}

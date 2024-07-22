namespace AbsenceNotifier.Core.DTOs.Yandex.Responses
{
    public sealed class UpdateMembersInChatResponse
    {
        public const string DefaultError = "Cannot add new members to chat!";
        public bool Ok { get; set; }
        public string? Description { get; set; }
    }
}

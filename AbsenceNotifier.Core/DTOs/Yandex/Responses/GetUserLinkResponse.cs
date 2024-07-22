namespace AbsenceNotifier.Core.DTOs.Yandex.Responses
{
    public sealed class GetUserLinkResponse
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public bool Ok {  get; set; }
    }
}

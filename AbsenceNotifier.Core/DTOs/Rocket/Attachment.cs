namespace AbsenceNotifier.Core.DTOs.Rocket
{
    public class Attachment
    {
        public string? Text { get; set; }
        public bool Collapsed { get; set; }
        public string? Title { get; set; }
        public IEnumerable<AttachmentField>? Fields { get; set; }
    }
}

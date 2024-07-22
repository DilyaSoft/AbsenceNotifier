using AbsenceNotifier.Core.DTO.Results;

namespace AbsenceNotifier.Core.Interfaces
{
    public interface IEmailService
    {
        Task<SendEmailResult> SendEmailAsync(string email, string subject, string message);

    }
}

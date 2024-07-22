using AbsenceNotifier.Core.Interfaces;
using AbsenceNotifier.Core.Interfaces.TimeOffsService;
using AbsenceNotifier.Core.Services.Messengers;
using AbsenceNotifier.Core.Settings;
using AbsenceNotifier.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AbsenceNotifier.Controllers
{
    /// <summary>
    /// Custom api to create mediator between team's messenger and timetastic
    /// </summary>
    [Route("[controller]")]
    public class TimeOffController : Controller
    {
        private readonly MessengerContext _messengerContext;
        private readonly IDataProtectionProviderHelper _dataProtectionProvider;
        private readonly ITimeTasticService _timeTasticService;

        public TimeOffController(MessengerContext messengerContext,
            ITimeTasticService timeTasticService,
            IDataProtectionProviderHelper dataProtectionProvider)
        {
            _messengerContext = messengerContext;
            _messengerContext.SetMessenger(ApplicationSettings.CurrentMessengerName);
            _dataProtectionProvider = dataProtectionProvider;
            _timeTasticService = timeTasticService;
        }

        /// <summary>
        /// Check healthcare of application
        /// </summary>
        /// <returns></returns>
        [HttpGet("Alive")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public IActionResult GetAlive()
        {
            return Ok("Works");
        }

        /// <summary>
        /// Approve time-off by user id, time-off id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="skipFirstRequest"></param>
        /// <returns>
        /// View with status message
        /// </returns>
        [HttpGet("Approve")]
        public async Task<IActionResult> Approve([FromQuery] string id, [FromQuery] string userId,
            [FromQuery] bool skipFirstRequest = false)
        {
            if (skipFirstRequest && !HttpContext.Session.TryGetValue(id, out var sessionUId)) // yandex chat perfomes get request after the message with link was sent
            {
                HttpContext.Session.SetString(id, id);
                return Ok("If you get this message redirect this link again or refresh page to confirn action.");
            }

            var view = new ApprovalViewModel()
            {
                Message = "Approved"
            };
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId))
            {
                Log.Logger.Information($"Approve can not be done with empty event id {id} or userId {userId}");
                view.Message = "event id or user id was empty";
                return View(view);
            }
            long parsedEventId = 0;
            long parsedUserId = 0;
            try
            {
                var resultEvent = _dataProtectionProvider.GetDecryptedValue(id);
                var resultUser = _dataProtectionProvider.GetDecryptedValue(userId);
                if (!resultEvent.Success || !resultUser.Success || string.IsNullOrEmpty(resultEvent.Value)
                    || string.IsNullOrEmpty(resultUser.Value))
                {
                    view.Message = resultEvent.Message + "\n" + resultUser.Message;
                    return View(view);
                }
                parsedEventId = long.Parse(resultEvent.Value);
                parsedUserId = long.Parse(resultUser.Value);
            }
            catch (Exception ex)
            {
                view.Message = ex.Message;
                view.Message += " " + ex.InnerException?.Message;
                return View(view);
            }
            var timeTasticRequesterResult = await _timeTasticService.GetTimetasticUser(parsedUserId, TimeTasticSettings.AuthHeader);
            if (!timeTasticRequesterResult.Success || timeTasticRequesterResult.TimetasticUser == null)
            {
                view.Message = timeTasticRequesterResult.Message;
                return View(view);
            }
            var result = await _messengerContext.SendEditHolidayResponse(timeTasticRequesterResult.TimetasticUser,
                parsedEventId, Core.Enums.EditHolidayActions.Approve);

            view.Message = result.Message ?? view.Message;
            return View(view);
        }

        /// <summary>
        /// Decline time-off by id and user-id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="skipFirstRequest"></param>
        /// <returns>
        /// View with status message
        /// </returns>
        [HttpGet("Decline")]
        public async Task<IActionResult> DeclineAsync([FromQuery] string id, [FromQuery] string userId,
            [FromQuery] bool skipFirstRequest = false)
        {
            if (skipFirstRequest && !HttpContext.Session.TryGetValue(id, out var sessionUId)) // yandex chat perfomes get request after the message with link was sent
            {
                HttpContext.Session.SetString(id, id);
                return Ok();
            }
            var view = new DeclineViewModel()
            {
                Message = "Declined"
            };
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId))
            {
                view.Message = "event id or user id was empty";
                return View(view);
            }
            long parsedEventId = 0;
            long parsedUserId = 0;
            try
            {
                var resultEvent = _dataProtectionProvider.GetDecryptedValue(id);
                var resultUser = _dataProtectionProvider.GetDecryptedValue(userId);
                if (!resultEvent.Success || !resultUser.Success || string.IsNullOrEmpty(resultEvent.Value)
                    || string.IsNullOrEmpty(resultUser.Value))
                {
                    view.Message = resultEvent.Message + "\n" + resultUser.Message;
                    return View(view);
                }
                parsedEventId = long.Parse(resultEvent.Value);
                parsedUserId = long.Parse(resultUser.Value);
            }
            catch (Exception ex)
            {
                view.Message = ex.Message;
                view.Message += " " + ex.InnerException?.Message;
                return View(view);
            }
            var timeTasticRequesterResult = await _timeTasticService.GetTimetasticUser(parsedUserId, TimeTasticSettings.AuthHeader);
            if (!timeTasticRequesterResult.Success || timeTasticRequesterResult.TimetasticUser == null)
            {
                view.Message = timeTasticRequesterResult.Message;
                return View(view);
            }
            var result = await _messengerContext.SendEditHolidayResponse(timeTasticRequesterResult.TimetasticUser,
                parsedEventId, Core.Enums.EditHolidayActions.Decline);

            view.Message = result.Message ?? view.Message;
            return View(view);
        }
    }
}

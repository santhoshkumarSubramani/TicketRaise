using Microsoft.AspNetCore.Mvc;
using TicketRaise.Model;
using TicketRaise.Repository.Interface;

namespace TicketRaise.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketRaiseController : Controller
    {
        private readonly IRaiseTicket _raiseTicket;
        private readonly IEmailServices _emailService;
        public TicketRaiseController(IRaiseTicket raiseTicket, IEmailServices emailServices)
        {
            _raiseTicket = raiseTicket;
            _emailService = emailServices;
        }

        [HttpPost("Send-Ticket")]
        public async Task<TicketResponse> SendTicket(TicketNotification request)
        {
            TicketResponse response = await _raiseTicket.RaiseTicket(request);

            return response;
        }

        [HttpGet("SendMail")]
        public async Task<bool> SendMail()
        {
            List<Ticket> ticket = await _raiseTicket.GetUnreadTicketsAsync();
            if (ticket.Count > 0)
            {
                var result = await _emailService.SendEmailAdmin(ticket);
                if (result.Where(x => x.Status == "Read").Count() > 0)
                {
                    bool updateStatus = await _raiseTicket.UpdateTicketsAsync(result);
                    return updateStatus;
                }
            }

            return false;
        }
    }
}

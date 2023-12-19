using Microsoft.AspNetCore.Mvc;
using Tradeify.Models;

namespace Tradeify.Controllers
{
    public class BaseController : Controller
    {
        protected void SetMessage(string message, Message.Category messageType)
        {
            Message msg = new Message(message, (int)messageType);
            TempData["Message"] = msg;
        }
    }
}

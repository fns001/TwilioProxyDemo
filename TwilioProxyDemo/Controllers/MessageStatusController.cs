using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwilioProxyDemo.Services;

namespace TwilioProxyDemo.Controllers
{
    /// <summary>
    /// API Controller to handle status call backs. Use https://requestbin.com/ to test in a dev environment
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MessageStatusController : ControllerBase
    {
        private readonly ILogger<MessageStatusController> logger;
        private readonly SmsAlertService smsAlertService;

        public MessageStatusController(ILogger<MessageStatusController> logger, SmsAlertService smsAlertService)
        {
            this.logger = logger;
            this.smsAlertService = smsAlertService;
        }

        [HttpPost]
        public ActionResult Post(
            [FromForm] string AccountSid,
            [FromForm] string MessageSid, 
            [FromForm] string MessageStatus, 
            [FromForm] string From,
            [FromForm] string To
            )
        {
            logger.LogInformation("Message status webhook api triggered. From:{0}. To:{1}. MessageSid:{2}. Status:{3}", From, To, MessageSid, MessageStatus);
            smsAlertService.CreateAlertByStatus(MessageStatus);
            return Ok();
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public ActionResult Get(string id)
        {
            return Ok(id);
        }
    }


}

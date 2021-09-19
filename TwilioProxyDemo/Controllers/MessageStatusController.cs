using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private readonly ProxyAlertService proxyAlertService;

        public MessageStatusController(ILogger<MessageStatusController> logger, ProxyAlertService proxyAlertService)
        {
            this.logger = logger;
            this.proxyAlertService = proxyAlertService;
        }

        [HttpPost]
        public ActionResult Post(
            [FromForm] string outboundResourceStatus,
            [FromForm] string outboundResourceType, 
            [FromForm] string interactionDateUpdated, 
            [FromForm] string interactionData,
            [FromForm] string outboundParticipantSid,
            [FromForm] string interactionSessionSid,
            [FromForm] string inboundParticipantSid,
            [FromForm] string inboundResourceSid,
            [FromForm] string interactionType
            )
        {
            //more at: https://www.twilio.com/docs/proxy/api/webhooks

            var loginfo = new List<string> 
            { 
                outboundResourceStatus, 
                outboundResourceType,
                interactionDateUpdated,
                interactionData,
                outboundParticipantSid,
                interactionSessionSid,
                inboundParticipantSid,
                inboundResourceSid,
                interactionType
            };
            logger.LogInformation("Proxy service webhook api triggered. {loginfo}", loginfo);
            string interactionBody = JsonSerializer.Deserialize<InteractionObject>(interactionData)?.body;
            proxyAlertService.Participants.TryGetValue(inboundParticipantSid ?? "", out string fromName);
            if (fromName == null)
            {
                fromName = "Twilio";
            }
            if (outboundResourceStatus == "delivered" && !string.IsNullOrEmpty(interactionBody))
            {
                proxyAlertService.CreateProxyAlert($"[{fromName}]: {interactionBody}");
            }

            //example post:
            /*
            "root":
            "outboundResourceStatus": "delivered"
            "outboundResourceType": "message"
            "interactionDateUpdated": "2021-09-18T06:06:12.818Z"
            "interactionData": "{"body":"Reply to this message to chat!"}"
            "interactionDateCreated": "2021-09-18T06:06:09Z"
            "interactionServiceSid": "KS8426b29b1025149fd5506c9079d81818"
            "outboundParticipantSid": "KPff30a1e616f9eeeedc281ca0d2d05ba9"
            "interactionType": "Message"
            "interactionAccountSid": "AC5cc3e22f18b62a31747ffdae83b93eba"
            "outboundResourceSid": "SM0da6e02db7a1bdf9d1460bb08d3a98a2"
            "outboundResourceUrl": "https://api.twilio.com/2010-04-01/Accounts/AC5cc3e22f18b62a31747ffdae83b93eba/Messages/SM0da6e02db7a1bdf9d1460bb08d3a98a2.json"
            "interactionNumMedia": "0"
            "interactionSessionSid": "KCb289bf48756095361d24d8c44d9fd35a"
            "interactionSid": "KIa8c5c79b349bb15d7ba6d151bd60dd63"
            */

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

    public class InteractionObject
    {
        public string body { get; set; }
    }


}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using TwilioProxyDemo.Models;
using Twilio.Rest.Proxy.V1;
using Twilio.Rest.Proxy.V1.Service;
using Twilio.Rest.Proxy.V1.Service.Session;
using Twilio.Rest.Proxy.V1.Service.Session.Participant;

namespace TwilioProxyDemo.Services
{
    public class ProxyService
    {
        private const string DEFAULT_SESSION_NAME = "Session_Alice_Bob";
        private readonly PhoneNumberUtil phoneNumberUtil;
        private readonly IConfiguration configuration;
        private readonly ILogger<ProxyService> logger;
        private readonly ProxyAlertService proxyAlertService;

        public ProxyService(IConfiguration configuration, ILogger<ProxyService> logger, ProxyAlertService proxyAlertService)
        {
            phoneNumberUtil = PhoneNumberUtil.GetInstance();        
            this.logger = logger;
            this.configuration = configuration;
            this.proxyAlertService = proxyAlertService;
        }

        public async Task<string> CreateSessionAsync()
        {
            ServiceResource proxyService = GetProxyService();

            //await Task.Delay(3000);
            //return "in-progress";

            var session = await SessionResource.CreateAsync(
                uniqueName: DEFAULT_SESSION_NAME,
                pathServiceSid: proxyService.Sid
            );

            var participant1Name = configuration["Participant1Name"];
            var participant1Number = configuration["Participant1Number"];
            var participant2Name = configuration["Participant2Name"];
            var participant2Number = configuration["Participant2Number"];

            var participant1 = await ParticipantResource.CreateAsync(
                friendlyName: participant1Name,
                identifier: participant1Number,
                pathServiceSid: proxyService.Sid,
                pathSessionSid: session.Sid
            );

            var participant2 = await ParticipantResource.CreateAsync(
                friendlyName: participant2Name,
                identifier: participant2Number,
                pathServiceSid: proxyService.Sid,
                pathSessionSid: session.Sid
            );

            proxyAlertService.Participants = new Dictionary<string, string>
            {
                { participant1.Sid, participant1.FriendlyName },
                { participant2.Sid, participant2.FriendlyName }
            };

            logger.LogInformation("Participants created");

            var messageInteraction = await MessageInteractionResource.CreateAsync(
                body: $"Hello {participant1.FriendlyName}. Reply here to chat with {participant2.FriendlyName}",
                pathServiceSid: proxyService.Sid,
                pathSessionSid: session.Sid,
                pathParticipantSid: participant1.Sid
            );

            logger.LogInformation("Message Interaction Created");

            string sessionStatus = session.Status.ToString().ToLower();
            return sessionStatus;
        }

        public string OpenSession(bool open)
        {
            ServiceResource proxyService = GetProxyService();
            var sessions = SessionResource.Read(proxyService.Sid);
            
            var s = sessions.Where(i => i.UniqueName == DEFAULT_SESSION_NAME).FirstOrDefault();
            
            var session = SessionResource.Update(
                status: open ? SessionResource.StatusEnum.InProgress : SessionResource.StatusEnum.Closed,
                pathServiceSid: proxyService.Sid,
                pathSid: s.Sid
            );

            string updatedStatus = session?.Status?.ToString()?.ToLower();
            logger.LogInformation("Session status updated to {0}", updatedStatus);
            return updatedStatus;
        }

        public string GetSession()
        {
            ServiceResource proxyService = GetProxyService();
            var sessions = SessionResource.Read(proxyService.Sid);
            var session = sessions.Where(i => i.UniqueName == DEFAULT_SESSION_NAME).FirstOrDefault();

            if (session == null)
            {
                logger.LogInformation("No session found");
                return null;
            }
            
            var participants = ParticipantResource.Read(proxyService.Sid, session.Sid);
            proxyAlertService.Participants = new Dictionary<string, string>();
            foreach (var p in participants)
            {
                proxyAlertService.Participants.Add(p.Sid, p.FriendlyName);
            }
            logger.LogInformation("Session retrieved. Participants. {0}", proxyAlertService.Participants);
            return session?.Status?.ToString()?.ToLower();
        }

        private ServiceResource GetProxyService()
        {
            string accountSid = configuration["AccountSID"];
            string authToken = configuration["AuthToken"];
            string proxyServiceSID = configuration["ProxyServiceSID"];
            TwilioClient.Init(accountSid, authToken);
            var proxyService = ServiceResource.Fetch(pathSid: proxyServiceSID);
            return proxyService;
        }

        private void CreateProxyService()
        {
            var guid = Guid.NewGuid().ToString();
            var proxyService = ServiceResource.Create(uniqueName: $"proxy_service_resouce_{guid}");
            AddProxyPhoneNumber(proxyService);
        }

        private void AddProxyPhoneNumber(ServiceResource proxyService)
        {
            try
            {
                var phoneNumber = PhoneNumberResource.Create(
                    sid: configuration["ProxyPhoneNumberSID"],
                    pathServiceSid: proxyService.Sid
                );
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }
    }
}

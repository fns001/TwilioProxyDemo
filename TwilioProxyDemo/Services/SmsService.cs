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

namespace TwilioProxyDemo.Services
{
    public class SmsService
    {
        private readonly PhoneNumberUtil phoneNumberUtil;
        private readonly IConfiguration configuration;
        private readonly ILogger<SmsService> logger;
        private readonly SmsAlertService smsAlertService;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger, SmsAlertService smsAlertService)
        {
            phoneNumberUtil = PhoneNumberUtil.GetInstance();        
            this.logger = logger;
            this.configuration = configuration;
            this.smsAlertService = smsAlertService;
        }

        public async Task SendSMS(string toNumber, string region)
        {
            //Create client
            string fromE164Number = configuration["FromNumber"];
            string accountSid = configuration["AccountSID"];
            string authToken = configuration["AuthToken"];
            if (new string[] { fromE164Number, accountSid, authToken }.Any(s => string.IsNullOrWhiteSpace(s) || s.Contains("not set")))
            {
                logger.LogError("App secrets has not been configured");
                smsAlertService.ShowAlert("", "Phone number, account SID and authentication token has not been configured. See the Readme file for more information.", Blazorise.Color.Danger);            
                return;
            }
            TwilioClient.Init(accountSid, authToken);     
            
            //Validate number
            var toPhoneNumber = phoneNumberUtil.Parse(toNumber, region);
            var isPhoneNumberValid = phoneNumberUtil.IsValidNumber(toPhoneNumber);
            if (!isPhoneNumberValid)
            {
                logger.LogWarning("Phone number invalid: {0}", toPhoneNumber.NationalNumber);
                return;
            }

            //Good to go. Create message
            string toE164number = $"+{toPhoneNumber.CountryCode}{toPhoneNumber.NationalNumber}"; //more on e164: https://www.twilio.com/docs/glossary/what-e164
            logger.LogInformation("Sending SMS to {0}", toE164number);
            try
            {
                var statusCallBack = configuration["StatusCallBack"];
                var statusCallBackUri = string.IsNullOrWhiteSpace(statusCallBack) ? null : new Uri(statusCallBack);
                var message = await MessageResource.CreateAsync(
                    body: "Just go ahead and press that button", 
                    from: new Twilio.Types.PhoneNumber(fromE164Number),
                    to: new Twilio.Types.PhoneNumber(toE164number),
                    statusCallback: statusCallBackUri
                );
                logger.LogInformation("Message created. SID {0}. Status {1}", message.Sid, message.Status);
                smsAlertService.CreateAlertByStatus(message.Status.ToString().ToLower());
            }
            catch (Exception e)
            {
                logger.LogError("SendSMS exception. {0}", e.Message);
                if (e.Message.Contains("is unverified"))
                {
                    smsAlertService.ShowAlert("", e.Message, Blazorise.Color.Danger);
                }
                else
                {
                    smsAlertService.ShowAlert("Error sending SMS", "Please see the log file for more information.", Blazorise.Color.Danger);
                }            
            }
        }

        public List<CountrySelectModel> GetCountriesSelectList()
        {
            //This is hard-coded for the demo.
            //Link it to another data souce for a more comprehensive list of country codes

            return new List<CountrySelectModel> 
            { 
                new CountrySelectModel 
                { 
                    Value = "AU", Text = "Australia (+61)" 
                }, 
                new CountrySelectModel 
                { 
                    Value = "US", Text = "US (+1)" 
                } 
            };
        }

    }
}

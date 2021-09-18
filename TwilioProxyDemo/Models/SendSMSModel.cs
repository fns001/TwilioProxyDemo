using System.ComponentModel.DataAnnotations;

namespace TwilioProxyDemo.Models
{
    public class SendSMSModel
    {
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Country required")]
        [MinLength(2, ErrorMessage = "Country invalid")]
        [MaxLength(2, ErrorMessage = "Country invalid")]
        public string CountryCode { get; set; }

        [Required(ErrorMessage = "Number required")]
        [DataType(DataType.PhoneNumber)]
        [CustomPhoneNumberValidation(ErrorMessage = "Number is invalid")]
        public string ToPhoneNumber { get; set; }

        public string Message { get; set; }
    }
}

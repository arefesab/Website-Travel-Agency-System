using System.Threading.Tasks;

namespace agancywebProject.Services.Payment
{
    public interface IPaymentGateway
    {
        bool IsSimulated { get; }

        Task<PaymentRequestResult> RequestPaymentAsync(PaymentRequestModel model);

        Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, int amountToman);
    }

    public class PaymentRequestModel
    {
        public int AmountToman { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public string? Mobile { get; set; }

        public string FakeGatewayController { get; set; } = "Booking";
    }

    public class PaymentRequestResult
    {
        public bool Success { get; set; }
        public string? Authority { get; set; }
        public string? PaymentUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PaymentVerifyResult
    {
        public bool Success { get; set; }
        public string? RefId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

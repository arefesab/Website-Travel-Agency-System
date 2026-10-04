using System;
using System.Threading.Tasks;

namespace agancywebProject.Services.Payment
{
    public class FakePaymentGateway : IPaymentGateway
    {
        public bool IsSimulated => true;

        public Task<PaymentRequestResult> RequestPaymentAsync(PaymentRequestModel model)
        {
            var authority = "FAKE-" + Guid.NewGuid().ToString("N");
            return Task.FromResult(new PaymentRequestResult
            {
                Success = true,
                Authority = authority,
                PaymentUrl = $"/{model.FakeGatewayController}/FakeGateway/{authority}"
            });
        }

        public Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, int amountToman)
        {
            var refId = "TEST" + new Random().Next(100000, 999999);
            return Task.FromResult(new PaymentVerifyResult { Success = true, RefId = refId });
        }
    }
}

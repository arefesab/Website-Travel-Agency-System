using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace agancywebProject.Services.Payment
{
    public class ZarinpalGateway : IPaymentGateway
    {
        private readonly HttpClient _http;
        private readonly string _merchantId;
        private readonly bool _sandbox;

        public ZarinpalGateway(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _http = httpClientFactory.CreateClient("zarinpal");
            _merchantId = config["Payment:Zarinpal:MerchantId"] ?? "";
            _sandbox = bool.TryParse(config["Payment:Zarinpal:Sandbox"], out var s) ? s : true;
        }

        private string BaseUrl => _sandbox
            ? "https://sandbox.zarinpal.com/pg/rest/WebGate/"
            : "https://api.zarinpal.com/pg/rest/WebGate/";

        private string GatewayUrl => _sandbox
            ? "https://sandbox.zarinpal.com/pg/StartPay/"
            : "https://www.zarinpal.com/pg/StartPay/";

        public bool IsSimulated => false;

        public async Task<PaymentRequestResult> RequestPaymentAsync(PaymentRequestModel model)
        {
            try
            {
                var payload = new
                {
                    MerchantID = _merchantId,
                    Amount = (long)(model.AmountToman * 10),
                    Description = model.Description,
                    CallbackURL = model.CallbackUrl,
                    Mobile = model.Mobile
                };

                var response = await _http.PostAsJsonAsync(BaseUrl + "PaymentRequest.json", payload);
                if (!response.IsSuccessStatusCode)
                {
                    return new PaymentRequestResult { Success = false, ErrorMessage = "ارتباط با درگاه پرداخت برقرار نشد." };
                }

                var result = await response.Content.ReadFromJsonAsync<ZarinpalRequestResponse>();
                if (result != null && result.Status == 100 && !string.IsNullOrEmpty(result.Authority))
                {
                    return new PaymentRequestResult
                    {
                        Success = true,
                        Authority = result.Authority,
                        PaymentUrl = GatewayUrl + result.Authority
                    };
                }

                return new PaymentRequestResult
                {
                    Success = false,
                    ErrorMessage = $"درگاه پرداخت درخواست را نپذیرفت (کد {result?.Status})."
                };
            }
            catch (Exception ex)
            {
                return new PaymentRequestResult { Success = false, ErrorMessage = "خطا در اتصال به درگاه پرداخت: " + ex.Message };
            }
        }

        public async Task<PaymentVerifyResult> VerifyPaymentAsync(string authority, int amountToman)
        {
            try
            {
                var payload = new
                {
                    MerchantID = _merchantId,
                    Authority = authority,
                    Amount = (long)(amountToman * 10)
                };

                var response = await _http.PostAsJsonAsync(BaseUrl + "PaymentVerification.json", payload);
                if (!response.IsSuccessStatusCode)
                {
                    return new PaymentVerifyResult { Success = false, ErrorMessage = "ارتباط با درگاه پرداخت برای تایید تراکنش برقرار نشد." };
                }

                var result = await response.Content.ReadFromJsonAsync<ZarinpalVerifyResponse>();
                if (result != null && (result.Status == 100 || result.Status == 101))
                {
                    return new PaymentVerifyResult { Success = true, RefId = result.RefID?.ToString() };
                }

                return new PaymentVerifyResult { Success = false, ErrorMessage = $"پرداخت تایید نشد (کد {result?.Status})." };
            }
            catch (Exception ex)
            {
                return new PaymentVerifyResult { Success = false, ErrorMessage = "خطا در تایید پرداخت: " + ex.Message };
            }
        }

        private class ZarinpalRequestResponse
        {
            [JsonPropertyName("Status")]
            public int Status { get; set; }
            [JsonPropertyName("Authority")]
            public string? Authority { get; set; }
        }

        private class ZarinpalVerifyResponse
        {
            [JsonPropertyName("Status")]
            public int Status { get; set; }
            [JsonPropertyName("RefID")]
            public long? RefID { get; set; }
        }
    }
}

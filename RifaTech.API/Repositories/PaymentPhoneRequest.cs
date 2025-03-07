using MercadoPago.Client.Payment;

namespace RifaTech.API.Repositories
{
    public class PaymentPhoneRequest : PaymentPayerPhoneRequest
    {
        public string AreaCode { get; set; }
        public string Number { get; set; }
    }
}
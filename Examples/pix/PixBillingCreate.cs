using Abacatepay;
using Newtonsoft.Json.Linq;

namespace Examples
{
    class PixBillingCreate
    {
        public static void Execute()
        {
            dynamic abacate = new AbacatePay(JObject.Parse(File.ReadAllText("credentials.json")));

            var body = new
            {
                frequency = "ONE_TIME",
                methods = new[] { "PIX" },
                products = new[]
                {
                    new
                    {
                        externalId = "prod-1234",
                        name = "Teste",
                        description = "Teste",
                        quantity = 1,
                        price = 1000
                    }
                },
                returnUrl = "https://example.com/billing",
                completionUrl = "https://example.com/completion",
                customer = new
                {
                    name = "Daniel Lima",
                    cellphone = "(11) 4002-8922",
                    email = "daniel_lima@abacatepay.com",
                    taxId = "123.456.789-01"
                }
            };

            try
            {
                var response = abacate.PixBillingCreate(null, body);
                Console.WriteLine(response);
            }
            catch (AbacateException e)
            {
                Console.WriteLine(e.ErrorType);
                Console.WriteLine(e.Message);
            }
        }
    }
}
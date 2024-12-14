using Abacatepay;
using Newtonsoft.Json.Linq;

namespace Examples
{
    class CustomerCreate
    {
        public static void Execute()
        {
            dynamic abacate = new AbacatePay(JObject.Parse(File.ReadAllText("credentials.json")));

            var body = new
            {
                name = "Daniel Lima",
                cellphone = "(11) 4002-8922",
                email = "daniel_lima@abacatepay.com",
                taxId = "123.456.789-01"
            };

            try
            {
                var response = abacate.CustomerCreate(null, body);
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
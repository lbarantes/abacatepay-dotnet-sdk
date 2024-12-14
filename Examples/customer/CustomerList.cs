using Abacatepay;
using Newtonsoft.Json.Linq;

namespace Examples
{
    class CustomerList
    {
        public static void Execute()
        {
            dynamic abacate = new AbacatePay(JObject.Parse(File.ReadAllText("credentials.json")));

            try
            {
                var response = abacate.CustomerList();
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
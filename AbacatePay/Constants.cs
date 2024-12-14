using Newtonsoft.Json;

namespace Abacatepay
{
    public class Constants
    {
        public object Const = new
        {
            APIS = new
            {
                PIX = new
                {
                    URL = new
                    {
                        production = "https://api.abacatepay.com",
                        sandbox = "https://api.abacatepay.com"
                    },
                    ENDPOINTS = new
                    {
                        PixBillingCreate = new
                        {
                            route = "/v1/billing/create",
                            method = "POST"
                        },
                        PixBillingList = new
                        {
                            route = "/v1/billing/list",
                            method = "GET"
                        }
                    }
                },
                CUSTOMER = new
                {
                    URL = new
                    {
                        production = "https://api.abacatepay.com",
                        sandbox = "https://api.abacatepay.com"
                    },
                    ENDPOINTS = new
                    {
                        CustomerCreate = new
                        {
                            route = "/v1/customer/create",
                            method = "POST"
                        },
                        CustomerList = new
                        {
                            route = "/v1/customer/list",
                            method = "GET"
                        }
                    }
                }
            }
        };
        public string getConstant()
        {
            string jsonString = JsonConvert.SerializeObject(Const);
            return jsonString;
        }
    }
}
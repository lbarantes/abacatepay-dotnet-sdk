using System.Dynamic;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Abacatepay
{
    public class AbacatePay : DynamicObject
    {
        private const string version = "1.0.0";
        private static string apiKey;
        private static JObject constants;
        private static JObject urls;
        private static bool sandbox;
        private string baseURL;

        public AbacatePay(JObject options)
        {
            ApiKey = (string)options["apiKey"];
            Sandbox = (bool)options["sandbox"];
            Constants constant = new Constants();
            constants = JObject.Parse(constant.getConstant());
        }

        public AbacatePay(string apiKey, bool sandbox)
        {
            ApiKey = apiKey;
            Sandbox = sandbox;
            Constants constant = new Constants();
            constants = JObject.Parse(constant.getConstant());
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            JObject endpoint = null;

            if ((JObject)constants["APIS"]["PIX"]["ENDPOINTS"][binder.Name] != null)
            {
                urls = (JObject)constants["APIS"]["PIX"]["URL"];
                baseURL = Sandbox ? (string)urls["sandbox"] : (string)urls["production"];
                endpoint = (JObject)constants["APIS"]["PIX"]["ENDPOINTS"][binder.Name];
            }

            if ((JObject)constants["APIS"]["CUSTOMER"]["ENDPOINTS"][binder.Name] != null)
            {
                urls = (JObject)constants["APIS"]["CUSTOMER"]["URL"];
                baseURL = Sandbox ? (string)urls["sandbox"] : (string)urls["production"];
                endpoint = (JObject)constants["APIS"]["CUSTOMER"]["ENDPOINTS"][binder.Name];
            }

            if (endpoint == null)
                throw new AbacateException(0, "invalid_endpoint", string.Format("Método '{0}' inexistente", binder.Name));

            if (baseURL == "")
                throw new AbacateException(1, "invalid_environment", "Endpoint não disponível para o ambiente informado.");

            var route = (string)endpoint["route"];
            var method = (string)endpoint["method"];
            object query = args.Length > 0 ? args[0] : null;
            object body = args.Length > 1 ? args[1] : null;

            try
            {
                result = RequestEndpoint(route, method, query, body);
                return true;
            }
            catch (AbacateException e)
            {
                if (e.Code == 401)
                {
                    throw new AbacateException(401, "Unauthorized", "Não foi possível autenticar. Por favor, certifique-se de sua chave de api está correta.");
                }
                else
                {
                    throw new AbacateException(500, "server_error",
                                           "Ocorreu um erro ao realizar a requisição.");
                }
            }
        }

        private object RequestEndpoint(string endpoint, string method, object query, object body)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string newEndpoint = GetEndpointRequest(endpoint, method, query);

            var request = new RestRequest();

            if (method == "GET") request.Method = Method.Get;
            else if (method == "POST") request.Method = Method.Post;

            request.AddHeader("Authorization", string.Format("Bearer {0}", ApiKey));
            request.AddHeader("api-sdk", string.Format("abacatepay-dotnet-{0}", version));

            try
            {
                return SendRequest(request, body, newEndpoint);
            }
            catch (WebException e)
            {
                if (e.Response != null && e.Response is HttpWebResponse)
                {
                    var statusCode = (int)((HttpWebResponse)e.Response).StatusCode;
                    var reader = new StreamReader(e.Response.GetResponseStream());
                    throw AbacateException.Build(reader.ReadToEnd(), statusCode);
                }
                throw AbacateException.Build("", 500);
            }
        }


        public string GetEndpointRequest(string endpoint, string method, object query)
        {
            if (query != null)
            {
                var attr = BindingFlags.Public | BindingFlags.Instance;
                var queryDict = new Dictionary<string, object>();
                foreach (var property in query.GetType().GetProperties(attr))
                    if (property.CanRead)
                        queryDict.Add(property.Name, property.GetValue(query, null));

                var matchCollection = Regex.Matches(endpoint, ":([a-zA-Z0-9]+)");
                for (var i = 0; i < matchCollection.Count; i++)
                {
                    var resource = matchCollection[i].Groups[1].Value;
                    try
                    {
                        var value = queryDict[resource].ToString();
                        endpoint = Regex.Replace(endpoint, string.Format(":{0}", resource), value);
                        queryDict.Remove(resource);
                    }
                    catch (Exception) { }
                }

                var queryString = "";
                foreach (var pair in queryDict)
                {
                    if (queryString.Equals(""))
                        queryString = "?";
                    else
                        queryString += "&";
                    queryString += string.Format("{0}={1}", pair.Key, pair.Value);
                }

                endpoint += queryString;
            }

            return endpoint;
        }

        public dynamic SendRequest(RestRequest request, object body, string newEndpoint)
        {
            if (body != null)
                request.AddJsonBody(body);

            var client = new RestClient(baseURL + newEndpoint);

            RestResponse restResponse = client.Execute(request);
            string response = restResponse.Content;
            return response;
        }


        private static string ApiKey { get => apiKey; set => apiKey = value; }
        public static bool Sandbox { get => sandbox; set => sandbox = value; }
    }
}
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
        private static bool testing;
        private string baseURL;

        public AbacatePay(JObject options)
        {
            ApiKey = (string)options["apiKey"];
            Testing = (bool)options["testing"];
            Constants constant = new Constants();
            constants = JObject.Parse(constant.getConstant());
        }

        public AbacatePay(string apiKey, bool testing)
        {
            ApiKey = apiKey;
            Testing = testing;
            Constants constant = new Constants();
            constants = JObject.Parse(constant.getConstant());
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            JObject endpoint = null;
            string apiType = constants["APIS"].Children<JProperty>()
                                     .FirstOrDefault(prop => prop.Value["ENDPOINTS"][binder.Name] != null)?.Name;

            if (apiType != null)
            {
                baseURL = (string)constants["URL"];
                endpoint = (JObject)constants["APIS"][apiType]["ENDPOINTS"][binder.Name];
            }

            if (endpoint == null)
                throw new AbacateException(0, "invalid_endpoint", $"Método '{binder.Name}' inexistente");

            if (string.IsNullOrEmpty(baseURL))
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
            catch (AbacateException e) when (e.Code == 401)
            {
                throw new AbacateException(401, "Unauthorized", "Não foi possível autenticar. Por favor, certifique-se de sua chave de api está correta.");
            }
            catch (AbacateException)
            {
                throw new AbacateException(500, "server_error", "Ocorreu um erro ao realizar a requisição.");
            }
        }

        private object RequestEndpoint(string endpoint, string method, object query, object body)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string newEndpoint = GetEndpointRequest(endpoint, method, query);

            var request = new RestRequest
            {
                Method = method switch
                {
                    "GET" => Method.Get,
                    "POST" => Method.Post,
                    _ => throw new AbacateException(400, "invalid_method", "Método HTTP inválido.")
                }
            };

            request.AddHeader("Authorization", $"Bearer {ApiKey}");
            request.AddHeader("api-sdk", $"abacatepay-dotnet-{version}");

            try
            {
                return SendRequest(request, body, newEndpoint);
            }
            catch (WebException e) when (e.Response is HttpWebResponse response)
            {
                using var reader = new StreamReader(response.GetResponseStream());
                var statusCode = (int)response.StatusCode;
                throw AbacateException.Build(reader.ReadToEnd(), statusCode);
            }
            catch
            {
                throw AbacateException.Build("", 500);
            }
        }


        public string GetEndpointRequest(string endpoint, string method, object query)
        {
            if (query == null) return endpoint;

            var queryDict = query.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead)
                                 .ToDictionary(p => p.Name, p => p.GetValue(query, null)?.ToString());

            var matches = Regex.Matches(endpoint, ":([a-zA-Z0-9]+)");

            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                if (queryDict.TryGetValue(key, out var value))
                {
                    endpoint = endpoint.Replace($":{key}", value);
                    queryDict.Remove(key);
                }
            }

            if (queryDict.Any())
            {
                var queryString = string.Join("&", queryDict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                endpoint += "?" + queryString;
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
        public static bool Testing { get => testing; set => testing = value; }
    }
}
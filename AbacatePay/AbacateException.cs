using System;
using Newtonsoft.Json;

namespace Abacatepay
{
    public class AbacateException : Exception
    {
        public AbacateException(int code, string error, string message) : base(message)
        {
            Code = code;
            ErrorType = error;
        }

        public int Code { get; }

        public string ErrorType { get; }

        public static AbacateException Build(string json, int statusCode)
        {
            try
            {
                object def = new { };
                dynamic jsonObject = JsonConvert.DeserializeAnonymousType(json, def);

                int code = jsonObject.code;
                string error = jsonObject.error.ToString();
                string description = jsonObject.error_description.ToString();
                return new AbacateException(code, error, description);
            }
            catch (Exception)
            {
                if (statusCode == 401)
                    throw new AbacateException(401, "authorization_error",
                        "Não foi possível autenticar. Por favor, certifique-se de sua chave de api está correta.");
                return new AbacateException(500, "internal_server_error", "Ocorreu um erro no servidor");
            }
        }
    }

}

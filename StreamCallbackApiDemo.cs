using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Com.Netease.Is.Antispam.Demo
{
    class AigcStreamCallbackAPIDemo
    {
        private static readonly string SECRETID = "your_secret_id";
        private static readonly string SECRETKEY = "your_secret_key";
        private static readonly string API_URL = "https://as.dun.163.com/v1/stream/callback/results";

        public static void AigcStreamCallback()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "secretId", SECRETID },
                { "version", "v1" },
                { "timestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
                { "nonce", new Random().Next().ToString() },
                { "signatureMethod", "MD5" }
            };

            string signature = Utils.genSignature(SECRETKEY, parameters);
            parameters.Add("signature", signature);

            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, API_URL, parameters, 10000);
            
            JObject jObject = JObject.Parse(result);
            int code = jObject["code"].Value<int>();
            string msg = jObject["msg"].Value<string>();

            if (code == 200)
            {
                JArray resultArray = (JArray)jObject.SelectToken("result");
                if (resultArray == null || !resultArray.HasValues)
                {
                    Console.WriteLine("No results available at the moment. Please try again later.");
                }
                else
                {
                    foreach (var streamCheckResult in resultArray)
                    {
                        JObject tmp = (JObject)streamCheckResult;
                        string sessionTaskId = streamCheckResult["sessionTaskId"].ToObject<String>();
                        string sessionIdReturn = streamCheckResult["sessionId"].ToObject<String>();
                        JObject antispam = tmp.GetValue("antispam").ToObject<JObject>();
                        string suggestion = antispam["suggestion"].ToObject<String>();
                        string label = antispam["label"].ToObject<String>();
                        Console.WriteLine($"sessionTaskId={sessionTaskId}, sessionId={sessionIdReturn}, suggestion={suggestion}, label={label}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"ERROR: code={code}, msg={msg}");
            }
        }
    }
}
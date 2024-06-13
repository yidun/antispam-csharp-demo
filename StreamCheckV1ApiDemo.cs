using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Com.Netease.Is.Antispam.Demo
{
    class AigcStreamPushAPIDemo
    {
        private static readonly string SECRETID = "your_secret_id";
        private static readonly string SECRETKEY = "your_secret_key";
        private static readonly string API_URL = "https://as.dun.163.com/v1/stream/push";

        // Input check demo
        // PushDemoForInputCheck(sessionId);

        // Output stream check demo
        // PushDemoForOutputStreamCheck(sessionId);

        // Close session demo
        // PushDemoForOutputStreamClose(sessionId);

        public static void PushDemoForOutputStreamClose()
        {
            Dictionary<string, string> parameters = PrepareParams();
            parameters.Add("sessionId", "yourSessionId" + DateTimeOffset.Now.ToUnixTimeMilliseconds());
            parameters.Add("type", "3");
            InvokeAndParseResponse(parameters);
        }

        public static void PushDemoForOutputStreamCheck()
        {
            Dictionary<string, string> parameters = PrepareParams();
            parameters.Add("sessionId", "yourSessionId" + DateTimeOffset.Now.ToUnixTimeMilliseconds());
            parameters.Add("type", "1");
            parameters.Add("dataId", "yourDataId");
            parameters.Add("content", "Current output segment 1");
            parameters.Add("publishTime", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
            InvokeAndParseResponse(parameters);
        }

        public static void PushDemoForInputCheck()
        {
            Dictionary<string, string> parameters = PrepareParams();
            parameters.Add("sessionId", "yourSessionId" + DateTimeOffset.Now.ToUnixTimeMilliseconds());
            parameters.Add("type", "2");
            parameters.Add("dataId", "yourDataId");
            parameters.Add("content", "Current input content");
            parameters.Add("publishTime", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
            InvokeAndParseResponse(parameters);
        }

        private static Dictionary<string, string> PrepareParams()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "secretId", SECRETID },
                { "version", "v1" },
                { "timestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
                { "nonce", new Random().Next().ToString() },
                { "signatureMethod", "MD5" }
            };
            return parameters;
        }

        private static void InvokeAndParseResponse(Dictionary<string, string> parameters)
        {
            String signature = Utils.genSignature(SECRETKEY, parameters);
            parameters.Add("signature", signature);

            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, API_URL, parameters, 10000);

            JObject jObject = JObject.Parse(result);
            int code = jObject["code"].Value<int>();
            string msg = jObject["msg"].Value<string>();

            if (code == 200)
            {
                JObject streamCheckResult = (JObject)jObject["result"];
                if (streamCheckResult != null)
                {
                    string sessionTaskId = streamCheckResult["sessionTaskId"].ToObject<String>();
                    string sessionIdReturn = streamCheckResult["sessionId"].ToObject<String>();
                    JObject antispam = streamCheckResult["antispam"] as JObject;
                    Console.WriteLine($"sessionTaskId={sessionTaskId}, sessionId={sessionIdReturn}, antispam={antispam}");
                }
            }
            else
            {
                Console.WriteLine($"ERROR: code={code}, msg={msg}");
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Com.Netease.Is.Antispam.Demo
{
    class Utils
    {
        // 根据secretKey和parameters生成签名
        public static String genSignature(String secretKey, Dictionary<String, String> parameters)
        {
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<String, String> kv in parameters)
            {
                builder.Append(kv.Key).Append(kv.Value);
            }
            builder.Append(secretKey);
            String tmp = builder.ToString();
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(tmp));
            builder.Clear();
            foreach (byte b in result)
            {
                builder.Append(b.ToString("x2").ToLower());
            }
            return builder.ToString();
        }

        public static HttpClient makeHttpClient()
        {
            HttpClient client = new HttpClient() {};
            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.SendAsync(new HttpRequestMessage
            {
                Method = new HttpMethod("HEAD"),
                RequestUri = new Uri("https://api.aq.163.com")
            }).Wait();
            return client;
        }

        // 执行post操作
        public static String doPost(HttpClient client, String url, Dictionary<String, String> parameters, int timeOutInMillisecond)
        {
            HttpContent content = new FormUrlEncodedContent(parameters);
            Task<HttpResponseMessage> task = client.PostAsync(url, content);
            if (task.Wait(timeOutInMillisecond))
            {
                HttpResponseMessage response = task.Result;
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    Task<string> result = response.Content.ReadAsStringAsync();
                    result.Wait();
                    return result.Result;
                }
            }
            return null;
        }
    }
}

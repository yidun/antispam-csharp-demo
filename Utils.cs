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
            parameters = parameters.OrderBy(o => o.Key, StringComparer.Ordinal).ToDictionary(o => o.Key, p => p.Value);
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
                RequestUri = new Uri("http://as.dun.163yun.com")
            }).Wait();
            return client;
        }

        // 执行post操作
        public static String doPost(HttpClient client, String url, Dictionary<String, String> parameters, int timeOutInMillisecond)
        {
            HttpContent content = new MyFormUrlEncodedContent(parameters);
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

    /// <summary>
    /// 默认的FormUrlEncodedContent碰到超长的文本会出现uri too long的异常，这里自己封装一个
    /// 参考来自 stackoverflow
    /// </summary>
    public class MyFormUrlEncodedContent : ByteArrayContent
    {
        public MyFormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
            : base(MyFormUrlEncodedContent.GetContentByteArray(nameValueCollection))
        {
            base.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
        }
        private static byte[] GetContentByteArray(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> current in nameValueCollection)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append('&');
                }

                stringBuilder.Append(MyFormUrlEncodedContent.Encode(current.Key));
                stringBuilder.Append('=');
                stringBuilder.Append(MyFormUrlEncodedContent.Encode(current.Value));
            }
            return Encoding.Default.GetBytes(stringBuilder.ToString());
        }

        private static string Encode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
            return WebUtility.UrlEncode(data);
        }
    }
}

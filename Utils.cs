using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Encoders;

namespace Com.Netease.Is.Antispam.Demo
{
    class Utils
    {
        // 单例 HttpClient 实例
        private static readonly HttpClient HttpClientInstance;

        static Utils()
        {
            // 配置 SocketsHttpHandler 参数
            SocketsHttpHandler httpHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(10), // 池中的连接最长存活时间
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2), // 池中的连接最大空闲时间
                MaxConnectionsPerServer = 256 // 每个服务器的最大连接数
            };

            // 创建单例 HttpClient 实例，并配置默认请求头
            HttpClientInstance = new HttpClient(httpHandler)
            {
                Timeout = TimeSpan.FromMilliseconds(10000) // 设置请求超时时间
            };
            HttpClientInstance.DefaultRequestHeaders.Connection.Add("keep-alive");
        }


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

        // 根据secretKey，signatureMethod和parameters生成签名
        public static String genSignature(String secretKey, String signatureMethod, Dictionary<String, String> parameters)
        {
            if(signatureMethod.ToUpper().Equals("SM3"))
            {
                // 国密SM3加密
                parameters = parameters.OrderBy(o => o.Key, StringComparer.Ordinal).ToDictionary(o => o.Key, p => p.Value);
                StringBuilder builder = new StringBuilder();
                foreach (KeyValuePair<String, String> kv in parameters)
                {
                    builder.Append(kv.Key).Append(kv.Value);
                }
                builder.Append(secretKey);
                byte[] tmp = Encoding.Default.GetBytes(builder.ToString());
                byte[] md = new byte[32];
                SM3Digest sm3 = new SM3Digest();
                sm3.BlockUpdate(tmp, 0, tmp.Length);
                sm3.DoFinal(md, 0);
                return new UTF8Encoding().GetString(Hex.Encode(md));
            }else
            {
                return genSignature(secretKey, parameters);
            }
        }

        public static HttpClient makeHttpClient()
        {
            HttpClientInstance.SendAsync(new HttpRequestMessage
            {
                Method = new HttpMethod("HEAD"),
                RequestUri = new Uri("http://as.dun.163.com")
            }).Wait();
            return HttpClientInstance;
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
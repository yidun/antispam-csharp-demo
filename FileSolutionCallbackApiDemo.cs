using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class FileSolutionCallbackApiDemo
    {
        public static void fileCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 易盾反垃圾云服务文档解决方案检测结果获取接口地址 */
            String apiUrl = "https://as-file.dun.163yun.com/v1/file/callback/results";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("version", "v1");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 3.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 10000);
            if(result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    JArray array = (JArray)ret.SelectToken("result");
                }
                else
                {
                    Console.WriteLine(String.Format("ERROR: code={0}, msg={1}", code, msg));
                }
            }
            else
            {
                Console.WriteLine("Request failed!");
            }
        }
    }
}

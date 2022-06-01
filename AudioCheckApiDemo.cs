using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class AudioCheckApiDemo
    {
        public static void audioCheck()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务短语音在线检测接口 */
            String apiUrl = "http://as.dun.163.com/v1/audio/check";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v1.0");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            parameters.Add("url", "http://xxx.xx");

            // 3.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 4.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 10000);
            if(result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    JObject resultObject = (JObject)ret["result"];
                    String taskId = resultObject["taskId"].ToObject<String>();
                    int status = resultObject["status"].ToObject<Int32>();
                    if (status == 0) {
                        Console.WriteLine(String.Format("CHECK SUCCESS! taskId={0}", taskId));
                        //反垃圾检测结果
                        JArray antispamArray = (JArray)resultObject.SelectToken("antispam");
                        //语种检测结果
                        JArray languageArray = (JArray)resultObject.SelectToken("language");
                        //语音识别检测结果
                        JArray asrArray = (JArray)resultObject.SelectToken("asr");
                        //人声识别检测结果
                        JArray voiceArray = (JArray)resultObject.SelectToken("voice");
                    } else if(status == 1) {
                        Console.WriteLine(String.Format("CHECK TIMEOUT! taskId={0}, status={1}", taskId, status));
                    }
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
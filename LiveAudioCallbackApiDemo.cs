using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveAudioCallbackApiDemo
    {
        public static void audioCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务直播音频离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163.com/v3/liveaudio/callback/results";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v3");
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
                    JObject resultObject = (JObject)ret["result"];
                    JArray antispamArray = (JArray)resultObject.SelectToken("antispam");
                    foreach (var item in antispamArray)
                    {
                        JObject jObject = (JObject)item;
                        String taskId = jObject.GetValue("taskId").ToObject<String>();
                        String callback = jObject.GetValue("callback").ToObject<String>();
                        String dataId = jObject.GetValue("dataId").ToObject<String>();
                        Console.WriteLine(String.Format("taskId:{0}, callback:{1}, dataId:{2}", taskId, callback, dataId));
                        // 机审结果
                        if( jObject["evidences"] != null ) { JObject evidences = (JObject) jObject.SelectToken("evidences"); }
                        // 人审结果
                        if( jObject["evidences"] != null ) { JObject reviewEvidences = (JObject) jObject.SelectToken("reviewEvidences"); }
                    }
                    JArray asrArray = (JArray)resultObject.SelectToken("asr");
                    foreach (var item in asrArray)
                    {
                        JObject jObject = (JObject)item;
                        String taskId = jObject.GetValue("taskId").ToObject<String>();
                        String content = jObject.GetValue("content").ToObject<String>();
                        long startTime = jObject.GetValue("startTime").ToObject<long>();
                        long endTime = jObject.GetValue("endTime").ToObject<long>();
                        Console.WriteLine(String.Format("taskId:{0}, content:{1}, startTime:{2}, endTime:{3}", taskId, content, startTime, endTime));
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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveVideoCallbackApiDemoV4
    {
        public static void liveVideoCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务直播离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163.com/v4/livevideo/callback/results";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v4");
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
                    foreach (var item in array)
                    {
                        JObject tmp = (JObject)item;
                        JObject antispam = tmp.GetValue("antispam").ToObject<JObject>();
                        String taskId = null == antispam["taskId"] ? "" : antispam.GetValue("taskId").ToObject<String>();
                        Console.WriteLine(String.Format("taskId:{0}, 反垃圾信息:{1}", taskId, antispam));
                        String callback = null == antispam["callback"] ? "" : antispam.GetValue("callback").ToObject<String>();
                        JObject evidenceObjec = (JObject)antispam.SelectToken("evidence");
                        JArray labels = (JArray)antispam.SelectToken("labels");
                        if (null == labels || labels.Count == 0)
                        {
                            Console.WriteLine(String.Format("正常, callback={0}, 证据信息: {1}", callback, evidenceObjec));
                        }
                        else
                        {
                            foreach (var labelObj in labels)
                            {
                                JObject tmp2 = (JObject)labelObj;
                                int label = tmp2.GetValue("label").ToObject<Int32>();
                                int level = tmp2.GetValue("level").ToObject<Int32>();
                                double rate = tmp2.GetValue("rate").ToObject<Double>();
                                Console.WriteLine(String.Format("异常, callback={0}, 分类：{1}, 证据信息：{2}", callback, label, evidenceObjec));
                            }
                        }
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

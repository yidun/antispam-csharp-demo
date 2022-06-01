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
                        String taskId = null == tmp["taskId"] ? "" : tmp.GetValue("taskId").ToObject<String>();
                        String callback = null == tmp["callback"] ? "" : tmp.GetValue("callback").ToObject<String>();
                        int callbackStatus = null == tmp["callbackStatus"] ? 0 : tmp.GetValue("callbackStatus").ToObject<int>();
                        int riskLevel = null == tmp["riskLevel"] ? 0 : tmp.GetValue("riskLevel").ToObject<int>();
                        int riskScore = null == tmp["riskScore"] ? 0 : tmp.GetValue("riskScore").ToObject<int>();
                        long duration = null == tmp["duration"] ? 0 : tmp.GetValue("duration").ToObject<long>();
                        Console.WriteLine(String.Format("taskId:{0}, 回调信息:{1}, 回调状态{2}, 风险等级{3}, 风险评分{4}, 时长 {5}", taskId, callback, callbackStatus, riskLevel, riskScore, duration));
                        JObject evidenceObjec = (JObject)tmp.SelectToken("evidence");
                        JArray labels = (JArray)tmp.SelectToken("labels");
                        if (null == labels || labels.Count == 0)
                        {
                            Console.WriteLine(String.Format("正常, callback={0}, 证据信息: {1}", callback, evidenceObjec.ToString()));
                        }
                        else
                        {
                            foreach (var labelObj in labels)
                            {
                                JObject tmp2 = (JObject)labelObj;
                                int label = tmp2.GetValue("label").ToObject<Int32>();
                                int level = tmp2.GetValue("level").ToObject<Int32>();
                                double rate = tmp2.GetValue("rate").ToObject<Double>();
                                Console.WriteLine(String.Format("异常, callback={0}, 分类：{1}, 证据信息：{2}", callback, label, evidenceObjec.ToString()));
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

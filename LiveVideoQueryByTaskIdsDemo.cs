using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveVideoQueryByTaskIdsDemo
    {
        public static void liveVideoQuery()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务直播音视频解决方案离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163.com/v1/livevideo/query/task";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v1");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            JArray jarray = new JArray();
            jarray.Add("liz9lagl8ymv3tj0h3537l8g00409r6s");
            parameters.Add("taskIds", jarray.ToString());

            // 3.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 4.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String resultResponse = Utils.doPost(client, apiUrl, parameters, 10000);
            if(resultResponse != null)
            {
                JObject ret = JObject.Parse(resultResponse);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    JArray array = (JArray)ret.SelectToken("result");
                    if(null == array){
                        Console.WriteLine("没有结果");
                    }else {
                        foreach (var item in array)
                        {
                            JObject tmp = (JObject)item;
                            int status = tmp.GetValue("status").ToObject<Int32>();
                            String taskId = tmp.GetValue("taskId").ToObject<String>();
                            String callback = null == tmp["callback"] ? "" : tmp.GetValue("callback").ToObject<String>();
                            int callbackStatus = null == tmp["callback"] ? 0 : tmp.GetValue("callbackStatus").ToObject<int>();
                            int expireStatus = null == tmp["callback"] ? 0 : tmp.GetValue("expireStatus").ToObject<int>();
                            Console.WriteLine(String.Format("taskId={0}, status={1}, callback={2}, callbackStatus={3}, expireStatus={4}", taskId, status, callback, callbackStatus, expireStatus));
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

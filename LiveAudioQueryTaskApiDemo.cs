using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveAudioQueryTaskApiDemo
    {
        public static void audioCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 调用易盾反垃圾云服务查询直播语音片段离线结果接口API示例 */
            String apiUrl = "http://as-liveaudio.dun.163.com/v1/liveaudio/query/task";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v1.0");
            parameters.Add("taskId", "xxx");
            parameters.Add("startTime", "1578326400000");
            parameters.Add("endTime", "1578327000000");//10min limit
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
                        JObject jObject = (JObject)item;
                        int asrStatus = jObject.GetValue("asrStatus").ToObject<Int32>();
                        String taskId = jObject.GetValue("taskId").ToObject<String>();
                        int action = jObject.GetValue("action").ToObject<Int32>();
                        long startTime = jObject.GetValue("startTime").ToObject<Int64>();
                        long endTime = jObject.GetValue("endTime").ToObject<Int64>();
                        // 证据信息
                        JArray segmentArray = (JArray)jObject.SelectToken("segments");
                        if (action == 0) {
                            Console.WriteLine(String.Format("结果：通过!taskId={0},startTime={1},endTime={2}", taskId, startTime, endTime));
                        } else if (action == 1 || action == 2) {
                            Console.WriteLine(String.Format("taskId={0},结果：{1},startTime={2},endTime={3}", taskId, action == 1 ? "不确定" : "不通过", startTime, endTime));
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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveVideoSolutionFeedbackApiDemo
    {
        public static void liveVideoFeedback()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 易盾反垃圾云服务直播音视频解决方案离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163yun.com/v1/livewallsolution/feedback";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("version", "v1");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            JArray jarray = new JArray();
            JObject realTimeInfo = new JObject();
            realTimeInfo.Add("taskId", "c5ce8cb3888c46868ff5c69cd3a8e00d");
            realTimeInfo.Add("status", 100);
            jarray.Add(realTimeInfo);
            parameters.Add("realTimeInfoList", jarray.ToString());

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
                String msg = ret.GetValue("message").ToObject<String>();
                if (code == 200)
                {
                    JArray array = (JArray)ret.SelectToken("result");
                    foreach (var item in array)
                    {
                        JObject tmp = (JObject)item;
                        int r = tmp.GetValue("result").ToObject<Int32>();
                        String taskId = tmp.GetValue("taskId").ToObject<String>();
                        switch (r) {
                            case 0:
                                Console.WriteLine(String.Format("SUCCESS, taskId={0}" + taskId));
                                break;
                            case 1:
                                Console.WriteLine(String.Format("SERVER ERROR, taskId={0}" + taskId));
                                break;
                            case 2:
                                Console.WriteLine(String.Format("NOT EXISTS, taskId={0}" + taskId));
                                break;
                            default:
                                break;
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

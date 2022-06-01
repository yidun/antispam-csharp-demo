using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveVideoSolutionQueryAudioApiDemo
    {
        public static void liveVideoQuery()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 易盾反垃圾云服务直播音视频解决方案离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163yun.com/v1/livewallsolution/query/audio/task";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("version", "v1.0");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            parameters.Add("taskId", "292604e0200b4551b411c2d53adde893");
            parameters.Add("startTime", (curr - 10 * 60 * 1000).ToString());
            parameters.Add("endTime", time);// 最长支持查10分钟跨度

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
                            int action = tmp.GetValue("action").ToObject<Int32>();
                            String taskId = tmp.GetValue("taskId").ToObject<String>();
                            long startTime = tmp.GetValue("startTime").ToObject<long>();
                            long endTime = tmp.GetValue("endTime").ToObject<long>();
                            JArray segments = (JArray)tmp.SelectToken("segments");
                            if(action == 0){
                                Console.WriteLine(String.Format("taskId={0}，结果：通过，时间区间【{1}-{2}】，证据信息如下：{3}", taskId, startTime, endTime, segments));
                            }else if(action == 1 || action == 2){
                                Console.WriteLine(String.Format("taskId={0}，结果：{1}，时间区间【{2}-{3}】，证据信息如下：{4}", taskId, action == 1 ? "不确定" : "不通过", startTime, endTime, segments));
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

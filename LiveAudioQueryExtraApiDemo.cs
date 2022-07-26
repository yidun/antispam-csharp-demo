using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveAudioQueryExtraApiDemo
    {
        public static void audioQueryExtra()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 调用易盾反垃圾云服务直播语音查询直播语音增值检测结果地址 */
            String apiUrl = "http://as.dun.163.com/v1/liveaudio/query/extra";
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
            parameters.Add("taskId", "xxx");

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
                    JArray asrs = (JArray)resultObject.SelectToken("asr");
                    JArray languages = (JArray)resultObject.SelectToken("language");
                    if(asrs != null && asrs.Count > 0){
                        foreach (var item in asrs)
                        {
                            JObject asr = (JObject)item;
                            String taskId = asr.GetValue("taskId").ToObject<String>();
                            String content = asr.GetValue("content").ToObject<String>();
                            long startTime = asr.GetValue("startTime").ToObject<long>();
                            long endTime = asr.GetValue("endTime").ToObject<long>();
                            String speakerId = asr.GetValue("speakerId").ToObject<String>();
                            Console.WriteLine(String.Format("语音识别检测结果：taskId={0}, speakerId={1} content={2}, startTime={3}, endTime={4}",
                                    taskId, speakerId, content, startTime, endTime));
                        }
                    }else {
                        Console.WriteLine("无语音识别检测结果");
                    }
                    if(languages != null && languages.Count > 0){
                        foreach (var item in languages)
                        {
                            JObject language = (JObject)item;
                            String taskId = language.GetValue("taskId").ToObject<String>();
                            String content = language.GetValue("content").ToObject<String>();
                            String callback = language.GetValue("callback").ToObject<String>();
                            String segmentId = language.GetValue("segmentId").ToObject<String>();
                            long startTime = language.GetValue("startTime").ToObject<long>();
                            long endTime = language.GetValue("endTime").ToObject<long>();
                            String speakerId = language.GetValue("speakerId").ToObject<String>();
                            Console.WriteLine(String.Format("语种检测结果：taskId={0}, content={1} callback={2}, segmentId={3}, startTime={4}, endTime={5}",
                                    taskId, content, callback, segmentId, startTime, endTime));
                        }
                    }else {
                        Console.WriteLine("无语种识别检测结果");
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

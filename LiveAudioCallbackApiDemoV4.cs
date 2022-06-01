using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveAudioCallbackApiDemoV4
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
            String apiUrl = "http://as.dun.163.com/v4/liveaudio/callback/results";
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
                    JArray resultArray = (JArray)ret.SelectToken("result");
                    if(null == resultArray){
                        Console.WriteLine("暂时没有结果需要获取，请稍后重试！");
                    }else {
                        foreach (var item in resultArray){
                            JObject resultObj = (JObject)item;
                            if(null != resultObj["antispam"]){
                                JObject antispam = resultObj.GetValue("antispam").ToObject<JObject>();
                                String taskId = antispam.GetValue("taskId").ToObject<String>();
                                int status = antispam.GetValue("status").ToObject<int>();
                                Console.WriteLine(String.Format("taskId:{0}, status:{1}", taskId, status));
                                if(null != antispam["evidences"]){
                                    JObject evidences = antispam.GetValue("evidences").ToObject<JObject>();
                                    Console.WriteLine(String.Format("evidences:{0}", evidences));
                                }
                                if(null != antispam["reviewEvidences"]){
                                    JObject reviewEvidences = antispam.GetValue("reviewEvidences").ToObject<JObject>();
                                    Console.WriteLine(String.Format("reviewEvidences:{0}", reviewEvidences));
                                }
                            }
                            if(null != resultObj["asr"]){
                                JObject asr = resultObj.GetValue("asr").ToObject<JObject>();
                                String taskId = asr.GetValue("taskId").ToObject<String>();
                                String content = asr.GetValue("content").ToObject<String>();
                                long startTime = asr.GetValue("startTime").ToObject<long>();
                                long endTime = asr.GetValue("endTime").ToObject<long>();
                                Console.WriteLine(String.Format("taskId:{0}, content:{1}, startTime:%s, endTime:%s", taskId, content, startTime, endTime));
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

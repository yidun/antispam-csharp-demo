using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class AudioCheckApiDemoV2
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
            String apiUrl = "http://as.dun.163.com/v2/audio/check";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v2");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            parameters.Add("url", "http://xxx.xxx.com/xxxx");

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
                     JObject resultObj = ret.GetValue("result").ToObject<JObject>();
                    if(null == resultObj){
                        Console.WriteLine("暂时没有结果需要获取，请稍后重试！");
                    }else {
                        if(null != resultObj["antispam"]){
                            JObject antispam = resultObj.GetValue("antispam").ToObject<JObject>();
                            String taskId = antispam.GetValue("taskId").ToObject<String>();
                            int status = antispam.GetValue("status").ToObject<int>();
                            if(status == 3){
                                int failureReason = antispam.GetValue("failureReason").ToObject<int>();
                                Console.WriteLine(String.Format("内容安全检测结果：检测失败，taskId = {0}，失败原因：{1}", taskId, failureReason));
                            }
                            if(status == 2){
                                Console.WriteLine(String.Format("内容安全检测结果：检测成功, taskId = {0}，反垃圾结果: {1}", taskId, antispam));
                            }
                        }
                        if(null != resultObj["language"]){
                            JObject language = resultObj.GetValue("language").ToObject<JObject>();
                            String taskId = language.GetValue("taskId").ToObject<String>();
                            String dataId = null == language["dataId"] ? "" : language.GetValue("dataId").ToObject<String>();
                            String callback = null == language["callback"] ? "" : language.GetValue("callback").ToObject<String>();
                            JArray details = (JArray)language.SelectToken("details");
                            Console.WriteLine(String.Format("语种检测结果：taskId = {0}, dataId = {1}, callback = {2}, 语种检测详情如下: {3}", taskId, dataId, callback, details));
                        }
                        if(null != resultObj["asr"]){
                            JObject asr = resultObj.GetValue("asr").ToObject<JObject>();
                            String taskId = asr.GetValue("taskId").ToObject<String>();
                            String dataId = null == asr["dataId"] ? "" : asr.GetValue("dataId").ToObject<String>();
                            String callback = null == asr["callback"] ? "" : asr.GetValue("callback").ToObject<String>();
                            JArray details = (JArray)asr.SelectToken("details");
                            Console.WriteLine(String.Format("语音识别检测结果：taskId = {0}, dataId = {1}, callback = {2}, 语种检测详情如下: {3}", taskId, dataId, callback, details));
                        }
                        if(null != resultObj["voice"]){
                            JObject voice = resultObj.GetValue("voice").ToObject<JObject>();
                            String taskId = voice.GetValue("taskId").ToObject<String>();
                            String dataId = null == voice["dataId"] ? "" : voice.GetValue("dataId").ToObject<String>();
                            String callback = null == voice["callback"] ? "" : voice.GetValue("callback").ToObject<String>();
                            JArray details = (JArray)voice.SelectToken("details");
                            Console.WriteLine(String.Format("人声检测属性结果：taskId = {0}, dataId = {1}, callback = {2}, 语种检测详情如下: {3}", taskId, dataId, callback, details));
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
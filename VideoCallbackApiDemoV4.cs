using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class VideoCallbackApiDemoV4
    {
        public static void videoCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务视频离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163.com/v4/video/callback/results";
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
                    if(null == array){
                        Console.WriteLine("暂无回调数据");
                    }else {
                        foreach (var item in array)
                        {
                            JObject tmp = (JObject)item;
                            JObject antispam = tmp.GetValue("antispam").ToObject<JObject>();
                            int status = antispam.GetValue("status").ToObject<int>();
                            if(status!=2){//异常，异常码定义见官网文档
                                int failureReason = antispam.GetValue("failureReason").ToObject<int>();
                                Console.WriteLine(String.Format("视频检测失败，status={0}, 失败类型={1}", status, failureReason));
                                continue;
                            }
                            String taskId = antispam.GetValue("taskId").ToObject<String>();
                            int suggestion = antispam.GetValue("suggestion").ToObject<int>();
                            int resultType = antispam.GetValue("resultType").ToObject<int>();
                            String callback = null == antispam["callback"] ? "" : antispam.GetValue("callback").ToObject<String>();
                            int censorSource = null == antispam["censorSource"] ? 0 : antispam.GetValue("censorSource").ToObject<int>();
                            String censorLabels = null == antispam["censorLabels"] ? "" : antispam.GetValue("censorLabels").ToObject<String>();
                            long censorTime = null == antispam["censorTime"] ? 0 : antispam.GetValue("censorTime").ToObject<long>();
                            Console.WriteLine(String.Format("检测成功，taskId={0}, 嫌疑类型 {1}，结果类型 {2}，回调信息 {3}， 审核来源 {4}， 人审时长 {5}， 分类标签 {6}", taskId, suggestion, resultType, callback, censorSource, censorTime, censorLabels));
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

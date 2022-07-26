using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class VideoSolutionCallbackApiDemo
    {
        public static void videoCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 易盾反垃圾云服务点播音视频解决方案离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163.com/v2/videosolution/callback/results";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("version", "v2");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 3.发送HTTP请求
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
                    foreach (var item in array)
                    {
                        JObject tmp = (JObject)item;
                        String callback = tmp.GetValue("callback").ToObject<String>();
                        String taskId = tmp.GetValue("taskId").ToObject<String>();
                        String dataId = tmp.GetValue("dataId").ToObject<String>();
                        int result = null == tmp["result"] ? 0 : tmp.GetValue("result").ToObject<int>();
                        int censorSource = null == tmp["censorSource"] ? 0 : tmp.GetValue("censorSource").ToObject<int>();
                        int checkStatus = null == tmp["checkStatus"] ? 0 : tmp.GetValue("checkStatus").ToObject<int>();
                        long checkTime = null == tmp["checkTime"] ? 0 : tmp.GetValue("checkTime").ToObject<long>();
                        long duration = null == tmp["duration"] ? 0 : tmp.GetValue("duration").ToObject<long>();
                        long receiveTime = null == tmp["receiveTime"] ? 0 : tmp.GetValue("receiveTime").ToObject<long>();
                        long censorTime = null == tmp["censorTime"] ? 0 : tmp.GetValue("censorTime").ToObject<long>();
                        JObject evidences = null == tmp["evidences"] ? null : tmp.GetValue("evidences").ToObject<JObject>();
                        JObject reviewEvidences = null == tmp["reviewEvidences"] ? null : tmp.GetValue("reviewEvidences").ToObject<JObject>();
                        Console.WriteLine(String.Format("taskId={0}：dataId={1}：result={2}：callback={3}：censorSource={4}：checkStatus={5}：checkTime={6}：duration={7}：receiveTime={8}：censorTime={9}, evidences={10}, reviewEvidences={11}", taskId, dataId, result, callback, censorSource, checkStatus, checkTime, duration, receiveTime, censorTime, evidences, reviewEvidences));
                        
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

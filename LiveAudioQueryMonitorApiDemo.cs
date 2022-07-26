using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveAudioQueryMonitorApiDemo
    {
        public static void audioQueryMonitor()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 调用易盾反垃圾云服务直播语音查询人审操作记录接口地址*/
            String apiUrl = "http://as.dun.163.com/v1/liveaudio/query/monitor";
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
                    int status = (int)resultObject["status"];
                    if(status == 0)
                    {
                        JArray monitors = (JArray)resultObject.SelectToken("monitors");
                        foreach(var item in monitors)
                        {
                            JObject monitor = (JObject) item;
                            int action = (int) monitor["action"];
                            long actionTime = (long) monitor["actionTime"];
                            int spamType = (int) monitor["spamType"];
                            String spamDetail = (String) monitor["spamDetail"];
                        }
                        Console.WriteLine(String.Format("直播人审结果：{0}", monitors.ToString()));
                    }else if(status == 20)
                    {
                        Console.WriteLine("数据过期");
                    }else if(status == 30)
                    {
                        Console.WriteLine("数据不存在");
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
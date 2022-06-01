using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveWallQueryMonitorApiDemo
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
            String apiUrl = "https://as.dun.163.com/v1/livewall/query/monitor";
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
            parameters.Add("taskId", "49db4e9dc56a424bb720fb46071532b4");

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
                    JObject result = ret.GetValue("result").ToObject<JObject>();
                    if(null == result){
                        Console.WriteLine("没有结果");
                    }else {
                        int status = result.GetValue("status").ToObject<Int32>();                    
                        if(status == 0){
                            JArray records = (JArray)result.SelectToken("records");
                            foreach (var item in records){
                                JObject record = (JObject)item;
                                int label = record.GetValue("label").ToObject<Int32>();
                                int action = record.GetValue("action").ToObject<Int32>();
                                long actionTime = record.GetValue("actionTime").ToObject<long>();
                                String detail = record.GetValue("detail").ToObject<String>();
                            }
                            Console.WriteLine(String.Format("直播人审结果：{0}", records));
                        }else if(status == 20){
                            Console.WriteLine("taskId is expired");
                        }else if(status == 30){
                            Console.WriteLine("taskId is not exist");
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

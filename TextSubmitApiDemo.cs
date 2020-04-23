using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class TextSubmitApiDemo
    {
        public static void textSubmit()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务文本数据抄送接口地址 */
            String apiUrl = "http://as.dun.163yun.com/v1/text/submit";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v1");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            JArray jarray = new JArray();
            JObject text1 = new JObject();
            text1.Add("dataId", "dataId1");
            // action: 0 通过 1 嫌疑 2 确定删除
            text1.Add("action", 1);
            text1.Add("content", "content嫌疑内容");
            jarray.Add(text1);

            JObject text2 = new JObject();
            text2.Add("dataId", "dataId2");
            text2.Add("action", 2);
            text2.Add("content", "content不通过内容");
            jarray.Add(text2);

            parameters.Add("texts", jarray.ToString());

            // 3.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 4.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 1000);
            if(result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    JArray resultArray = (JArray)ret.SelectToken("result");
                    foreach (var item in resultArray)
                    {
                        JObject tmp = (JObject)item;
                        String dataId = tmp.GetValue("dataId").ToObject<String>();
                        String taskId = tmp.GetValue("taskId").ToObject<String>();
                        Console.WriteLine(String.Format("文本提交返回:taskId={0},dataId={1}", taskId, dataId));
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

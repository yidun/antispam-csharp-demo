using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class TextBatchCheckApiDemo
    {
        public static void textBatchCheck()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务文本在线检测接口地址 */
            String apiUrl = "http://as.dun.163.com/v3/text/batch-check";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v3");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            JArray texts = new JArray();
            JObject text = new JObject();
            text.Add("dataId", "dataId1");
            text.Add("content", "易盾测试内容1");
            // text.Add("dataType", "1");
            // text.Add("ip", "123.115.77.137");
            // text.Add("account", "csharp@163.com");
            // text.Add("deviceType", "4");
            // text.Add("deviceId", "92B1E5AA-4C3D-4565-A8C2-86E297055088");
            // text.Add("callback", "ebfcad1c-dba1-490c-b4de-e784c2691768");
            // text.Add("callbackUrl", "主动回调url地址");
            texts.Add(text);
            JObject text2 = new JObject();
            text2.Add("dataId", "dataId2");
            text2.Add("content", "易盾测试内容2");
            texts.Add(text2);
            parameters.Add("texts", texts.ToString());

            // 3.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 4.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 1000);
            Console.WriteLine(result);
            if(result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    JArray array = (JArray)ret.SelectToken("result");
                    foreach (var item in array)
                    {
                        JObject resultObject = (JObject)item;
                        String taskId = resultObject["taskId"].ToObject<String>();
                        int action = resultObject["action"].ToObject<Int32>();
                        JArray labelArray = (JArray)resultObject.SelectToken("labels");
                        if (action == 0)
                        {
                            Console.WriteLine(String.Format("taskId={0}，文本机器检测结果：通过", taskId));
                        }
                        else if (action == 1)
                        {
                            Console.WriteLine(String.Format("taskId={0}，文本机器检测结果：嫌疑，需人工复审，分类信息如下：{1}", taskId, labelArray));
                        }
                        else if (action == 2)
                        {
                            Console.WriteLine(String.Format("taskId={0}，文本机器检测结果：不通过，分类信息如下：{1}", taskId, labelArray));
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

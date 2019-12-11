using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class ImageSubmitApiDemo
    {
        public static void imageSubmit()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务图片数据抄送接口地址 */
            String apiUrl = "https://as.dun.163yun.com/v1/image/submit";
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
            JObject image1 = new JObject();
            image1.Add("name", "name1");
            // level: 0 通过 1 嫌疑 2 确定删除
            image1.Add("level", 1);
            image1.Add("data", "https://nos.netease.com/yidun/2-0-0-a6133509763d4d6eac881a58f1791976.jpg");
            jarray.Add(image1);

            JObject image2 = new JObject();
            image2.Add("name", "name2");
            image2.Add("level", 2);
            image2.Add("data", "http://dun.163.com/public/res/web/case/sexy_normal_2.jpg?dda0e793c500818028fc14f20f6b492a");
            jarray.Add(image2);

            parameters.Add("images", jarray.ToString());

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
                        String name = tmp.GetValue("name").ToObject<String>();
                        String taskId = tmp.GetValue("taskId").ToObject<String>();
                        Console.WriteLine(String.Format("图片提交返回:taskId={0},name={1}", taskId, name));
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

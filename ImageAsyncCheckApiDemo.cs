using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class ImageAsyncCheckApiDemo
    {
        public static void imageCheck()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务图片在线检测接口地址 */
            String apiUrl = "http://as.dun.163.com/v4/image/asyncCheck";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v4");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            JArray jarray = new JArray();
            JObject image1 = new JObject();
            image1.Add("name", "https://nos.netease.com/yidun/2-0-0-a6133509763d4d6eac881a58f1791976.jpg");
            image1.Add("type", 1);
            image1.Add("data", "https://nos.netease.com/yidun/2-0-0-a6133509763d4d6eac881a58f1791976.jpg");
            jarray.Add(image1);

            parameters.Add("images", jarray.ToString());
            // parameters.Add("account", "csharp@163.com");
            // parameters.Add("ip", "123.115.77.137");

            // 3.生成签名信息,指定国密SM3加密
            //parameters.Add("signatureMethod", "SM3");
            String signature = Utils.genSignature(secretKey, parameters.GetValueOrDefault("signatureMethod", "MD5"), parameters);
            parameters.Add("signature", signature);

            // 4.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 10000);
            if(result != null){
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200){
                    JObject resultObj = ret.GetValue("result").ToObject<JObject>();
                    JArray resultArray = (JArray)resultObj.SelectToken("checkImages");
                    foreach (var item in resultArray)
                    {
                        JObject tmp = (JObject)item;
                        String name = null == tmp.GetValue("name") ? "" : tmp.GetValue("name").ToObject<String>();
                        String taskId = tmp.GetValue("taskId").ToObject<String>();
                        Console.WriteLine(String.Format("name={0},taskId={1}", name, taskId));
                    }
                }else {
                    Console.WriteLine(String.Format("ERROR: code={0}, msg={1}", code, msg));
                }
            }else {
                Console.WriteLine("Request failed!");        
            }
        }
    }
}
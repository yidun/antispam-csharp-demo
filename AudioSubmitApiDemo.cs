using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class AudioSubmitApiDemo
    {

        public static void audioSubmit()
        {     
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务音频信息提交接口地址  */
            String apiUrl = "http://as.dun.163.com/v4/audio/submit";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            // 点播语音版本v3.2及以上二级细分类结构进行调整
            parameters.Add("version", "v4");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            parameters.Add("url", "http://xxx.xxx.com/xxxx");

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
                    JObject resultObject = (JObject)ret["result"];
                    String taskId = resultObject["taskId"].ToObject<String>();
                    int status = resultObject["status"].ToObject<Int32>();
                    int dealingCount = resultObject["dealingCount"].ToObject<Int32>();
                    if (status == 0) {
                        Console.WriteLine(String.Format("推送成功!taskId={0},dealingCount={1}", taskId, dealingCount));
                    } else {
                        Console.WriteLine(String.Format("推送失败!taskId={0},dealingCount={1}", taskId, dealingCount));
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

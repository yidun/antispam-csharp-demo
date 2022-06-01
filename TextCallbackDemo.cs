using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class TextCallbackDemo
    {

        public static void textCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务文本离线检测结果获取接口地址 */
            String apiUrl = "http://as.dun.163.com/v3/text/callback/results";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v3.1");
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
                    if (array==null || !array.HasValues)
                    {
                        Console.WriteLine("暂时没有人工复审结果需要获取，请稍后重试！");
                    }
                    
                    foreach (var item in array)
                    {
                        JObject tmp = (JObject)item;
                        int action = tmp.GetValue("action").ToObject<Int32>();
                        String taskId = tmp["taskId"].ToObject<String>();
                        String callback = tmp.GetValue("callback").ToObject<String>();
                        JArray labelArray = (JArray)tmp.SelectToken("labels");
                        if (action == 0)
                        {
			                 Console.WriteLine(String.Format("taskId={0}，callback={1}，文本人工复审结果：通过", taskId,callback));
                        }
                        else if (action == 2)
                        {
			                 Console.WriteLine(String.Format("taskId={0}，callback={1}，文本人工复审结果：不通过，分类信息如下：{2}", taskId,callback,labelArray));
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

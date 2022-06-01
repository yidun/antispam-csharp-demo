using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class FileSolutionCallbackApiDemoV2
    {
        public static void fileCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 易盾反垃圾云服务文档解决方案检测结果获取接口地址 */
            String apiUrl = "http://as-file.dun.163.com/v2/file/callback/results";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("version", "v2.0");
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
                    if(null == array){
                        Console.WriteLine("Can't find Callback Data");
                    }else {
                        foreach (var item in array)
                        {
                            JObject jObject = (JObject)item;
                            JObject antispam = jObject.GetValue("antispam").ToObject<JObject>();
                            String taskId = antispam.GetValue("taskId").ToObject<String>();
                            String dataId = antispam.GetValue("dataId").ToObject<String>();
                            int result = antispam.GetValue("suggestion").ToObject<Int32>();
                            String callback = null == antispam["callback"] ? "" : antispam.GetValue("callback").ToObject<String>();
                            JObject evidencesObject = antispam.GetValue("evidences").ToObject<JObject>();
                             Console.WriteLine(String.Format("SUCCESS: dataId={0}, taskId={1}, result={2}, callback={3}, evidences={4}",dataId, taskId, result, callback, evidencesObject));
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class DigitalBookQueryV2ApiDemo
    {
        public static void digitalBookQuery()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 易盾反垃圾云服务举报解决方案离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163.com/v2/digital/callback/query";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("version", "v2");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            parameters.Add("taskIds", "['bz29tn9yu944hjzmdg57dg3g05009r28']");

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
                        Console.WriteLine("暂时没有结果需要获取，请稍后重试");
                    }else {
                        foreach (var item in array)
                        {
                            JObject tmp = (JObject)item;
                            if(null != tmp["antispam"]){
                                JObject antispam = tmp.GetValue("antispam").ToObject<JObject>();
                                Console.WriteLine(String.Format("机器检测结果: {0}", antispam));
                            }
                            if(null != tmp["valueAddService"]){
                                JObject valueAddService = tmp.GetValue("valueAddService").ToObject<JObject>();
                                Console.WriteLine(String.Format("增值服务结果: {0}", valueAddService));
                            }
                            if(null != tmp["anticheat"]){
                                JObject anticheat = tmp.GetValue("anticheat").ToObject<JObject>();
                                Console.WriteLine(String.Format("反作弊结果: {0}", anticheat));
                            }
                            if(null != tmp["censor"]){
                                JObject censor = tmp.GetValue("censor").ToObject<JObject>();
                                Console.WriteLine(String.Format("人工审核结果: {0}", censor));
                            }
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

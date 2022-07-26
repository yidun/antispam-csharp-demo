using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class CrawlerJobSubmitApiDemo
    {
        public static void crawlerJobSubmit()
        {     
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 易盾反垃圾网站检测解决方案提交接口地址  */
            String apiUrl = "http://as.dun.163.com/v1/crawler/job/submit";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("version", "v1.0");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            parameters.Add("dataId", "ebfcad1c-dba1-490c-b4de-e784c2691768");
            // 主站URL
            parameters.Add("siteUrl", "http://xxx.com");
            // 爬虫深度/网站层级
            parameters.Add("level", "3");
            // 单次任务周期内爬取页面的最大数量
            parameters.Add("maxResourceAmount", "1000");
            // 任务类型
            parameters.Add("type", "1");
            // 回调接口地址
            parameters.Add("callbackUrl", "主动将结果推送给调用方的接口");

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
                    String jobId = resultObject["jobId"].ToObject<String>();
                    String dataId = resultObject["dataId"].ToObject<String>();
                    Console.WriteLine(String.Format("SUCCESS: jobId={0}, dataId={1}", jobId, dataId));
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

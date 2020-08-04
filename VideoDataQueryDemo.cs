using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class VideoDataQueryDemo
    {
        public static void videoDataQuery()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务点播截图查询接口地址 */
            String apiUrl = "http://as.dun.163.com/v1/video/query/image";
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
            parameters.Add("taskId", "c633a8cb6d45497c9f4e7bd6d8218443");
            // 图片级别：1 嫌疑 2 确定删除
            parameters.Add("levels", "[1,2]");
            // 回调状态，1 待回调
            parameters.Add("callbackStatus", "1");
            parameters.Add("pageNum", "1");
            parameters.Add("pageSize", "10");
            // 详情查看官网VideoDataOderType
            parameters.Add("orderType", "3");

            // 3.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 4.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 10000);
            if (result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    // 视频状态
                    int status = ret.GetValue("status").ToObject<Int32>();
                    JObject images = (JObject)ret.SelectToken("images");
                    // 截图总数
                    int count = images.GetValue("count").ToObject<Int32>();
                    // 截图详情
                    JArray rows = (JArray)images.SelectToken("rows");
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

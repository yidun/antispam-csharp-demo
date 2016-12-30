using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class ImageQueryByTaskIdsDemo
    {
        public static void imageQueryByTaskIds()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务图片离线检测结果获取接口地址 */
            String apiUrl = "https://api.aq.163.com/v1/image/query/task";
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
            ISet<String> taskIds = new HashSet<String>();
            taskIds.Add("3898f9e189404ea98fb20e77d11b69e3");
            taskIds.Add("3f343b8947a24a6987cba8ef5ea6534f");
            parameters.Add("taskIds", JArray.FromObject(taskIds).ToString());

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
                    JArray array = (JArray)ret.SelectToken("result");
                    foreach (var item in array)
                    {
                        JObject tmp = (JObject)item;
                        String name =tmp.GetValue("name")!=null?tmp.GetValue("name").ToObject<String>():"";
                        String taskId = tmp.GetValue("taskId").ToObject<String>();
                        JArray labels = (JArray)tmp.SelectToken("labels");
                        Console.WriteLine(String.Format("taskId={0}，name={1}，labels：", taskId, name));
                        int maxLevel = -1;
                        // 产品需根据自身需求，自行解析处理，本示例只是简单判断分类级别
                        foreach (var lable in labels)
                        {
                            JObject lableData = (JObject)lable;
                            int label = lableData.GetValue("label").ToObject<Int32>();
                            int level = lableData.GetValue("level").ToObject<Int32>();
                            double rate = lableData.GetValue("rate").ToObject<Double>();
                            Console.WriteLine(String.Format("label:{0}, level={1}, rate={2}", label, level, rate));
                            maxLevel = level > maxLevel ? level : maxLevel;
                        }

                        switch (maxLevel)
                        {
                            case 0:
                                Console.WriteLine("#图片查询结果：最高等级为\"正常\"\n");
                                break;
                            case 2:
                                Console.WriteLine("#图片查询结果：最高等级为\"确定\"\n");
                                break;
                            default:
                                break;
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

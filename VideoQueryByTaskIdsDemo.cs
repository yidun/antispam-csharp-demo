using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class VideoQueryByTaskIdsDemo
    {
        public static void videoQueryByTaskIds()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务点播查询检测结果获取接口地址 */
            String apiUrl = "http://as.dun.163yun.com/v1/video/query/task";
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
                        int status = tmp.GetValue("status").ToObject<Int32>();
                        if (status != 0)
                        {//-1:提交检测失败，0:正常，10：检测中，20：不是7天内数据，30：taskId不存在，110：请求重复，120：参数错误，130：解析错误，140：数据类型错误
                            Console.WriteLine("获取结果异常，status={0}",status);
                            continue;
                        }
                        String taskId = tmp.GetValue("taskId").ToObject<String>();
                        String callback = tmp.GetValue("callback").ToObject<String>();
                        int videoLevel  = tmp.GetValue("level").ToObject<Int32>();
                        if (videoLevel==0)
                        {
                            Console.WriteLine("正常,callback={0}", callback);
                        }
                        else if (videoLevel == 1 || videoLevel == 2)
                        {
                            JArray evidenceArray = (JArray)tmp.SelectToken("evidences");
                            foreach (var evidenceElement in evidenceArray)
                            {
                                JObject eObject = (JObject)evidenceElement;
                                long beginTime = eObject.GetValue("beginTime").ToObject<Int64>();
                                long endTime = eObject.GetValue("endTime").ToObject<Int64>();
                                Int32 type = eObject.GetValue("type").ToObject<Int32>();
                                String url = eObject.GetValue("url").ToObject<String>();
                                JArray labelArray = (JArray)eObject.SelectToken("labels");
                                foreach (var lable in labelArray)
                                {
                                    JObject lableData = (JObject)lable;
                                    int label = lableData.GetValue("label").ToObject<Int32>();
                                    int level = lableData.GetValue("level").ToObject<Int32>();
                                    double rate = lableData.GetValue("rate").ToObject<Double>();
                                }
                                Console.WriteLine("{0}, callback={1}, 证据信息：{2}, 证据分类：{03}, ", videoLevel == 1 ? "不确定"
                                : "确定", callback, eObject, labelArray);
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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class TextQueryByTaskIdsDemo
    {

        public static void textQueryByTaskIds()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务文本结果查询接口地址 */
            String apiUrl = "http://as.dun.163.com/v1/text/query/task";
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
            ISet<String> taskIds=new HashSet<String>();
            taskIds.Add("ecac3bc976674c36bfc5c06445243306");
            taskIds.Add("9fb210fa19a343f69b7e287912fa1ba6");
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
                    Console.WriteLine(array);

                    foreach (var item in array)
                    {
                        JObject tmp = (JObject)item;
                        int action = tmp.GetValue("action").ToObject<Int32>();
                        String taskId = tmp.GetValue("taskId").ToObject<String>();
                        int status = tmp.GetValue("status").ToObject<Int32>();
                        String callback = tmp.GetValue("callback")!=null?tmp.GetValue("callback").ToObject<String>():"";
                        JArray labelArray = (JArray)tmp.SelectToken("labels");
                        if (action == 0)
                        {
                            Console.WriteLine(String.Format("taskId={0}，status={1},callback={2}，文本查询结果：通过", taskId, status, callback));
                        }
                        else if (action == 2)
                        {
                            Console.WriteLine(String.Format("taskId={0}，status={1},callback={2}，文本查询结果：不通过，分类信息如下：{3}", taskId, status, callback, labelArray));
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

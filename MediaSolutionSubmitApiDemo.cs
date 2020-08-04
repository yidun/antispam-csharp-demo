using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class MediaSolutionSubmitApiDemo
    {
        public static void mediaSolutionSubmit()
        {     
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 易盾反垃圾融媒体解决方案在线检测接口地址  */
            String apiUrl = "http://as.dun.163.com/v1/mediasolution/submit";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("version", "v1");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            parameters.Add("title", "融媒体解决方案的标题");
            JArray jarray = new JArray();
            JObject text = new JObject();
            text.Add("type", "text");
            text.Add("data", "融媒体文本段落");
            jarray.Add(text);
            JObject image = new JObject();
            image.Add("type", "image");
            image.Add("data", "http://xxx");
            jarray.Add(image);
            JObject audio = new JObject();
            audio.Add("type", "audio");
            audio.Add("data", "http://xxx");
            jarray.Add(audio);
            JObject video = new JObject();
            video.Add("type", "video");
            video.Add("data", "http://xxx");
            jarray.Add(video);
            JObject audiovideo = new JObject();
            audiovideo.Add("type", "audiovideo");
            audiovideo.Add("data", "http://xxx");
            jarray.Add(audiovideo);
            JObject file = new JObject();
            file.Add("type", "file");
            file.Add("data", "http://xxx");
            jarray.Add(file);
            parameters.Add("content", jarray.ToString());
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
                    String dataId = resultObject["dataId"].ToObject<String>();
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

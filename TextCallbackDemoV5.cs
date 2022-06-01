using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class TextCallbackDemoV5
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
            String apiUrl = "http://as.dun.163.com/v5/text/callback/results";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v5");
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
                    }else {
                        int resultType = 1;
                        foreach (var item in array)
                        {
                            JObject resultObject = (JObject)item;
                            if(null != resultObject["antispam"]){
                                JObject antispam = resultObject["antispam"].ToObject<JObject>();
                                if(null != antispam){
                                    String taskId = antispam["taskId"].ToObject<String>();
                                    String dataId = antispam["dataId"].ToObject<String>();
                                    int suggestion = antispam["suggestion"].ToObject<int>();
                                    resultType = antispam["resultType"].ToObject<int>();
                                    if(resultType == 2){
                                        String callback = antispam["callback"].ToObject<String>();
                                        int censorSource = antispam["censorSource"].ToObject<int>();
                                        int censorRound = antispam["censorRound"].ToObject<int>();
                                        if(null != antispam["censorLabels"]){
                                            JArray censorLabels = (JArray)antispam.SelectToken("censorLabels");
                                            foreach (var censorLabelElement in censorLabels){
                                                JObject censorLabel = (JObject)censorLabelElement;
                                                String subLabelCode = antispam["code"].ToObject<String>();
                                                String subLabelDesc = antispam["desc"].ToObject<String>();
                                            }
                                        }
                                    }
                                    int censorType = antispam["censorType"].ToObject<int>();
                                    Boolean isRelatedHit = antispam["isRelatedHit"].ToObject<Boolean>();
                                    JArray labels = (JArray)antispam.SelectToken("labels");
                                    Console.WriteLine(String.Format("内容安全结果，taskId: {0}，dataId: {1}，suggestion: {2}", taskId, dataId, suggestion));
                                    foreach (var labelElement in labels)
                                    {
                                        JObject labelItem = (JObject)labelElement;
                                        int label = labelItem.GetValue("label").ToObject<Int32>();
                                        int level = labelItem.GetValue("level").ToObject<Int32>();
                                        JArray subLabels = (JArray)labelItem.SelectToken("subLabels");
                                        if(null != subLabels){
                                            foreach (var subLabelElement in subLabels){
                                                JObject subLabelItem = (JObject)subLabelElement;
                                                String subLabel = subLabelItem.GetValue("subLabel").ToObject<String>();
                                                Console.WriteLine(String.Format("内容安全分类，label: {0}，subLabel: {1}", label, subLabel));
                                                if(null != subLabelItem["details"]){
                                                    JObject details = subLabelItem["details"].ToObject<JObject>();
                                                    // 自定义敏感词信息
                                                    if(null != details["keywords"]){
                                                        JArray keywords = (JArray)antispam.SelectToken("keywords");
                                                        if(null != keywords){
                                                            foreach (var keywordElement in keywords){
                                                                JObject keywordItem = (JObject)keywordElement;
                                                                String word = keywordItem["word"].ToObject<String>();
                                                            }
                                                        }
                                                    }
                                                    // 自定义名单库信息
                                                    if(null != details["libInfos"]){
                                                        JArray libInfos = (JArray)antispam.SelectToken("libInfos");
                                                        if(null != libInfos){
                                                            foreach (var libInfoElement in libInfos){
                                                                JObject libInfoItem = (JObject)libInfoElement;
                                                                int type = libInfoItem["type"].ToObject<int>();
                                                                String entity = libInfoItem["entity"].ToObject<String>();
                                                            }
                                                        }
                                                    }
                                                    // 线索信息
                                                    if(null != details["hitInfos"]){
                                                        JArray hitInfos = (JArray)antispam.SelectToken("hitInfos");
                                                        if(null != hitInfos){
                                                            foreach (var hitInfoElement in hitInfos){
                                                                JObject hitInfoItem = (JObject)hitInfoElement;
                                                                String value = hitInfoItem["value"].ToObject<String>();
                                                                JArray positions = (JArray)hitInfoItem.SelectToken("positions");
                                                                if(null != positions){
                                                                    foreach (var positionElement in positions){
                                                                        JObject positionItem = (JObject)positionElement;
                                                                        String fieldName = positionItem["fieldName"].ToObject<String>();
                                                                        int startPos = positionItem["startPos"].ToObject<int>();
                                                                        int endPos = positionItem["endPos"].ToObject<int>();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    // 反作弊信息
                                                    if(null != details["anticheat"]){
                                                        JObject anticheat = details["anticheat"].ToObject<JObject>();
                                                        if(null != anticheat){
                                                            int type = anticheat["type"].ToObject<int>();
                                                        }
                                                    }
                                                }
                                            }
                                            
                                        }
                                    }
                                }
                            }
                            // 情感分析结果
                            if(null != resultObject["emotionAnalysis"]){
                                JObject emotionAnalysis = resultObject["emotionAnalysis"].ToObject<JObject>();
                                if(null != emotionAnalysis){
                                    String taskId = emotionAnalysis["taskId"].ToObject<String>();
                                    String dataId = emotionAnalysis["dataId"].ToObject<String>();
                                    if (null != emotionAnalysis["details"]) {
                                        JArray details = (JArray)emotionAnalysis.SelectToken("details");
                                        Console.WriteLine(String.Format("情感分析结果，taskId: {0}，dataId: {1}，details: {2}", taskId, dataId, details));
                                        if (details != null) {
                                            foreach (var detailElement in details){
                                                JObject detailItem = (JObject)detailElement;
                                                double positiveProb = detailItem["positiveProb"].ToObject<double>();
                                                double negativeProb = detailItem["negativeProb"].ToObject<double>();
                                                String sentiment = detailItem["sentiment"].ToObject<String>();
                                            }
                                        }
                                    }
                                }
                            }
                            // 反作弊结果
                            if(null != resultObject["anticheat"]){
                                JObject anticheat = resultObject["anticheat"].ToObject<JObject>();
                                if(null != anticheat){
                                    String taskId = anticheat["taskId"].ToObject<String>();
                                    String dataId = anticheat["dataId"].ToObject<String>();
                                    if (null != anticheat["details"]) {
                                        JArray details = (JArray)anticheat.SelectToken("details");
                                        Console.WriteLine(String.Format("反作弊结果，taskId: {0}，dataId: {1}，details: {2}", taskId, dataId, details));
                                        if (details != null) {
                                            foreach (var detailElement in details){
                                                JObject detailItem = (JObject)detailElement;
                                                int suggestion = detailItem["suggestion"].ToObject<int>();
                                                JArray hitInfos = (JArray)detailItem.SelectToken("hitInfos");
                                                if(null != hitInfos){
                                                    foreach (var hitInfoElement in hitInfos){
                                                        JObject hitInfoItem = (JObject)hitInfoElement;
                                                        int hitType = hitInfoItem["hitType"].ToObject<int>();
                                                        String hitMsg = hitInfoItem["hitMsg"].ToObject<String>();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // 用户画像结果
                            if(null != resultObject["userRisk"]){
                                JObject userRisk = resultObject["userRisk"].ToObject<JObject>();
                                if(null != userRisk){
                                    String taskId = userRisk["taskId"].ToObject<String>();
                                    String dataId = userRisk["dataId"].ToObject<String>();
                                    if (null != userRisk["details"]) {
                                        JArray details = (JArray)userRisk.SelectToken("details");
                                        Console.WriteLine(String.Format("用户画像结果，taskId: {0}，dataId: {1}，details: {2}", taskId, dataId, details));
                                        if (details != null) {
                                            foreach (var detailElement in details){
                                                JObject detailItem = (JObject)detailElement;
                                                String account = detailItem["account"].ToObject<String>();
                                                int accountLevel = detailItem["accountLevel"].ToObject<int>();
                                            }
                                        }
                                    }
                                }
                            }
                            // 语种检测结果
                            if(null != resultObject["language"]){
                                JObject language = resultObject["language"].ToObject<JObject>();
                                if(null != language){
                                    String taskId = language["taskId"].ToObject<String>();
                                    String dataId = language["dataId"].ToObject<String>();
                                    if (null != language["details"]) {
                                        JArray details = (JArray)language.SelectToken("details");
                                        Console.WriteLine(String.Format("语种检测结果，taskId: {0}，dataId: {1}，details: {2}", taskId, dataId, details));
                                        if (details != null) {
                                            foreach (var detailElement in details){
                                                JObject detailItem = (JObject)detailElement;
                                                String type = detailItem["type"].ToObject<String>();
                                            }
                                        }
                                    }
                                }
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

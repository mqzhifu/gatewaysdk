using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor.PackageManager.Requests;
using System;
using System.Threading.Tasks;

public delegate string HttpCallback(long responseCode, JsonData jsonData);

//处理HTTP协议 请求/响应
//此类会抛异常，注意外层调用时要捕捉~
public class HttpUtil
{
    public string serverHttpDns;//后端服务器的 HTTP 地址+端口
    public string userToken;    //登陆成功后，获取到的用户 TOKEN
    public string sourceType;   //来源，即当前设备
    public string projectId;    //项目ID
    public string access;       //项目的请求密钥
    //public int timeout;         //请求超时时间
    public Log log;             //日志输出
    public int reqTimeout;      //http 请求超时时间(秒)

    /*构造函数
     * dns:服务端URL地址，如：http://www.baidu.com/
     * header 字段说明如下：
     * sourceType:
     *  11 //MAC台式浏览器
        12 //MAC台式APP
        22 //WIN台式浏览器
        23 //WIN台式APP
        31 //安卓手机浏览器
        32 //安卓手机APP
        41 //IOS手机浏览器
        42 //IOS手机APP
        99 //未知
     * projectId:项目ID，向后台管理员申请
     * access:http头基础的验证，向后台管理员申请
     * token:用户名+密码登陆成功后返回，之后大部分接口都要加上此字段值
     * 注：所有的 http-header 都要遵守W3C规定，前面要加上：X，如X-PorjectID、X-Token.......
     */

    public HttpUtil(string dns, string sourceType, string projectId, string access,int logLevel)
    {
        this.log = new Log(logLevel, "HttpUtil  ");

        this.serverHttpDns  = dns;
        this.userToken      = "";
        this.access         = access;
        this.sourceType     = sourceType;
        this.projectId      = projectId;
        this.reqTimeout = 5;
    }
    //发送请求，阻塞模式
    public JsonData RequestBlock(string HttpMethod, string uri, string jsonStr)
    {
        UnityWebRequest req = this.GetUnityWebRequest(uri, HttpMethod, jsonStr,"block");
        var timeout = this.reqTimeout * 1000;
        var  sendWebRequestContext = req.SendWebRequest();
        var time = 0;
        var everySleepTime = 100;
        
        while (!sendWebRequestContext.isDone)
        {
            this.log.Info("RequestBlock while time:" + time+", sendWebRequestContext.webRequest.error:" + sendWebRequestContext.webRequest.error);
            Thread.Sleep(everySleepTime);
            //Task.Delay(everySleepTime);
            time += everySleepTime;
            //this.log.debug("RequestBlock while time:" + time);
            if (time > timeout)
            {
                req.Dispose();
                this.throwExpception("http request timeout second:" + this.reqTimeout);
            }
        }

        JsonData jd = this.ProcessHttpBack(req);
        req.Dispose();
        return jd;
    }
    //发送请求，async await 模式
    async public void RequestAsync(string HttpMethod, string uri, string jsonStr, HttpCallback callback)
    {
        UnityWebRequest req = this.GetUnityWebRequest(uri, HttpMethod, jsonStr, "async");
        req.SendWebRequest();

        await Task.Delay(100);
        JsonData jd = this.ProcessHttpBack(req);
        callback(req.responseCode, jd);
    }
    //发送请求，协程 模式
    public IEnumerator RequestCoroutine(string HttpMethod, string uri, string jsonStr, HttpCallback callback)
    {
        UnityWebRequest req = this.GetUnityWebRequest(uri, HttpMethod, jsonStr, "coroutine");
        yield return req.SendWebRequest();
        JsonData jd = this.ProcessHttpBack(req);
        req.Dispose();
        callback(req.responseCode,jd);
    }
    //获取一个 WebRequest  实例
    public UnityWebRequest GetUnityWebRequest(string uri,string HttpMethod, string jsonStr,string source)
    {
        this.Check();
        if (uri == "" || HttpMethod == "") {
            this.throwExpception(" uri | HttpMethod is empty ");
        }
        string url = this.GetUrl(uri);
        this.log.Info("HttpRequest url:" + url + " ,post data:" + jsonStr + " HttpMethod:"+ HttpMethod + " , source:"+source);
        UnityWebRequest req = new UnityWebRequest(url);
        
        req.downloadHandler = new DownloadHandlerBuffer();
        if (HttpMethod == "POST")
        {
            req.method = UnityWebRequest.kHttpVerbPOST;
            if (jsonStr != "")
            {
                byte[] postBytes = Encoding.UTF8.GetBytes(jsonStr);
                req.uploadHandler = new UploadHandlerRaw(postBytes);
            }
        }
        else if (HttpMethod == "GET")
        {
            req.method = UnityWebRequest.kHttpVerbGET;
        }
        else
        {
            req.Dispose();
            this.throwExpception("HttpMethod wrong: " + HttpMethod + " .only support:POST GET");
        }

        this.setCommonHeader(req);
        return req;
    }
    //http 网络正常请求成功，对返回的数据的进行统一格式化处理
    public JsonData ProcessHttpBack(UnityWebRequest req)
    {
        JsonData jd = null;
        if (req.result == UnityWebRequest.Result.Success)
        {     
            this.log.debug("request ok , response body string:" + req.downloadHandler.text);
            jd = JsonMapper.ToObject(req.downloadHandler.text);
            if ((int)(jd["code"]) != 200)
            {
                string errInfo = "commonResponse has err . code:" + jd["code"] + " , msg:" + jd["msg"];
                this.throwExpception(errInfo);
                throw new Exception(errInfo);
            }
            else
            {
                return jd;
            }
        }
        else
        {
            string errInfo = "request failed ，error: " + req.error + " , result: " + req.result + "  , responseCode: " + req.responseCode;
            this.throwExpception(errInfo);          
        }
        return jd;
    }
    //登陆成功后，要设置一下 userToken 下次请求追回到 header中
    public void SetUserToken(string token)
    {
        this.userToken = token;
    }
    //设置 http 公共请求头
    public void setCommonHeader(UnityWebRequest req)
    {
        req.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        req.SetRequestHeader("X-Source-Type", this.sourceType);
        req.SetRequestHeader("X-Project-Id", this.projectId);
        req.SetRequestHeader("X-Access", this.access);
        req.SetRequestHeader("X-Token", this.userToken);
    }
    //校验 url + header 参数
    public void Check()
    {
        if (this.serverHttpDns == "" || this.sourceType == "" || this.projectId == "" || this.access == "")
        {
            this.throwExpception("serverHttpDns || sourceType || projectId || access is empty!");
        }
    }
    //获取当前请求的 URL 地址
    public string GetUrl(string uri)
    {
        return this.serverHttpDns + uri;
    }

    public void throwExpception(string errInfo)
    {
        this.log.Err(errInfo);
        throw new Exception(errInfo);
    }

}

//public IEnumerator SendWebRequest()
//{
//    //接口地址
//    string url = "http://**.**.***.***:****/********/*****";
//    //post数据 通过序列化获得字符串
//    string postData = JsonMapper.ToJson(new Affirm());
//    //Post网络请求
//    using (UnityWebRequest request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST))
//    {
//        byte[] postBytes = Encoding.UTF8.GetBytes(postData);
//        request.uploadHandler = new UploadHandlerRaw(postBytes);
//        request.downloadHandler = new DownloadHandlerBuffer();
//        request.SetRequestHeader("Content-Type", "application/json");
//        yield return request.SendWebRequest();
//        if (!request.isNetworkError && !request.isHttpError)
//        {
//            Debug.Log("发起网络请求成功");
//        }
//        else
//        {
//            Debug.LogError($"发起网络请求失败：确认过闸接口 -{request.error}");
//        }
//    }
//}
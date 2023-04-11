using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Networking;
using static System.Collections.Specialized.BitVector32;



public class ApiLogicAwait
{
    //public const int API_LOGIC_INIT_STATUS_EXCEPTION = -1;  //初始化失败，大多是接口请求异常
    //public const int API_LOGIC_INIT_STATUS_UN_PROCESS = 0;  //未处理
    //public const int API_LOGIC_INIT_STATUS_PROCESS = 1;     //处理中
    //public const int API_LOGIC_INIT_STATUS_OK = 2;          //处理成功
    public enum API_LOGIC_INIT_STATUS
    {
        EXCEPTION = -1,   //初始化失败，大多是接口请求异常,
        UN_PROCESS = 0,   //未处理,
        PROCESSING = 1,   //处理中
        SUCCESS = 2,      //处理成功
    }
    
    public int      initTimeout;        //初始化时，要请求后端接口，正常2秒内肯定是能完成的，超时后就证明有问题了
    public int      userId;             //用户ID
    public string   userToken;          //用户登陆成功的token
    public string   username;           //用户名
    public string   password;           //用户密码
    public int      initStatus;         //当前初始化的状态:-1发生错误，0未处理，1处理中，2成功

    public Log      log;                //日志输出
    public HttpUtil httpUtil;           //http 请求基础类
    public GatewayConfig gatewayConfig;//网关配置信息，接口获取，主要是用于长连接
    public ProtocolAction protocolAction; //
    public Websocket websocket;         //基础ws类
    //构造函数
    public ApiLogicAwait(HttpUtil httpUtil,ProtocolAction protocolAction)
    {
        this.log = new Log(1, "ApiLogicAwait  ");

        this.log.Info("start:");

        this.initTimeout    = 2;
        this.userId         = 0;
        this.userToken      = "";
        this.username       = "";
        this.password       = "";
        this.httpUtil       = httpUtil;
        this.protocolAction = protocolAction;
        this.initStatus     = (int)API_LOGIC_INIT_STATUS.UN_PROCESS;
        
        this.log.Info("finish.");
    }


    public void SetUserInfo(string username, string password)
    {
        this.username = username;
        this.password = password;
    }


    public string Init()
    {
        this.initStatus = (int)API_LOGIC_INIT_STATUS.PROCESSING;
        try
        {
            this.Login();
            this.GetActionMap();
            this.GetConfig();
            this.initStatus = (int)API_LOGIC_INIT_STATUS.SUCCESS;

        }
        catch (Exception e)
        {
            this.log.Info("im in exception "+e.Message);
            this.initStatus = (int)API_LOGIC_INIT_STATUS.EXCEPTION;
        }
        

        return "";
    }

    public int Login()
    {
        this.log.Info("im in login func.");
        string uri = "base/login";

        UserLoginReq userLogin = new UserLoginReq();
        userLogin.username = this.username;
        userLogin.password = this.password;
        string jsonStr = JsonMapper.ToJson(userLogin);


        JsonData jd =  this.httpUtil.RequestBlock("POST", uri, jsonStr);
        this.LoginBack(jd);

        return 1;
        //this.httpUtil.Request(uri, jsonStr, cb);

    }


    public int GetActionMap()
    {
        string uri = "gateway/action/map";
        JsonData jd = this.httpUtil.RequestBlock("GET", uri, "");
        //Debug.Log("GetActionMap:" + jd["data"].ToJson());
        var am = JsonMapper.ToObject<ProtocolActionMap>(jd["data"].ToJson());
        //Debug.Log("am:" + am.client["90104"].desc);
        //this.GetActionMapBack(jd);
        this.protocolAction.SetMap(am);
        return 1;


    }

    public int GetConfig()
    {
        string uri = "gateway/config";
        JsonData jd = this.httpUtil.RequestBlock("GET", uri, "");
        this.GetConfigBack(jd);
        return 1;

    }


    public string LoginBack(JsonData jsonData)
    {
        UserLoginRes userLoginRes = JsonMapper.ToObject<UserLoginRes>(jsonData["data"].ToJson());

        this.log.Info("LoginBack ,uid:" + userLoginRes.user.id + " , token:" + userLoginRes.token);

        this.userId = userLoginRes.user.id;
        this.userToken = userLoginRes.token;
        this.httpUtil.SetUserToken (userLoginRes.token) ;

        return "";
    }

    //public string GetActionMapBack( JsonData jsonData)
    //{
    //    this.log.Info("GetActionMapBack:");
    //    ActionMap actionMap = JsonMapper.ToObject<ActionMap>(jsonData["data"].ToJson());
    //    //Debug.Log("actionMap:"+ actionMap.client["10100"].desc);
    //    //this.GetConfig();
    //    return "";
    //}

    public string GetConfigBack( JsonData jsonData)
    {
        this.log.Info("GetConfigBack:");


        GatewayConfig gatewayConfig = JsonMapper.ToObject<GatewayConfig>(jsonData["data"].ToJson());
        Debug.Log("gatewayConfig wsPort: " + gatewayConfig.wsPort + " , wsUri:" + gatewayConfig.wsUri + " ,outIp:" + gatewayConfig.outIp );

        this.gatewayConfig = gatewayConfig;

        //this.websocket = new Websocket();
        //this.websocket.Init(this.gatewayConfig);
        return "";
    }

    //public void print(object message)
    //{
    //    Debug.Log(this.prefix + message);
    //}

}
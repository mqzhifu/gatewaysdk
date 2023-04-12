using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using static System.Collections.Specialized.BitVector32;

//处理所有请求后端API的逻辑
public class ApiLogicAwait
{
    public enum API_LOGIC_INIT_STATUS
    {
        EXCEPTION = -1,   //初始化失败，大多是接口请求异常,
        UN_PROCESS = 0,   //未处理,
        PROCESSING = 1,   //处理中
        SUCCESS = 2,      //处理成功
    }
    
    public int      initTimeout;        //初始化时，要请求后端接口，正常2秒内肯定是能完成的，超时后就证明有问题了
    public int      userId;             //用户ID
    public string   userToken;          //用户登陆成功的 token
    public string   username;           //用户名
    public string   password;           //用户密码
    public int      initStatus;         //当前初始化的状态:-1发生错误，0未处理，1处理中，2成功

    public Log              log;            //日志输出
    public HttpUtil         httpUtil;       //http 请求基础类
    public GatewayConfig    gatewayConfig;  //网关配置信息，接口获取，主要是用于长连接
    public ProtocolAction   protocolAction; //长连接的协议定义中的函数映射
    public Websocket        websocket;      //基础ws类
    //构造函数
    public ApiLogicAwait(HttpUtil httpUtil,int logLevel)
    {
        this.log = new Log(logLevel, "ApiLogicAwait  ");

        this.log.Info("start:");

        this.initTimeout    = 2;
        this.userId         = 0;
        this.userToken      = "";
        this.username       = "";
        this.password       = "";
        this.httpUtil       = httpUtil;
        
        this.initStatus     = (int)API_LOGIC_INIT_STATUS.UN_PROCESS;
        
        this.log.Info("finish.");
    }

    public void SetProtocolAction(ProtocolAction protocolAction)
    {
        this.protocolAction = protocolAction;
    }

    public void SetUserInfo(string username, string password)
    {
        this.username = username;
        this.password = password;
    }


    public void InitLogin()
    {
        this.initStatus = (int)API_LOGIC_INIT_STATUS.PROCESSING;
        this.Login();
        this.initStatus = (int)API_LOGIC_INIT_STATUS.SUCCESS;
        //try
        //{
        //    this.Login();
        //    this.GetActionMap();
        //    this.GetConfig();
        //    this.initStatus = (int)API_LOGIC_INIT_STATUS.SUCCESS;

        //}
        //catch (Exception e)
        //{
        //    this.log.Info("im in exception "+e.Message);
        //    this.initStatus = (int)API_LOGIC_INIT_STATUS.EXCEPTION;
        //}

    }
    public void InitGateway()
    {
        this.GetActionMap();
        this.GetConfig();
    }
    //登陆
    public int Login()
    {
        //this.log.Info("im in login func.");
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
    //获取 - 长连接的函数映射表
    public int GetActionMap()
    {
        string uri = "gateway/action/map";
        JsonData jd = this.httpUtil.RequestBlock("GET", uri, "");
        var am = JsonMapper.ToObject<ProtocolActionMap>(jd["data"].ToJson());
        this.protocolAction.SetMap(am);
        return 1;


    }
    //获取 - 网关的配置文件（主要是用于长连接）
    public int GetConfig()
    {
        string uri = "gateway/config";
        JsonData jd = this.httpUtil.RequestBlock("GET", uri, "");
        this.GetConfigBack(jd);
        return 1;
    }
    //登陆成功回调
    public string LoginBack(JsonData jsonData)
    {
        UserLoginRes userLoginRes = JsonMapper.ToObject<UserLoginRes>(jsonData["data"].ToJson());
        this.log.debug("LoginBack ,uid:" + userLoginRes.user.id + " , token:" + userLoginRes.token);

        this.userId = userLoginRes.user.id;
        this.userToken = userLoginRes.token;
        this.httpUtil.SetUserToken (userLoginRes.token) ;

        return "";
    }

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
    //public string GetActionMapBack( JsonData jsonData)
    //{
    //    this.log.Info("GetActionMapBack:");
    //    ActionMap actionMap = JsonMapper.ToObject<ActionMap>(jsonData["data"].ToJson());
    //    //Debug.Log("actionMap:"+ actionMap.client["10100"].desc);
    //    //this.GetConfig();
    //    return "";
    //}

}
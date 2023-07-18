//using System;
//using System.Collections;
//using System.Collections.Generic;
//using LitJson;
//using Unity.VisualScripting;
//using Unity.VisualScripting.FullSerializer;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;
//using static System.Collections.Specialized.BitVector32;

//public class ApiLogic : MonoBehaviour
//{
//    public int UserId;
//    public string Username;
//    public string Password;
//    public HttpUtil httpUtil;
//    public GatewayConfig gatewayConfig;
//    public ActionMap actionMap;
//    public Websocket websocket;
//    //public ApiLogic( HttpUtil httpUtil)
//    //{
//    //    this.Username = "frame_sync_1";
//    //    this.Password = "123456";
//    //    this.UserId = 0;
//    //    this.httpUtil = httpUtil;
//    //}

//    void Start()
//    {
//        Debug.Log("api start:");
//        this.Username = "frame_sync_1";
//        this.Password = "123456";
//        this.UserId = 0;
//        this.httpUtil = new HttpUtil("aaaa");
//        this.Login();
//        Debug.Log("api start finish.");
//    }

//    public void Login()
//    {

//        this.Username = "frame_sync_1";
//        this.Password = "123456";
//        this.UserId = 0;
//        //this.httpUtil = httpUtil;

//        Debug.Log("im in login func.");
//        string uri = "base/login";

//        UserLoginReq userLogin = new UserLoginReq();
//        userLogin.username = this.Username;
//        userLogin.password = this.Password;
//        string jsonStr = JsonMapper.ToJson(userLogin);

//        Callback cb = new Callback(this.LoginBack);


//        StartCoroutine(this.httpUtil.Request("POST", uri, jsonStr, cb));
//        //this.httpUtil.Request(uri, jsonStr, cb);

//    }

//    //public void GetProto()
//    //{
//    //    string uri = "gateway/proto";
//    //    Callback cb = new Callback(this.GetProtoBack);
//    //    StartCoroutine(this.httpUtil.Request("GET", uri, "", cb));

//    //}

//    public void GetActionMap()
//    {
//        string uri = "gateway/action/map";
//        Callback cb = new Callback(this.GetActionMapBack);
//        StartCoroutine(this.httpUtil.Request("GET", uri, "", cb));

//    }

//    public void GetConfig()
//    {
//        string uri = "gateway/config";
//        Callback cb = new Callback(this.GetConfigBack);
//        StartCoroutine(this.httpUtil.Request("GET", uri, "", cb));

//    }


//    public string LoginBack(long responseCode, JsonData jsonData)
//    {
//        //Debug.Log("LoginBack ,uid:" + jsonData["data"]["user"]["id"] + " , token:" + jsonData["data"]["token"]);
//        //this.httpUtil.setUserToken((string)(jsonData["data"]["token"]));
//        //this.UserId = (int)(jsonData["data"]["user"]["id"]);

//        UserLoginRes userLoginRes = JsonMapper.ToObject<UserLoginRes>(jsonData["data"].ToJson());
//        Debug.Log("LoginBack ,uid:" + userLoginRes.user.id + " , token:" + userLoginRes.token);

//        this.UserId = userLoginRes.user.id;
//        this.httpUtil.setUserToken (userLoginRes.token) ;

//        this.GetActionMap();
//        return "";
//    }

//    //public string GetProtoBack(long responseCode, JsonData jsonData)
//    //{
//    //    Debug.Log("GetProtoBack:");
//    //    return "";
//    //}

//    public string GetActionMapBack(long responseCode, JsonData jsonData)
//    {
//        Debug.Log("GetActionMapBack:");
//        ActionMap actionMap = JsonMapper.ToObject<ActionMap>(jsonData["data"].ToJson());
//        //Debug.Log("actionMap:"+ actionMap.client["10100"].desc);
//        this.GetConfig();
//        return "";
//    }

//    public string GetConfigBack(long responseCode, JsonData jsonData)
//    {
//        Debug.Log("GetConfigBack:");


//        GatewayConfig gatewayConfig = JsonMapper.ToObject<GatewayConfig>(jsonData["data"].ToJson());
//        Debug.Log("gatewayConfig wsPort: " + gatewayConfig.wsPort + " , wsUri:" + gatewayConfig.wsUri + " ,outIp:" + gatewayConfig.outIp );

//        this.gatewayConfig = gatewayConfig;

//        //this.websocket = new Websocket();
//        //this.websocket.Init(this.gatewayConfig);
//        return "";
//    }

//}
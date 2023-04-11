using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using LitJson;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Net.WebSockets;
using System.Threading.Tasks;

using Debug = UnityEngine.Debug;

public delegate void HttpCallback();

public class Core : MonoBehaviour
{
    public HttpUtil         httpUtil;
    public Gateway        gateway;   
    public ApiLogicAwait    apiLogicAwait;
    public ProtocolAction   protocolAction;
    //public ApiLogic apiLogic;
    
    void Start()
    {
        Util.SetDebug(1);
        Util.Log("core start....");
        this.httpUtil = new HttpUtil("http://8.142.177.235:2222/","11","6", "imzgoframe");
        this.protocolAction = new ProtocolAction();
        //this.UseCoroutine();
        this.UseAsyncAwait();
        
        Util.Log("core start finish.");

    }

    
    void Update()
    {
        
    }

    void UseAsyncAwait()
    {
        this.apiLogicAwait = new ApiLogicAwait (this.httpUtil,this.protocolAction);
        this.apiLogicAwait.SetUserInfo("frame_sync_1","123456");

        //StartCoroutine(this.apiLogicAwait.Entry());
        this.apiLogicAwait.Init();

        int retryTime = 0;
        while (this.apiLogicAwait.initStatus == (int)ApiLogicAwait.API_LOGIC_INIT_STATUS.PROCESSING)
        {
            if (retryTime > this.apiLogicAwait.initTimeout * 1000)
            {
                Debug.Log("retryTime timeout. initTimeout:" + this.apiLogicAwait.initTimeout);
                break;
            }
            Task.Delay(100);
            retryTime += 100;
        }

        if (this.apiLogicAwait.initStatus != (int)ApiLogicAwait.API_LOGIC_INIT_STATUS.SUCCESS)
        {
            Debug.Log("err:apiLogicAwait.InitStatus ");
        }
        else
        {
            //json格式
            //this.websocket = new Websocket(Websocket.CONTENT_TYPE_JSON, Websocket.PROTOCOL_TYPE_WS, this.apiLogicAwait.gatewayConfig, this.apiLogicAwait.actionMap, this.apiLogicAwait.UserToken);
            //protobuf 格式
            this.gateway = new Gateway((int)Gateway.CONTENT_TYPE.PROTOBUF, (int)Gateway.PROTOCOL_TYPE.WS, this.apiLogicAwait.gatewayConfig, this.protocolAction, this.apiLogicAwait.userToken);

            this.gateway.Init();
        }
    }

    void UseCoroutine()
    { 
        //GameObject gameObject = new GameObject("DetectPlayer");
        //gameObject.AddComponent<ApiLogic>();
        //Util.Log("core start finish.");
    }


}

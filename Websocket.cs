using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Google.Protobuf;
using LitJson;
using Pb;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.VersionControl;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEditor.MaterialProperty;
using static UnityEditor.VersionControl.Asset;

//websocket 协议类，消息的收发
public class Websocket
{
    public Log                  log;
    public GatewayConfig        gatewayConfig;      //服务器的配置信息
    public ClientWebSocket      clientWebSocket;   
    public int                  state ;             //当前连接状态
    public int                  wsProtocol ;        //ws or wss
    public int                  connectTimeout;     //创建连接时：超时时间(秒)

    public ReceiveMsgCallback   gatewayReceiveMsg;  //接收消息 回调
    public FDExceptionCallback  fdExceptionCallback;
    public ConnectCallback      connectCallback;    //长连接，成功回调


    public enum WS_PROTOCOL
    {
        WS  = 1,
        WSS = 2,
    }

    public Websocket(GatewayConfig gatewayConfig, ReceiveMsgCallback callback, ConnectCallback back,int logLevel,int wsProtocol,int connectTimeout, FDExceptionCallback fdExceptionCallback)
    {
        this.state = (int)Gateway.CONN_STATE.INIT;
        this.log = new Log(logLevel, "Websocket  ");
        this.gatewayConfig = gatewayConfig;
        this.gatewayReceiveMsg = callback;
        this.connectCallback = back;
        this.wsProtocol = wsProtocol;
        this.connectTimeout = connectTimeout;
        this.fdExceptionCallback = fdExceptionCallback;
    }
    //初始化，连接后端服务器
    public async void Init()
    {               
        var dns = this.GetConnectDns();
        //var dns = this.TestWongConnectDns();
        this.log.Info("connect url:" + dns + " ConnectAsync......");

        this.state = (int)Gateway.CONN_STATE.ING;

        this.clientWebSocket = new ClientWebSocket();
        //this.ct = new CancellationToken();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(this.connectTimeout));
        try
        {
            await this.clientWebSocket.ConnectAsync(new Uri(dns), cts.Token);
            this.log.Info("ws connect success，"+ "clientWebSocket.State:" + this.clientWebSocket.State);
            this.state = (int)Gateway.CONN_STATE.SUCCESS;            
            this.ReceiveMsg();
            this.connectCallback((int)Gateway.CONN_STATE.SUCCESS, "SUCCESS");
        }
        catch (Exception e)
        {
            this.state = (int)Gateway.CONN_STATE.FAILED;
            this.connectCallback((int)Gateway.CONN_STATE.FAILED, e.Message);
        }        
    }
    public string TestWongConnectDns()
    {
        var dns = "ws://127.0.0.1:1234/ws";
        return dns;
    }
    public string GetConnectDns()
    {
        if (gatewayConfig.outIp == "" || gatewayConfig.wsPort == "" || gatewayConfig.wsUri == "")
        {
            this.throwExpception("check gatewayConfig , outIp || wsPort || wsUri is empty~ ");
        }
        //使用IP连接
        string url  = gatewayConfig.outIp + ":" + gatewayConfig.wsPort + gatewayConfig.wsUri;
        //使用域名连接
        //string url = gatewayConfig.outDomain + ":" + gatewayConfig.wsPort + gatewayConfig.wsUri;
        if (this.wsProtocol  == (int)Websocket.WS_PROTOCOL.WS)
        {
            url  = "ws://" + url;
        }
        else
        {
            url += "wss://" + url;
        }
        return url;
    }
    public void Close()
    {
        this.clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait();
    }
    //接收后端发送的消息
    public async void ReceiveMsg()
    {

        while (true)
        {
            this.log.Info("ReceiveMsg one :");
            if (this.clientWebSocket.State != WebSocketState.Open || this.clientWebSocket.State == WebSocketState.Closed)
            {
                this.fdExceptionCallback("clientWebSocket.State != Open || WebSocketState.Closed");
                this.log.Err("ws conn:"+ this.clientWebSocket.State + " , "+ this.clientWebSocket.State + " , so break loop.");
                break;
            }
            var readBuff = new byte[1024];
            WebSocketReceiveResult result = null;
            try
            {
                //这里用了 None，间接也可以理解为阻塞模式，也可以打开，具体看使用者吧
                result = await this.clientWebSocket.ReceiveAsync(new ArraySegment<byte>(readBuff), CancellationToken.None);//接受数据
            }
            catch(Exception e)
            {
                if (e.Message == "The remote party closed the WebSocket connection without completing the close handshake.")
                {
                    this.fdExceptionCallback("server fd has closed");
                    this.log.Err("server has closed....");

                }
                else
                {
                    this.log.Err("unknow exception:"+e.Message);
                    this.fdExceptionCallback("unknow exception:" + e.Message);
                }

                break;
            }
           
            if (result.MessageType == WebSocketMessageType.Close)
            {
                this.log.Err("WebSocket connection closed by server " + " , so break loop.");
                this.fdExceptionCallback("WebSocket connection closed by server");
                break;
            }

            this.log.Info("ReceiveMsg MessageType:" + result.MessageType+ ", Count:" + result.Count);
            if (this.gatewayReceiveMsg != null)
            {
                try
                {
                    this.gatewayReceiveMsg(readBuff);
                }
                catch(Exception e)
                {
                    this.log.Err("exec gatewayReceiveMsg err:"+e.Message);
                }                  
            }            
        }
        this.log.debug("ReceiveMsg loop dead.");
    }
    public async void SendMsg(byte[] content)
    {
        //var sendData = new ArraySegment<byte>(Encoding.UTF8.GetBytes("hello"));
        await clientWebSocket.SendAsync(content, WebSocketMessageType.Binary, true, new CancellationToken()); //发送数据
    }

    public void throwExpception(string errInfo)
    {
        this.log.Err(errInfo);
        throw new Exception(errInfo);
    }
}

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
    public GatewayConfig gatewayConfig;

    public ClientWebSocket clientWebSocket;
    public CancellationToken ct;
    public Log log;
    public GatewayReceiveMsg gatewayReceiveMsg;
    public ConnSuccess connSuccessBack;
    public int State ;
    public enum CONN_STATE
    {
        INIT = 1,
        FAILED = 2,
        SUCCESS = 3,
    }

    public Websocket(GatewayConfig gatewayConfig, GatewayReceiveMsg callback, ConnSuccess back,int logLevel)
    {
        this.log = new Log(logLevel, "Websocket  ");
        this.gatewayConfig = gatewayConfig;
        this.gatewayReceiveMsg = callback;
        this.connSuccessBack = back;
    }
    //初始化，连接后端服务器
    public async void Init()
    {
        
        if (gatewayConfig.outIp == "" || gatewayConfig.wsPort == "" || gatewayConfig.wsUri == "")
        {
            this.throwExpception("check gatewayConfig , outIp || wsPort || wsUri is empty~ ");
        }

        this.State = (int)Websocket.CONN_STATE.INIT;

        string url = gatewayConfig.outIp + ":" + gatewayConfig.wsPort + gatewayConfig.wsUri;

        this.log.Info("ws url:" + url + " ConnectAsync......");

        this.clientWebSocket = new ClientWebSocket();
        this.ct = new CancellationToken();

        
        await this.clientWebSocket.ConnectAsync(new Uri("ws://" + url), ct);
        if (this.clientWebSocket.State != WebSocketState.Open)
        {
            this.State = (int)Websocket.CONN_STATE.FAILED;
            this.throwExpception("clientWebSocket.State != Open , this state:" + this.clientWebSocket.State);
        }
        this.log.Info("ws connect success.");
        this.State = (int)Websocket.CONN_STATE.SUCCESS;
        this.connSuccessBack();
        this.ReceiveMsg();

        //var sendData = new ArraySegment<byte>(Encoding.UTF8.GetBytes("hello"));
        //await clientWebSocket.SendAsync(sendData, WebSocketMessageType.Binary, true, ct); //发送数据
    }

    //接收后端发送的消息
    public async void ReceiveMsg()
    {

        while (true)
        {
            if (!((this.clientWebSocket.State == WebSocketState.Open) || (this.clientWebSocket.State == WebSocketState.CloseSent)))
            {
                this.log.Err("ws conn:"+ this.clientWebSocket.State + " , "+ this.clientWebSocket.State);
                break;
            }
            var readBuff = new byte[1024];
            var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(readBuff), new CancellationToken());//接受数据
            this.log.Info("ReceiveMsg MessageType:" + result.MessageType+ ", Count:" + result.Count);
            if (this.gatewayReceiveMsg != null) {
              
                this.gatewayReceiveMsg(readBuff);
            }
            
        }

    }
    public async void SendMsg(byte[] content)
    {
        //var sendData = new ArraySegment<byte>(Encoding.UTF8.GetBytes("hello"));
        await clientWebSocket.SendAsync(content, WebSocketMessageType.Binary, true, ct); //发送数据
    }

    public void throwExpception(string errInfo)
    {
        this.log.Err(errInfo);
        throw new Exception(errInfo);
    }
}

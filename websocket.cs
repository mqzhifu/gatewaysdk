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




public class Websocket
{
    public GatewayConfig gatewayConfig;

    public ClientWebSocket clientWebSocket;
    public CancellationToken ct;
    public Log log;
    public GatewayReceiveMsg gatewayReceiveMsg;
    public ConnSuccess connSuccessBack;
    public Websocket(GatewayConfig gatewayConfig, GatewayReceiveMsg callback, ConnSuccess back)
    {
        this.log = new Log(1, "Websocket  ");
        if (gatewayConfig.outIp == "" || gatewayConfig.wsPort == "" || gatewayConfig.wsUri == "")
        {
            this.log.Err("check gatewayConfig , outIp || wsPort || wsUri is empty~ ");
        }
        this.gatewayConfig = gatewayConfig;
        this.gatewayReceiveMsg = callback;
        this.connSuccessBack = back;
    }
    //初始化，连接后端服务器
    public async void Init()
    {
        string url = gatewayConfig.outIp + ":" + gatewayConfig.wsPort + gatewayConfig.wsUri;
        

        this.clientWebSocket = new ClientWebSocket();
        this.ct = new CancellationToken();

        this.log.Info("ws url:" + url + " ConnectAsync......");
        await clientWebSocket.ConnectAsync(new Uri("ws://" + url), ct);
        //Debug.Log(clientWebSocket.State);
        if (clientWebSocket.State != WebSocketState.Open)
        {
            this.log.Err("clientWebSocket.State != Open");
            return;
        }
        this.log.Info("ws connect success.");
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
            this.gatewayReceiveMsg(readBuff);
        }

    }
    public async void SendMsg(byte[] content)
    {
        //var sendData = new ArraySegment<byte>(Encoding.UTF8.GetBytes("hello"));
        await clientWebSocket.SendAsync(content, WebSocketMessageType.Binary, true, ct); //发送数据
    }

}

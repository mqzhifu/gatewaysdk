using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class Tcp
{
    public GatewayConfig gatewayConfig;
    public GatewayReceiveMsg gatewayReceiveMsg;
    public Log log;
    public ConnSuccess connSuccessBack;
    public System.Net.Sockets.TcpClient tcpClient;
    public Tcp(GatewayConfig gatewayConfig, GatewayReceiveMsg callback, ConnSuccess back, int logLevel)
    {
        this.log = new Log(logLevel, "Tcp  ");

        this.gatewayConfig      = gatewayConfig;
        this.gatewayReceiveMsg  = callback;
        this.connSuccessBack    = back;
    }

    public void Init()
    {        
        string dns = gatewayConfig.outIp + ":" + gatewayConfig.tcpPort;
        this.log.Info("dns" + dns);

        this.tcpClient = new System.Net.Sockets.TcpClient();
        this.tcpClient.Connect(IPAddress.Parse(gatewayConfig.outIp), int.Parse(gatewayConfig.tcpPort));
        this.connSuccessBack();
        this.StartRead();

    }

    public void StartRead()
    {
        byte[] buffer = new byte[1024 * 10];
        tcpClient.GetStream().BeginRead(buffer, 0, buffer.Length, this.Receive, null);
    }

    public void Receive(IAsyncResult ar)
    {
        var len = this.tcpClient.GetStream().EndRead(ar);
        if (len < 1)
        {
            this.StartRead();
            return;
        }
        byte[] buffer = new byte[1024 * 10];
        var msgByte = Util.ByteSubstr(buffer,0, len);
        //string str = Encoding.UTF8.GetString(buffer, 0, len);
        //doing something
        this.gatewayReceiveMsg(msgByte);
        this.StartRead();

    }

    public void SendMsg(byte[] msgBytes)
    {
        tcpClient.GetStream().BeginWrite(msgBytes, 0, msgBytes.Length, (ar) => {
            tcpClient.GetStream().EndWrite(ar);//结束异步发送
        }, null);
    }
}

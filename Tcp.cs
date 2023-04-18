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
    public ReceiveMsgCallback gatewayReceiveMsg;
    public Log log;
    public ConnectCallback connectCallback;
    public System.Net.Sockets.TcpClient tcpClient;
    public int state;             //当前连接状态

    public Tcp(GatewayConfig gatewayConfig, ReceiveMsgCallback callback, ConnectCallback back, int logLevel)
    {
        this.state = (int)Gateway.CONN_STATE.INIT;
        this.log = new Log(logLevel, "Tcp  ");

        this.gatewayConfig      = gatewayConfig;
        this.gatewayReceiveMsg  = callback;
        this.connectCallback = back;
    }

    public void Init()
    {        
        string dns = gatewayConfig.outIp + ":" + gatewayConfig.tcpPort;
        this.log.Info("dns" + dns);

        this.state = (int)Gateway.CONN_STATE.ING;

        this.tcpClient = new System.Net.Sockets.TcpClient();
        try
        {
            this.tcpClient.Connect(IPAddress.Parse(gatewayConfig.outIp), int.Parse(gatewayConfig.tcpPort));
            this.connectCallback((int)Gateway.CONN_STATE.SUCCESS, "SUCCESS");
            this.StartRead();
        }
        catch (Exception e)
        {
            this.state = (int)Gateway.CONN_STATE.FAILED;
            this.connectCallback((int)Gateway.CONN_STATE.FAILED, e.Message);
        }               
      
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
    public void Close()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class Test
{
    public Log log;
    public Controller controller;
    public Test(int logLevel)
    {
        this.log = new Log(logLevel, "core");
        this.log.Info("start:");
        this.controller = new Controller(logLevel);
        this.UseHttp();
        this.UseGateway();
        this.log.Info("finish.");
    }
    //长连接，接收S端推送的公共消息
    public void receiveMsg(GatewayMsg msg)
    {

    }
    public void UseHttp()
    {
        //this.TestUseHttp();
        //return;
        //正常登陆
        this.controller.UseHttp("http://xxxx", "11", "6", "", "", "");
    }

    public void UseGateway()
    {
        //this.TestUseGateway();
        //return;

        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.PROTOBUF, (int)Gateway.PROTOCOL_TYPE.TCP, null);
        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.PROTOBUF, (int)Gateway.PROTOCOL_TYPE.WS, null);

        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, (int)Gateway.PROTOCOL_TYPE.TCP, null);
        this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, (int)Gateway.PROTOCOL_TYPE.WS, this.receiveMsg,null,null);
    }

    public void TestUseGateway()
    {
        //参数错误
        //this.controller.UseGateway(3, (int)Gateway.PROTOCOL_TYPE.TCP, null);
        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, 4, null);
    }

    public void TestUseHttp()
    {

    }


    public int TestLoginFailed()
    {
        return -1;
    }

    //用于测试，方便初始化一些基础信息
    public void HttpUtil()
    {
        //this.serverHttpDns = "http://8.142.177.235:2222/";
        //this.access = "imzgoframe";
        //this.projectId = "6";
        //this.sourceType = "11";
    }


}

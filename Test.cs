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
        this.log = new Log(logLevel, "Test");
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
        this.controller.UseHttp("http://127.0.0.1:1111/", "11", "6", "imzgoframe", "frame_sync_1", "123456");
        //this.controller.UseHttp("http://192.168.103.124:1111/", "11", "6", "imzgoframe", "frame_sync_1", "123456");
        //this.controller.UseHttp("http://8.142.177.235:3333/", "11", "6", "imzgoframe", "frame_sync_1", "123456");
        //this.TestUseHttp();
        //return;
        //正常登陆
    }

    public void UseGateway()
    {

        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.PROTOBUF, (int)Gateway.PROTOCOL_TYPE.TCP, null);//ROTOBUF+TCP
        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.PROTOBUF, (int)Gateway.PROTOCOL_TYPE.WS, null);//ROTOBUF+WS

        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, (int)Gateway.PROTOCOL_TYPE.TCP, null);//JSON+TCP
        this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, (int)Gateway.PROTOCOL_TYPE.WS, this.receiveMsg,null,null);//JSON+WS
    }

    public void TestUseGateway()
    {
        //参数错误
        //this.controller.UseGateway(3, (int)Gateway.PROTOCOL_TYPE.TCP, null);
        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, 4, null);
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

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

    void UseHttp()
    {
        //this.TestUseHttp();
        //return;
        //正常登陆
        this.controller.UseHttp("http://8.142.177.235:2222/", "11", "6", "imzgoframe", "frame_sync_1", "123456");
    }

    void UseGateway()
    {
        //this.TestUseGateway();
        //return;

        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.PROTOBUF, (int)Gateway.PROTOCOL_TYPE.TCP, null);
        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.PROTOBUF, (int)Gateway.PROTOCOL_TYPE.WS, null);

        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, (int)Gateway.PROTOCOL_TYPE.TCP, null);
        this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, (int)Gateway.PROTOCOL_TYPE.WS, null);
    }

    void TestUseGateway()
    {
        //参数错误
        //this.controller.UseGateway(3, (int)Gateway.PROTOCOL_TYPE.TCP, null);
        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, 4, null);
    }

    void TestUseHttp()
    {
        //测试参数为空
        //this.controller.UseHttp("", "11", "6", "imzgoframe", "", "123456");
        //测试登陆失败
        //this.controller.UseHttp("http://8.142.177.235:2222/", "11", "6", "imzgoframe", "1111", "44444");
    }


}

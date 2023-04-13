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

//测试：UNITY 入口
using Debug = UnityEngine.Debug;
using System;

public delegate void HttpCallback();

public class Core : MonoBehaviour
{
    public Log          log;
    public Controller   controller;
   
    void Start()
    {
        var logLevel = 15;
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
        //this.controller.UseGateway((int)Gateway.CONTENT_TYPE.PROTOBUF, (int)Gateway.PROTOCOL_TYPE.WS, null);
        this.controller.UseGateway((int)Gateway.CONTENT_TYPE.JSON, (int)Gateway.PROTOCOL_TYPE.TCP, null);
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

    void Update()
    {

    }
}

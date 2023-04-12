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


using Debug = UnityEngine.Debug;
using System;

public delegate void HttpCallback();

public class Core : MonoBehaviour
{
    public Log          log;
    public Controller   controller;
   
    void Start()
    {
        this.log = new Log(1,"core");
        this.log.Info("start:");
        this.controller = new Controller();
        this.controller.UseHttp("http://8.142.177.235:2222/", "11", "6", "imzgoframe", "frame_sync_1", "123456");
        this.controller.UseGateway((int)Gateway.CONTENT_TYPE.PROTOBUF, (int)Gateway.PROTOCOL_TYPE.WS, null);
        this.log.Info("finish.");

    }

    void Update()
    {

    }
}

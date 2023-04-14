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
    void Start()
    {
        var logLevel = 15;
        var test = new Test(logLevel);
    }
    
    void Update()
    {

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

//入口文件
public class Controller 
{
    public Log log;
    public HttpUtil httpUtil;
    public Gateway gateway;
    public ApiLogicAwait apiLogicAwait;
    public ProtocolAction protocolAction;
    public int logLevel;
    public Controller(int logLevel)
    {
        this.log = new Log(logLevel, "Controller ");
        this.logLevel = logLevel;
    }

    public void UseHttp(string dns, string sourceType, string projectId, string access, string username, string password)
    {
        this.httpUtil = new HttpUtil(dns, sourceType, projectId, access,this.logLevel);
        this.apiLogicAwait = new ApiLogicAwait(this.httpUtil,this.logLevel);
        this.apiLogicAwait.LoginBlock(username, password);
    }

    public void HttpAsyncLogin()
    {
        //int retryTime = 0;//超时时间
        //while (this.apiLogicAwait.initStatus == (int)ApiLogicAwait.API_LOGIC_INIT_STATUS.PROCESSING)
        //{
        //    if (retryTime > this.apiLogicAwait.initTimeout * 1000)
        //    {
        //        this.log.Err("retryTime timeout. initTimeout:" + this.apiLogicAwait.initTimeout);
        //        break;
        //    }
        //    Task.Delay(100);
        //    retryTime += 100;
        //}

        //if (this.apiLogicAwait.initStatus != (int)ApiLogicAwait.API_LOGIC_INIT_STATUS.SUCCESS)
        //{
        //    this.log.Err("apiLogicAwait.InitStatus ");
        //    return;
        //}
    }
    //使用网关组件，它强依赖 apiLogicAwait ，且 apiLogicAwait 要登陆成功
    public void UseGateway(int contentType, int protocolType, BackMsg backMsg, ConnectCallback connectCallback, FDExceptionCallback fdExceptionCallback)
    {
        if (this.httpUtil == null)
        {
            this.throwException("httpUtil null");
        }
        // apiLogicAwait 要登陆成功
        //if (this.apiLogicAwait.initStatus != (int)ApiLogicAwait.API_LOGIC_INIT_STATUS.SUCCESS)
        //{
        //    this.throwExpception("ApiLogicAwait.API_LOGIC_INIT_STATUS != SUCCESS");
        //}
        if (this.apiLogicAwait.userToken == "" || this.apiLogicAwait.userId == 0)
        {
            this.throwException("userToken or userId empty");
        }

        this.protocolAction = new ProtocolAction(this.logLevel);//创建长连接自定义协议管理类
        this.apiLogicAwait.SetProtocolAction(this.protocolAction);
        //请求 HTTP 获取后端的配置数据，这里最好捕获一下异常。
        this.apiLogicAwait.InitGateway();

        this.gateway = new Gateway(this.logLevel);
        //这里最好捕获一下异常，比如：创建连接失败
        this.gateway.Init(contentType, protocolType, this.apiLogicAwait.gatewayConfig, this.protocolAction, this.apiLogicAwait.userToken, backMsg,connectCallback,fdExceptionCallback);
    }
    //
    public void throwException(string errInfo)
    {
        this.log.Err(errInfo);
        throw new Exception(errInfo);
    }
}

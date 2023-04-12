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
    //使用 http 组件 
    public void UseHttp(string dns, string sourceType, string projectId, string access, string username, string password)
    {
        this.httpUtil = new HttpUtil(dns, sourceType, projectId, access,this.logLevel);
        this.apiLogicAwait = new ApiLogicAwait(this.httpUtil,this.logLevel);
        this.apiLogicAwait.SetUserInfo(username,password);
        this.apiLogicAwait.InitLogin();
        int retryTime = 0;//超时时间
        while (this.apiLogicAwait.initStatus == (int)ApiLogicAwait.API_LOGIC_INIT_STATUS.PROCESSING)
        {
            if (retryTime > this.apiLogicAwait.initTimeout * 1000)
            {
                this.log.Err("retryTime timeout. initTimeout:" + this.apiLogicAwait.initTimeout);
                break;
            }
            Task.Delay(100);
            retryTime += 100;
        }

        if (this.apiLogicAwait.initStatus != (int)ApiLogicAwait.API_LOGIC_INIT_STATUS.SUCCESS)
        {
            this.log.Err("apiLogicAwait.InitStatus ");
            return;
        }
    }
    //使用网关组件
    public void UseGateway(int contentType, int protocolType, BackMsg backMsg)
    {
        if (this.httpUtil == null)
        {
            this.throwExpception("httpUtil null");
        }

        if (this.apiLogicAwait.initStatus != (int)ApiLogicAwait.API_LOGIC_INIT_STATUS.SUCCESS)
        {
            this.throwExpception("ApiLogicAwait.API_LOGIC_INIT_STATUS != SUCCESS");
        }

        this.protocolAction = new ProtocolAction(this.logLevel);
        this.apiLogicAwait.SetProtocolAction(this.protocolAction);
        this.apiLogicAwait.InitGateway();

        this.gateway = new Gateway(this.logLevel);
        //try
        //{
        //    this.gateway.Init(contentType, protocolType,this.apiLogicAwait.gatewayConfig, this.protocolAction,this.apiLogicAwait.userToken,backMsg);
        //}
        //catch (Exception e)
        //{
        //    Debug.Log("=======" + e.Message);
        //    this.log.Err(e.Message);
        //}
        this.gateway.Init(contentType, protocolType, this.apiLogicAwait.gatewayConfig, this.protocolAction, this.apiLogicAwait.userToken, backMsg);
    }

    public void throwExpception(string errInfo)
    {
        this.log.Err(errInfo);
        throw new Exception(errInfo);
    }
}

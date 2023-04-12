using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Controller 
{
    public Log log;
    public HttpUtil httpUtil;
    public Gateway gateway;
    public ApiLogicAwait apiLogicAwait;
    public ProtocolAction protocolAction;

    public Controller()
    {
        this.log = new Log(1, "Controller ");
    }

    public void UseHttp(string dns, string sourceType, string projectId, string access, string username, string password)
    {
        this.httpUtil = new HttpUtil(dns, sourceType, projectId, access);
        this.apiLogicAwait = new ApiLogicAwait(this.httpUtil);
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

        this.protocolAction = new ProtocolAction();
        this.apiLogicAwait.SetProtocolAction(this.protocolAction);
        this.apiLogicAwait.InitGateway();

        this.gateway = new Gateway();
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

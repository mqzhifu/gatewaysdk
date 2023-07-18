using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void ImDelegate();

public class TestDelegation
{
    public event ImDelegate  AAA;
    public Action BBB;
    public Action<int> CCC;
    public event Func<string, int> DDD;


    public TestDelegation()
    {

        this.AAA = func1;
        //this.AAA();

        this.AAA += func2;
        this.AAA();

        this.BBB = func1;
        this.CCC = func3;

        this.DDD = func4;
        this.DDD += func5;

        //this.DDD("77777");
        DDD = (string x) =>{
            Debug.Log("im Lambda:"+x);
            return 1;
        };

        DDD("xxxx");
    }

    public void func1()
    {
        Debug.Log("im func1");
    }

    public void func2()
    {
        Debug.Log("im func2");
    }

    public void func3(int aaaa)
    {
        Debug.Log("im func3");
    }

    public int func4(string aaa)
    {
        Debug.Log("im func4 :"+aaa);
        return 1;
    }

    public int func5(string bbb)
    {
        Debug.Log("im func5 :" + bbb);
        return 1;
    }
}

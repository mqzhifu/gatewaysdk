using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    //public static int debug;
    //public static void SetDebug(int debugFlag)
    //{
    //    debug = debugFlag;
    //}

    //public static void Log(string info)
    //{
    //    if (debug == 1)
    //    {
    //        Debug.Log(info);
    //    }
        
    //}


    public static long GetTimestamp()
    {
        TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);//ToUniversalTime()转换为标准时区的时间,去掉的话直接就用北京时间
        return (long)ts.TotalMilliseconds; //精确到毫秒
        //return (long)ts.TotalSeconds;//获取10位
    }

    public static byte[] ByteSubstr(byte[] str, int start, int subNumber)
    {
        int end = start + subNumber - 1;
        byte[] rs = new byte[subNumber];
        int inc = 0;
        //Debug.Log("ByteSubstr start："+ start+ " , subNumber:"+ subNumber + " end:"+ end);
        for (int i = start; i <= end; i++)
        {
            rs[inc] = str[i];
            inc++;
        }

        return rs;
    }


    public static byte[] IntToBytes(int value)
    {
        byte[] src = new byte[4];
        //src[3] = (byte)((value >> 24) & 0xFF);
        //src[2] = (byte)((value >> 16) & 0xFF);
        //src[1] = (byte)((value >> 8) & 0xFF);
        //src[0] = (byte)(value & 0xFF);

        src[0] = (byte)((value >> 24) & 0xFF);
        src[1] = (byte)((value >> 16) & 0xFF);
        src[2] = (byte)((value >> 8) & 0xFF);
        src[3] = (byte)(value & 0xFF);


        //Debug.Log("value:" + value + " , "+ src[0] +" , " +src[1]+" , "+ src[2]+ " , "+ src[3]);

        return src;
    }

    public static byte[] Int16ByteReversal(byte[] b)
    {
        var n = new byte[2];
        n[0] = b[1];
        n[1] = b[0];

        return n;
    }


    public static byte[] IntByteReversal(byte[] b)
    {
        var n = new byte[4];
        n[0] = b[3];
        n[1] = b[2];
        n[2] = b[1];
        n[3] = b[0];

        return n;
    }


    public static int BytesToInt(byte[] src, int offset)
    {
        int value;
        value = (int)((src[offset] & 0xFF)
                | ((src[offset + 1] & 0xFF) << 8)
                | ((src[offset + 2] & 0xFF) << 16)
                | ((src[offset + 3] & 0xFF) << 24));
        return value;
    }

    public static byte[] Byte4Tobyte1(byte[] c)
    {
        byte[] rs = new byte[1];
        rs[0] = c[3];
        return rs;

    }

    public static byte[] Byte4Tobyte2(byte[] c)
    {
        byte[] rs = new byte[2];
        rs[0] = c[2];
        rs[1] = c[3];
        return rs;

    }

    public static byte[] Copybyte(byte[] a, byte[] b)
    {
        //Debug.Log("copybyte:" + a + "," + b);
        byte[] c = new byte[a.Length + b.Length];
        a.CopyTo(c, 0);
        b.CopyTo(c, a.Length);
        return c;
    }

    public static void PrintBytesArray(byte[] byteArr)
    {
        Debug.Log("PrintBytesArray  len:" + byteArr.Length);
        for (int i = 0; i < byteArr.Length; i++)
        {
            Debug.Log("i:" + i + " , " + byteArr[i] + " ,");
        }
        //Debug.Log("===========PrintBytesArray=========End====");
    }

    public static void PrintBytesArrayByRange(byte[] byteArr, int start, int end)
    {
        Debug.Log("PrintBytesArray  len:" + byteArr.Length);
        for (int i = start; i < end; i++)
        {
            Debug.Log("i:" + i + " , " + byteArr[i] + " ,");
        }
        //Debug.Log("===========PrintBytesArray=========End====");
    }

    //public static long ConvertDateTimeToInt(System.DateTime time)
    //{

    //    System.DateTime startTime = TimeZoneInfo.ConvertTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0), TimeZoneInfo.Local);
    //    long t = (time.Ticks - startTime.Ticks) / 10000;  //除10000调整为13位   
    //    return t;
    //}
}

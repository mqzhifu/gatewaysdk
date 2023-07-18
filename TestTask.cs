using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTask
{
    public TestTask()
    {
        //RunTask();
        //TestAsync();
        //TestAwait();
        //var ie = yieldFun();
        //foreach (string value in ie)
        //{
        //    Debug.Log(value);
        //}
        this.BasicTask();
    }

    int TaskDoing(string caller )
    {
        Debug.Log("TaskDoing start..."+ " CurrentProcessorId: " + Thread.GetCurrentProcessorId() + " caller:"+caller);
        Thread.Sleep(2000);
        Debug.Log("TaskDoing finish.");

        return 1;
    }

    public void BasicTask()
    {
        Task myTask1 = new Task(()=> { this.TaskDoing("task1"); });
        Task myTask2 = new Task(() => { this.TaskDoing("task2"); });

        myTask1.Start();
        myTask2.Start();




        //myTask1.ContinueWith(

        //    (myTask1) => { PrintTaskInfo(myTask1); }
        //    );
        //Task.WaitAll(myTask1, myTask2);


    }

    public void TaskBackInfo()
    {
        Task<int> myTask1 = new Task(() => { this.TaskDoing("task1"); });
    }

    public void PrintTaskInfo(Task t)
    {
        Debug.Log("PrintTaskInfo IsCanceled:" + t.IsCanceled+ " IsCompleted" + t.IsCompleted + " Status:" + t.Status);
    }

    public void RunTask()
    {
        Debug.Log(Thread.CurrentThread.ManagedThreadId);
        Task.Run(() => {

            Thread.Sleep(5000);
            Debug.Log(Thread.CurrentThread.ManagedThreadId);

        });
    }

    public void TestAsync()
    {
        Debug.Log("TestAsync start:");
        Debug.Log(Thread.CurrentThread.ManagedThreadId);
        AsyncFunc();
        Debug.Log("TestAsync finish.");

    }

    public async void AsyncFunc()
    {
        await Task.Delay(5000);
        Debug.Log(Thread.CurrentThread.ManagedThreadId);
    }

    public void TestAwait()
    {
        Debug.Log("start");
        Run();
        Debug.Log("end");
    }

    public async void Run()
    {
        //while (true)
        //{
        await Task.Delay(1000);
        //}



        Debug.Log("Run finish.");
    }

    public IEnumerable yieldFun()
    {
        yield return "aaaa";
        yield return "bbbbb";
        //yield return new WaitForSeconds(10);
        yield return "cccc";
    }
}

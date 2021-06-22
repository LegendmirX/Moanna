using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{
    static ThreadedDataRequester instance;
    Queue<ThreadInfo> DataQueue = new Queue<ThreadInfo>();

    public void SetUp()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();
    }

    private void Update()
    {
        if (DataQueue.Count > 0)
        {
            for (int i = 0; i < DataQueue.Count; i++)
            {
                ThreadInfo threadInfo = DataQueue.Dequeue();
                threadInfo.callBack(threadInfo.parameter);
            }
        }
    }

    public static void RequestData(Func<object> generateDataFunc, Action<object> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.DataThread(generateDataFunc, callback);
        };

        new Thread(threadStart).Start();
    }

    private void DataThread(Func<object> generateDataFunc, Action<object> callback)
    {
        object data = generateDataFunc();
        lock (DataQueue)
        {
            DataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    private struct ThreadInfo
    {
        public readonly Action<object> callBack;
        public readonly object parameter;

        public ThreadInfo(Action<object> callBack, object parameter)
        {
            this.callBack = callBack;
            this.parameter = parameter;
        }
    }
}

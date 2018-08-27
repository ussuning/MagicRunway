using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void HandleEvent(object [] param);

public class EventMsgDispatcher
{
    private static Dictionary<int, List<HandleEvent>> msgHandlerDic;

    private static EventMsgDispatcher mInstance = null;
    public static EventMsgDispatcher Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new EventMsgDispatcher();

                msgHandlerDic = new Dictionary<int, List<HandleEvent>>();
            }
            return mInstance;
        }
    }

    public void registerEvent(int msgID, HandleEvent observer)
    {
        if (observer == null)
        {
            return;
        }

        List<HandleEvent> observerList;
        if (msgHandlerDic.TryGetValue(msgID, out observerList) == false)
        {
            msgHandlerDic[msgID] = new List<HandleEvent>();

            if (msgHandlerDic.TryGetValue(msgID, out observerList) == false)
            {
                return;
            }
        }

        if (observerList.Contains(observer))
        {
            Debug.LogError("Msg Oberver has already registered! " + msgID + "   " + observer);
            return;
        }

        observerList.Add(observer);
    }

    public void unRegisterEvent(int msgId, HandleEvent observer)
    {
        if (observer == null)
        {
            return;
        }

        List<HandleEvent> observerList;
        foreach (ushort cmd in msgHandlerDic.Keys)
        {
            if (msgHandlerDic.TryGetValue(cmd, out observerList) && observerList.Contains(observer))
            {
                observerList.Remove(observer);
            }
        }
    }

    public void TriggerEvent(int msgID, object [] param = null)
    {
        DispatchMsg(msgID, param);
    }

    private void DispatchMsg(int msgID, object [] param)
    {
        List<HandleEvent> obList;
        if (msgHandlerDic.TryGetValue(msgID, out obList))
        {
            for (int i = 0; i < obList.Count; i++)
            {
                obList[i](param);
            }
        }
    }
}

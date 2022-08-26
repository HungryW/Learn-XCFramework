using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    /// <summary>
    /// 主要数据是 事件注册信息 和 发送的事件队列
    /// 主要接口 注册和解注册事件 发送事件 处理事件是在注册信息中找到回调列表,调用回调
    /// </summary>
    public partial class CEventPool<T> where T : CEventArgsBase
    {
        private readonly Dictionary<int, LinkedList<EventHandler<T>>> m_mapEventHandler;
        private readonly Queue<CEvent> m_qEvents;
        private EventHandler<T> m_DefaultHandler;

        public CEventPool()
        {
            m_mapEventHandler = new Dictionary<int, LinkedList<EventHandler<T>>>();
            m_qEvents = new Queue<CEvent>();
            m_DefaultHandler = null;
        }

        public void Subscribe(int a_nId, EventHandler<T> a_handler)
        {
            LinkedList<EventHandler<T>> listHandler = null;
            if (!m_mapEventHandler.TryGetValue(a_nId, out listHandler))
            {
                listHandler = new LinkedList<EventHandler<T>>();
                m_mapEventHandler.Add(a_nId, listHandler);
            }
            listHandler.AddLast(a_handler);
        }

        public void UnSubscribe(int a_nId, EventHandler<T> a_handler)
        {
            LinkedList<EventHandler<T>> listHandler = null;
            if (m_mapEventHandler.TryGetValue(a_nId, out listHandler))
            {
                listHandler.Remove(a_handler);
            }
        }

        public void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            lock (m_qEvents)
            {
                while (m_qEvents.Count > 0)
                {
                    CEvent eventNode = m_qEvents.Dequeue();
                    _HandleEvent(eventNode.Sender, eventNode.EventArgs);
                    CReferencePool.Release(eventNode);
                }
            }
        }

        public void Fire(object a_oSender, T a_args)
        {
            if (a_args == null)
            {
                throw new Exception("Event is invalid");
            }

            CEvent eventNode = CEvent.Create(a_oSender, a_args);
            lock (m_qEvents)
            {
                m_qEvents.Enqueue(eventNode);
            }
        }

        public void FireNow(object a_oSender, T a_args)
        {
            if (a_args == null)
            {
                throw new Exception("Event is invalid");
            }
            _HandleEvent(a_oSender, a_args);
        }

        private void _HandleEvent(object a_oSender, T a_arg)
        {
            LinkedList<EventHandler<T>> listHandler = null;
            if (m_mapEventHandler.TryGetValue(a_arg.Id, out listHandler))
            {
                LinkedListNode<EventHandler<T>> handlerNode = listHandler.First;
                while (handlerNode != null)
                {
                    LinkedListNode<EventHandler<T>> next = handlerNode.Next;
                    handlerNode.Value.Invoke(a_oSender, a_arg);
                    handlerNode = next;
                }
            }
            else if (m_DefaultHandler != null)
            {
                m_DefaultHandler.Invoke(a_oSender, a_arg);
            }

            CReferencePool.Release(a_arg);
        }
    }
}

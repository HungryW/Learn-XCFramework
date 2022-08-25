using System;

namespace XCFrameworkBase
{
    public partial class CEventPool<T> where T : CEventArgsBase
    {
        /// <summary>
        /// 每次发送的事件节点的定义
        /// 
        /// 其他知识
        /// 在static方法中,可以访问对象的私有成员
        /// 
        /// 分清楚 数据依赖 还是 表现依赖
        /// 数据依赖 要求数据的生命周期要长,操作流程要确定
        /// 表现依赖 大部分是读数据, 不用管元数据的是什么样,不确定生命周期且必要的时候可以复制一份数据
        /// </summary>
        private sealed class CEvent : IReference
        {
            private object m_oSender;
            private T m_EventArgs;

            public CEvent()
            {
                m_oSender = null;
                m_EventArgs = null;
            }

            public void Clear()
            {
                m_oSender = null;
                m_EventArgs = null;
            }

            public static CEvent Create(object a_oSender, T a_arg)
            {
                CEvent eventItem = CReferencePool.Acquire<CEvent>();
                eventItem.m_oSender = a_oSender;
                eventItem.m_EventArgs = a_arg;
                return eventItem;
            }

            public object Sender
            {
                get
                {
                    return m_oSender;
                }
            }

            public T EventArgs
            {
                get
                {
                    return m_EventArgs;
                }
            }
        }
    }
}

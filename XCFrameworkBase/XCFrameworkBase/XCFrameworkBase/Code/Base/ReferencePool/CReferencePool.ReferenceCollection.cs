using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public static partial class CReferencePool
    {
        /// <summary>
        /// 对象引用池,维护的是一个空闲对象的队列
        /// 主要提供 Acquire 和 Release 接口
        /// Acquire 当空闲队列不足时,new 一个新的返回
        /// Release 清理对象 插入到空闲队列中
        /// 一个对象在同一时间只能被获取一次
        /// 
        /// 问题
        /// 为什么 容器Queue,不是 stack,先进先出的话是不是cache的命中率会高些
        /// 理论上不Release的话也不会造成内存泄漏,因为不存在全局的静态引用,导致垃圾回收清理不掉
        /// 
        /// 理解 
        /// 池子是维护回收后的(能再次使用的)对象的容器
        /// 是某个类型的空闲的实例对象的集合
        /// 
        /// 其他知识
        /// Queue是数组实现的循环队列,记录了head和tail的下标
        /// 可以在知道Type变量的情况下实例化对象 用Activator.CreateInstance(Type a_t),然后可以强转类型.
        /// typeof(T)可以获得泛型的Type变量, new T() 可以直接实例化对象,T可以当成类名来用
        /// </summary>
        private sealed class CReferenceCollection
        {
            private readonly Queue<IReference> m_qReferences;
            private readonly Type m_tReferenceType;
            private int m_nUsingReferenceCount;
            private int m_nAcquireReferenceCount;
            private int m_nReleaseReferenceCount;
            private int m_nAddReferenceCount;
            private int m_nRemoveReferenceCount;

            public CReferenceCollection(Type a_tReferenceType)
            {
                m_qReferences = new Queue<IReference>();
                m_tReferenceType = a_tReferenceType;
                m_nAcquireReferenceCount = 0;
                m_nAddReferenceCount = 0;
                m_nReleaseReferenceCount = 0;
                m_nRemoveReferenceCount = 0;
                m_nUsingReferenceCount = 0;
            }

            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != m_tReferenceType)
                {
                    throw new Exception("Type is Invalid");
                }
                m_nUsingReferenceCount++;
                m_nAcquireReferenceCount++;
                lock (m_qReferences)
                {
                    if (m_qReferences.Count > 0)
                    {
                        return (T)m_qReferences.Dequeue();
                    }
                }
                m_nAddReferenceCount++;
                return new T();
            }

            public IReference Acquire()
            {
                m_nUsingReferenceCount++;
                m_nAcquireReferenceCount++;
                lock (m_qReferences)
                {
                    if (m_qReferences.Count > 0)
                    {
                        return m_qReferences.Dequeue();
                    }
                }
                m_nAddReferenceCount++;
                return (IReference)Activator.CreateInstance(m_tReferenceType);
            }

            public void Release(IReference a_ref)
            {
                a_ref.Clear();
                lock (m_qReferences)
                {
                    m_qReferences.Enqueue(a_ref);
                }
                m_nReleaseReferenceCount++;
                m_nUsingReferenceCount--;
            }

            public void Add<T>(int a_nCount) where T : class, IReference, new()
            {
                if (typeof(T) != m_tReferenceType)
                {
                    throw new Exception("Type is Invalid");
                }

                lock (m_qReferences)
                {
                    m_nAddReferenceCount += a_nCount;
                    while (a_nCount-- > 0)
                    {
                        m_qReferences.Enqueue(new T());
                    }
                }
            }

            public void RemoveAll()
            {
                lock (m_qReferences)
                {
                    m_nRemoveReferenceCount += m_qReferences.Count;
                    m_qReferences.Clear();
                }
            }

            public Type ReferenceType
            {
                get
                {
                    return m_tReferenceType;
                }
            }

            public int UnusedReferenceCount
            {
                get
                {
                    return m_qReferences.Count;
                }
            }

            public int UsingReferenceCount
            {
                get
                {
                    return m_nUsingReferenceCount;
                }
            }

            public int AcquireReferenceCount
            {
                get
                {
                    return m_nAcquireReferenceCount;
                }
            }

            public int ReleaseReferenceCount
            {
                get
                {
                    return m_nReleaseReferenceCount;
                }
            }

            public int AddReferenceCount
            {
                get
                {
                    return m_nAddReferenceCount;
                }
            }

            public int RemoveReferenceCount
            {
                get
                {
                    return m_nRemoveReferenceCount;
                }
            }
        }
    }
}

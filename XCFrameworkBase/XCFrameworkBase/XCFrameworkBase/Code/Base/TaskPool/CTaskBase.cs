using System;

namespace XCFrameworkBase
{
    /// <summary>
    /// Task 定义了Task的数据:id 优先级 tag 是否完成 用户数据
    /// 没有这个Task要干什么的数据
    /// </summary>
    /// 
    public abstract class CTaskBase : IReference
    {
        public const int mc_nDefaultPriority = 0;

        private int m_nId;
        private string m_szTag;
        private int m_nPriority;
        private object m_oUserData;
        private bool m_bDone;

        public CTaskBase()
        {
            m_nId = 0;
            m_szTag = null;
            m_nPriority = mc_nDefaultPriority;
            m_bDone = false;
            m_oUserData = null;
        }

        public virtual void Clear()
        {
            m_nId = 0;
            m_szTag = null;
            m_nPriority = mc_nDefaultPriority;
            m_bDone = false;
            m_oUserData = null;
        }

        public void Initialize(int a_nId, string tag, int priority, object userData)
        {
            m_nId = a_nId;
            m_szTag = tag;
            m_nPriority = priority;
            m_oUserData = userData;
            m_bDone = false;
        }

        public bool Done
        {
            get
            {
                return m_bDone;
            }
            set
            {
                m_bDone = value;
            }
        }

        public int Id
        {
            get
            {
                return m_nId;
            }
        }


        /// <summary>
        /// 获取任务的标签。
        /// </summary>
        public string Tag
        {
            get
            {
                return m_szTag;
            }
        }

        /// <summary>
        /// 获取任务的优先级。
        /// </summary>
        public int Priority
        {
            get
            {
                return m_nPriority;
            }
        }

        /// <summary>
        /// 获取任务的用户自定义数据。
        /// </summary>
        public object UserData
        {
            get
            {
                return m_oUserData;
            }
        }


        /// <summary>
        /// 获取任务描述。
        /// </summary>
        public virtual string Description
        {
            get
            {
                return null;
            }
        }

    }
}

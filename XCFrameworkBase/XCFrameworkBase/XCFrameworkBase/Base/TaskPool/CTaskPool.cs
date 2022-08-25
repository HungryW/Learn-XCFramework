using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed class CTaskPool<T> where T : CTaskBase
    {
        private readonly Stack<ITaskAgent<T>> m_stackFreeAgents;
        private readonly LinkedList<ITaskAgent<T>> m_listWorkingAgents;
        private readonly LinkedList<T> m_listWaitTasks;
        private bool m_bPaused;

        public CTaskPool()
        {
            m_stackFreeAgents = new Stack<ITaskAgent<T>>();
            m_listWorkingAgents = new LinkedList<ITaskAgent<T>>();
            m_listWaitTasks = new LinkedList<T>();
            m_bPaused = false;
        }

        public bool Pasused
        {
            get
            {
                return m_bPaused;
            }
            set
            {
                m_bPaused = value;
            }
        }

        public int TotalAgent
        {
            get
            {
                return m_listWorkingAgents.Count + m_stackFreeAgents.Count;
            }
        }

        public void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            if (m_bPaused)
            {
                return;
            }
            _ProcessRunningTask(a_fElapseSed, a_fRealElapseSed);
            _ProcessWaitingTask(a_fElapseSed, a_fRealElapseSed);
        }

        private void _ProcessRunningTask(float a_fElapseSed, float a_fRealElapseSed)
        {
            LinkedListNode<ITaskAgent<T>> agentNode = m_listWorkingAgents.First;
            while (agentNode != null)
            {
                T task = agentNode.Value.Task;
                if (!task.Done)
                {
                    agentNode.Value.Update(a_fElapseSed, a_fRealElapseSed);
                    agentNode = agentNode.Next;
                    continue;
                }
                else
                {
                    LinkedListNode<ITaskAgent<T>> next = agentNode.Next;
                    m_listWorkingAgents.Remove(agentNode);
                    agentNode.Value.Reset();
                    m_stackFreeAgents.Push(agentNode.Value);
                    CReferencePool.Release(task);
                    agentNode = next;
                }
            }
        }

        private void _ProcessWaitingTask(float a_fElapseSed, float a_fRealElapseSed)
        {
            LinkedListNode<T> taskNode = m_listWaitTasks.First;
            while (taskNode != null && m_stackFreeAgents.Count > 0)
            {
                ITaskAgent<T> agent = m_stackFreeAgents.Pop();
                LinkedListNode<ITaskAgent<T>> agentWorkingNode = m_listWorkingAgents.AddLast(agent);
                T task = taskNode.Value;
                LinkedListNode<T> nextTask = taskNode.Next;

                EStartTaskStatus status = agent.Start(task);
                if (status == EStartTaskStatus.Done || status == EStartTaskStatus.HasToWait || status == EStartTaskStatus.UnknownEror)
                {
                    agent.Reset();
                    m_stackFreeAgents.Push(agent);
                    m_listWorkingAgents.Remove(agentWorkingNode);
                }

                if (status == EStartTaskStatus.Done || status == EStartTaskStatus.CanResume || status == EStartTaskStatus.UnknownEror)
                {
                    m_listWaitTasks.Remove(taskNode);
                }

                if (status == EStartTaskStatus.Done || status == EStartTaskStatus.UnknownEror)
                {
                    CReferencePool.Release(task);
                }
                taskNode = nextTask;
            }
        }

        public void AddAgent(ITaskAgent<T> a_agent)
        {
            if (null == a_agent)
            {
                throw new Exception("Task agent is invalid");
            }
            a_agent.Init();
            m_stackFreeAgents.Push(a_agent);
        }

        public void AddTask(T a_task)
        {
            LinkedListNode<T> current = m_listWaitTasks.Last;
            while (current != null)
            {
                if (a_task.Priority <= current.Value.Priority)
                {
                    break;
                }
                current = current.Previous;
            }

            if (current != null)
            {
                m_listWaitTasks.AddAfter(current, a_task);
            }
            else
            {
                m_listWaitTasks.AddFirst(a_task);
            }
        }

        public bool RemoveTask(int a_nId)
        {
            LinkedListNode<T> taskWaitNode = m_listWaitTasks.First;
            while (null != taskWaitNode)
            {
                if (taskWaitNode.Value.Id == a_nId)
                {
                    m_listWaitTasks.Remove(taskWaitNode);
                    CReferencePool.Release(taskWaitNode.Value);
                    return true;
                }
                taskWaitNode = taskWaitNode.Next;
            }

            LinkedListNode<ITaskAgent<T>> agentDoingNode = m_listWorkingAgents.First;
            while (null != agentDoingNode)
            {
                T task = agentDoingNode.Value.Task;
                if (task.Id == a_nId)
                {
                    agentDoingNode.Value.Reset();
                    m_stackFreeAgents.Push(agentDoingNode.Value);
                    m_listWorkingAgents.Remove(agentDoingNode);
                    CReferencePool.Release(task);
                    return true;
                }
                agentDoingNode = agentDoingNode.Next;
            }

            return false;
        }

        public int RemoveTasks(string a_szTag)
        {
            int nCount = 0;
            LinkedListNode<T> taskNode = m_listWaitTasks.First;
            while (taskNode != null)
            {
                LinkedListNode<T> nextTaskNode = taskNode.Next;
                if (taskNode.Value.Tag == a_szTag)
                {
                    m_listWaitTasks.Remove(taskNode);
                    CReferencePool.Release(taskNode.Value);
                    nCount++;
                }
                taskNode = nextTaskNode;
            }

            LinkedListNode<ITaskAgent<T>> agentNode = m_listWorkingAgents.First;
            while (agentNode != null)
            {
                LinkedListNode<ITaskAgent<T>> nextagentNode = agentNode.Next;
                T task = agentNode.Value.Task;
                if (task.Tag == a_szTag)
                {
                    agentNode.Value.Reset();
                    m_stackFreeAgents.Push(agentNode.Value);
                    m_listWorkingAgents.Remove(agentNode);
                    CReferencePool.Release(task);
                    nCount++;
                }
                agentNode = nextagentNode;
            }

            return nCount;
        }

        public int RemoveAllTasks()
        {
            int nCount = m_listWaitTasks.Count + m_listWorkingAgents.Count;
            LinkedListNode<T> taskNode = m_listWaitTasks.First;
            while (taskNode != null)
            {
                LinkedListNode<T> nextTaskNode = taskNode.Next;
                CReferencePool.Release(taskNode.Value);
                taskNode = nextTaskNode;
            }
            m_listWaitTasks.Clear();

            LinkedListNode<ITaskAgent<T>> agentNode = m_listWorkingAgents.First;
            while (agentNode != null)
            {
                LinkedListNode<ITaskAgent<T>> nextNode = agentNode.Next;
                T task = agentNode.Value.Task;
                agentNode.Value.Reset();
                m_stackFreeAgents.Push(agentNode.Value);
                CReferencePool.Release(task);

                agentNode = nextNode;
            }
            m_listWorkingAgents.Clear();

            return nCount;
        }

        public void Shutdown()
        {
            RemoveAllTasks();
            while (m_stackFreeAgents.Count > 0)
            {
                m_stackFreeAgents.Pop().ShutDown();
            }
        }
    }
}

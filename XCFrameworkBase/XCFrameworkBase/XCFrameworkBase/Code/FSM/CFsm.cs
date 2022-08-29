using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed class CFsm<T> : CFsmBase, IReference, IFsm<T> where T : class
    {
        private T m_owner;
        private readonly Dictionary<Type, CFsmState<T>> m_mapStates;
        private Dictionary<string, CVariable> m_mapDatas;
        private CFsmState<T> m_curState;
        private float m_fCurStateTime;
        private bool m_bIsDestroyed;

        public CFsm()
        {
            m_owner = null;
            m_mapStates = new Dictionary<Type, CFsmState<T>>();
            m_mapDatas = null;
            m_curState = null;
            m_fCurStateTime = 0f;
            m_bIsDestroyed = true;
        }

        public void Clear()
        {
            _CleanStatesMap();
            _CleanDatas();
            m_owner = null;
            m_curState = null;
            m_fCurStateTime = 0;
            m_bIsDestroyed = true;
        }

        private void _CleanStatesMap()
        {
            if (m_curState != null)
            {
                m_curState.OnLeave(this, true);
            }

            foreach (var state in m_mapStates)
            {
                state.Value.OnDestroy(this);
            }
            m_mapStates.Clear();
        }

        private void _CleanDatas()
        {
            if (m_mapDatas != null)
            {
                foreach (var data in m_mapDatas)
                {
                    if (data.Value == null)
                    {
                        continue;
                    }
                    CReferencePool.Release(data.Value);
                }
                m_mapDatas.Clear();
            }
        }

        public void Start<TState>() where TState : CFsmState<T>
        {
            if (IsRunning)
            {
                return;
            }
            CFsmState<T> state = GetState<TState>();
            _Start(state);
        }

        public void Start(Type a_t)
        {
            if (IsRunning)
            {
                return;
            }
            if (!typeof(CFsmState<T>).IsAssignableFrom(a_t))
            {
                throw new Exception(CUtility.Text.Format("Fsm Can not Start, State type {0} is invalid", a_t.FullName));
            }
            CFsmState<T> state = GetState(a_t);
            _Start(state);
        }

        private void _Start(CFsmState<T> state)
        {
            if (null == state)
            {
                throw new Exception(" FSM Can not Start");
            }
            m_fCurStateTime = 0f;
            m_curState = state;
            m_curState.OnEnter(this);
        }


        public CFsmState<T> GetState(Type a_t)
        {
            if (a_t == null)
            {
                throw new Exception("State Type is invalid");
            }
            if (!typeof(CFsmState<T>).IsAssignableFrom(a_t))
            {
                throw new Exception("State Type is invalid");
            }

            CFsmState<T> state;
            m_mapStates.TryGetValue(a_t, out state);
            return state;
        }

        public TState GetState<TState>() where TState : CFsmState<T>
        {
            CFsmState<T> state = null;
            m_mapStates.TryGetValue(typeof(TState), out state);
            return (TState)state;
        }

        public bool HasState<TState>() where TState : CFsmState<T>
        {
            return m_mapStates.ContainsKey(typeof(TState));
        }

        public bool HasState(Type a_t)
        {
            if (null == a_t)
            {
                throw new Exception("State type is invalid");
            }
            if (!typeof(CFsmState<T>).IsAssignableFrom(a_t))
            {
                throw new Exception("state type is invalid");
            }

            return m_mapStates.ContainsKey(a_t);
        }

        public CFsmState<T>[] GetAllStates()
        {
            int nIdx = 0;
            CFsmState<T>[] arr = new CFsmState<T>[m_mapStates.Count];
            foreach (var state in m_mapStates)
            {
                arr[nIdx++] = state.Value;
            }
            return arr;
        }

        public bool HasData(string a_szName)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szName));
            if (m_mapDatas == null)
            {
                return false;
            }
            return m_mapDatas.ContainsKey(a_szName);
        }

        public TData GetData<TData>(string a_szName) where TData : CVariable
        {
            return (TData)GetData(a_szName);
        }

        public CVariable GetData(string a_szName)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szName));
            if (m_mapDatas == null)
            {
                return null;
            }
            CVariable data = null;
            m_mapDatas.TryGetValue(a_szName, out data);
            return data;
        }

        public void SetData<TData>(string a_szName, TData data) where TData : CVariable
        {
            SetData(a_szName, data);
        }

        public void SetData(string a_szName, CVariable data)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szName));
            if (m_mapDatas == null)
            {
                m_mapDatas = new Dictionary<string, CVariable>();
            }

            CVariable oldData = GetData(a_szName);
            if (oldData != null)
            {
                CReferencePool.Release(oldData);
            }
            m_mapDatas[a_szName] = data;
        }


        public bool RemoveData(string a_szName)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szName));
            if (m_mapDatas == null)
            {
                return false;
            }
            CVariable oldData = GetData(a_szName);
            if (oldData != null)
            {
                CReferencePool.Release(oldData);
            }
            return m_mapDatas.Remove(a_szName);
        }

        public override void Update(float elapseSed, float realElapseSed)
        {
            if (m_curState == null)
            {
                return;
            }
            m_curState.OnUpdate(this, elapseSed, realElapseSed);
        }

        public void ChangeState<TState>() where TState : CFsmState<T>
        {
            ChangeState(typeof(TState));
        }

        public void ChangeState(Type stateType)
        {
            if (m_curState == null)
            {
                throw new Exception("Current state is invalid");
            }
            CFsmState<T> state = GetState(stateType);
            if (state == null)
            {
                throw new Exception("State is invalid");
            }

            m_curState.OnLeave(this, false);
            m_fCurStateTime = 0;
            m_curState = state;
            m_curState.OnEnter(this);

        }

        public override void Shutdown()
        {
            CReferencePool.Release(this);
        }

        public static CFsm<T> Create(string a_szName, T owner, List<CFsmState<T>> a_listStates)
        {
            System.Diagnostics.Debug.Assert(owner != null);
            System.Diagnostics.Debug.Assert(a_listStates != null && a_listStates.Count > 0);

            CFsm<T> fsm = CReferencePool.Acquire<CFsm<T>>();
            fsm.SetName(a_szName);
            fsm.m_owner = owner;
            fsm.m_bIsDestroyed = false;
            foreach (var state in a_listStates)
            {
                System.Diagnostics.Debug.Assert(state != null);
                Type tState = state.GetType();
                System.Diagnostics.Debug.Assert(!fsm.m_mapStates.ContainsKey(tState));
                fsm.m_mapStates.Add(tState, state);
                state.OnInit(fsm);
            }

            return fsm;
        }

        public static CFsm<T> Create(string a_szName, T owner, params CFsmState<T>[] a_arrStates)
        {
            System.Diagnostics.Debug.Assert(owner != null);
            System.Diagnostics.Debug.Assert(a_arrStates != null && a_arrStates.Length > 0);

            CFsm<T> fsm = CReferencePool.Acquire<CFsm<T>>();
            fsm.SetName(a_szName);
            fsm.m_owner = owner;
            fsm.m_bIsDestroyed = false;
            foreach (var state in a_arrStates)
            {
                System.Diagnostics.Debug.Assert(state != null);
                Type tState = state.GetType();
                System.Diagnostics.Debug.Assert(!fsm.m_mapStates.ContainsKey(tState));
                fsm.m_mapStates.Add(tState, state);
                state.OnInit(fsm);
            }

            return fsm;
        }


        public T Owner
        {
            get
            {
                return m_owner;
            }
        }

        public override Type OwnerType
        {
            get
            {
                return typeof(T);
            }
        }

        public override bool IsRunning
        {
            get
            {
                return m_curState != null;
            }
        }

        public override bool IsDestoryed
        {
            get
            {
                return m_bIsDestroyed;
            }
        }

        public override int StateCount
        {
            get
            {
                return m_mapStates.Count;
            }
        }


        public float CurrentStateTime
        {
            get
            {
                return m_fCurStateTime;
            }
        }


        public CFsmState<T> CurState
        {
            get
            {
                return m_curState;
            }
        }

    }
}

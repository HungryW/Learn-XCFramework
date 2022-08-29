using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed class CFsmMgr : CGameFrameworkModule, IFsmMgr
    {
        private Dictionary<TypeNamePair, CFsmBase> m_mapFsms;

        public CFsmMgr()
        {
            m_mapFsms = new Dictionary<TypeNamePair, CFsmBase>();
        }

        public override int Priority => 1;
        public int Count => m_mapFsms.Count;

        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            if (m_mapFsms.Count <= 0)
            {
                return;
            }
            foreach (var fsm in m_mapFsms)
            {
                if (fsm.Value.IsDestoryed)
                {
                    continue;
                }
                fsm.Value.Update(a_fElapseSed, a_fRealElapseSed);
            }
        }

        public override void Shutdown()
        {
            foreach (var fsm in m_mapFsms)
            {
                fsm.Value.Shutdown();
            }
            m_mapFsms.Clear();
        }

        public bool HasFsm<T>() where T : class
        {
            return _HasFsm(new TypeNamePair(typeof(T)));
        }

        public bool HasFsm(Type ownerType)
        {
            return _HasFsm(new TypeNamePair(ownerType));
        }

        public bool HasFsm<T>(string name) where T : class
        {
            return _HasFsm(new TypeNamePair(typeof(T), name));
        }

        public bool HasFsm(Type ownerType, string name)
        {
            return _HasFsm(new TypeNamePair(ownerType, name));
        }

        private bool _HasFsm(TypeNamePair a_Name)
        {
            return m_mapFsms.ContainsKey(a_Name);
        }

        private CFsmBase _GetFsm(TypeNamePair a_Name)
        {
            CFsmBase fsm = null;
            m_mapFsms.TryGetValue(a_Name, out fsm);
            return fsm;
        }

        public IFsm<T> GetFsm<T>() where T : class
        {
            return (IFsm<T>)_GetFsm(new TypeNamePair(typeof(T)));
        }

        public CFsmBase GetFsm(Type ownerType)
        {
            return _GetFsm(new TypeNamePair(ownerType));
        }

        public IFsm<T> GetFsm<T>(string a_szName) where T : class
        {
            return (IFsm<T>)_GetFsm(new TypeNamePair(typeof(T), a_szName));
        }

        public CFsmBase GetFsm(Type ownerType, string a_szName)
        {
            return _GetFsm(new TypeNamePair(ownerType, a_szName));
        }

        public IFsm<T> CreateFsm<T>(T owner, params CFsmState<T>[] states) where T : class
        {
            return CreateFsm(string.Empty, owner, states);
        }

        public IFsm<T> CreateFsm<T>(string a_szName, T owner, params CFsmState<T>[] states) where T : class
        {
            TypeNamePair tName = new TypeNamePair(typeof(T), a_szName);
            if (_HasFsm(tName))
            {
                throw new Exception("Already exist fsm");
            }
            CFsm<T> fsm = CFsm<T>.Create(a_szName, owner, states);
            m_mapFsms.Add(tName, fsm);
            return fsm;
        }

        public IFsm<T> CreateFsm<T>(T owner, List<CFsmState<T>> states) where T : class
        {
            return CreateFsm<T>(string.Empty, owner, states);
        }

        public IFsm<T> CreateFsm<T>(string a_szName, T owner, List<CFsmState<T>> a_listStates) where T : class
        {
            TypeNamePair tName = new TypeNamePair(typeof(T), a_szName);
            if (_HasFsm(tName))
            {
                throw new Exception("Already exist Fsm");
            }
            CFsm<T> fsm = CFsm<T>.Create(a_szName, owner, a_listStates);
            m_mapFsms.Add(tName, fsm);
            return fsm;
        }

        public bool DestroyFsm<T>() where T : class
        {
            return _DestoryFsm(new TypeNamePair(typeof(T)));
        }

        public bool DestroyFsm(Type ownerType)
        {
            return _DestoryFsm(new TypeNamePair(ownerType));
        }

        public bool DestrotFsm<T>(string a_szName) where T : class
        {
            return _DestoryFsm(new TypeNamePair(typeof(T), a_szName));
        }

        public bool DestoryFsm(Type ownerType, string a_szName)
        {
            return _DestoryFsm(new TypeNamePair(ownerType, a_szName));
        }

        public bool DestoryFsm<T>(IFsm<T> fsm) where T : class
        {
            return _DestoryFsm(new TypeNamePair(typeof(T), fsm.Name));
        }

        public bool DestoryFsm(CFsmBase fsm)
        {
            return _DestoryFsm(new TypeNamePair(fsm.OwnerType, fsm.Name));
        }

        private bool _DestoryFsm(TypeNamePair a_tName)
        {
            CFsmBase fsm = null;
            if (m_mapFsms.TryGetValue(a_tName, out fsm))
            {
                fsm.Shutdown();
                return m_mapFsms.Remove(a_tName);
            }
            return false;
        }

        public void GetAllFsms(List<CFsmBase> results)
        {
            System.Diagnostics.Debug.Assert(results != null);
            results.Clear();
            foreach(var fsm in m_mapFsms)
            {
                results.Add(fsm.Value);
            }
        }
    }
}

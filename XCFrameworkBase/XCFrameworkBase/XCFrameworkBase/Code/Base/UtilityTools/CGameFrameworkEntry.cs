using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public class CGameFrameworkEntry
    {
        private static readonly LinkedList<CGameFrameworkModule> ms_listModules = new LinkedList<CGameFrameworkModule>();

        public static void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            LinkedListNode<CGameFrameworkModule> cur = ms_listModules.First;
            while (cur != null)
            {
                cur.Value.Update(a_fElapseSed, a_fRealElapseSed);
            }
            cur = cur.Next;
        }

        public static void Shutdowm()
        {
            LinkedListNode<CGameFrameworkModule> cur = ms_listModules.First;
            while (cur != null)
            {
                cur.Value.Shutdown();
            }

            ms_listModules.Clear();
            CReferencePool.ClearAll();
        }

        /// <summary>
        /// 可以获得不同行为的变量
        /// 存储相同基类的变量的容器(它们有相同基类是为了放在容器里管理)
        /// 每个单独实现不同接口,有不同的行为
        /// 接口名 可以通过一定的规则转化为 类型名
        /// 通过类型名的string字符,获得类型变量
        /// 根据类型变量是否相等找到子对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetModule<T>() where T : class
        {
            Type iType = typeof(T);
            if (!iType.IsInterface)
            {
                throw new GameFrameworkException(CUtility.Text.Format("You must get module by interface, but {0} is not", iType.FullName));
            }
            string szMoudleNme = CUtility.Text.Format("{0}.C{1}", iType.Namespace, iType.Name.Substring(1));
            Type tModlue = Type.GetType(szMoudleNme);
            if (null == tModlue)
            {
                throw new GameFrameworkException(CUtility.Text.Format("Can not find Game Framework module type '{0}'.", szMoudleNme));
            }
            return _GetMoudle(tModlue) as T;
        }

        private static CGameFrameworkModule _GetMoudle(Type a_t)
        {
            LinkedListNode<CGameFrameworkModule> cur = ms_listModules.First;
            while (cur != null)
            {
                if (cur.Value.GetType() == a_t)
                {
                    return cur.Value;
                }
                cur = cur.Next;
            }
            return _CreateMoudle(a_t);
        }

        private static CGameFrameworkModule _CreateMoudle(Type a_t)
        {
            CGameFrameworkModule module = (CGameFrameworkModule)Activator.CreateInstance(a_t);
            if (null == module)
            {
                throw new GameFrameworkException(CUtility.Text.Format("Create Module {0}", a_t.FullName));
            }

            LinkedListNode<CGameFrameworkModule> cur = ms_listModules.First;
            while (cur != null)
            {
                if (module.Priority > cur.Value.Priority)
                {
                    break;
                }
            }
            if (cur != null)
            {
                ms_listModules.AddBefore(cur, module);
            }
            else
            {
                ms_listModules.AddLast(module);
            }

            return module;
        }
    }
}

using System;

namespace XCFrameworkBase
{
    /// <summary>
    /// 知识点
    /// 泛型的类型名和基类名字一致没有问题,因为泛型的类型对象是开放类型,基类的类型对象是封闭类型
    /// 开放类型的泛型, 需要类型实参生成封闭类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CVariable<T> : CVariable
    {
        private T m_val;
        public CVariable()
        {
            m_val = default(T);
        }

        public override Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        public T Value
        {
            get
            {
                return m_val;
            }
            set
            {
                m_val = value;
            }
        }

        public override object GetValue()
        {
            return m_val;
        }

        public override void SetValue(object a_val)
        {
            m_val = (T)a_val;
        }

        public override void Clear()
        {
            m_val = default(T);
        }

        public override string ToString()
        {
            return (m_val != null) ? m_val.ToString() : "Null";
        }

    }
}

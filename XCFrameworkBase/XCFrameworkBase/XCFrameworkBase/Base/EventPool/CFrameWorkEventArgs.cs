using System;

namespace XCFrameworkBase
{
    public abstract class CFrameWorkEventArgs : EventArgs, IReference
    {
        public CFrameWorkEventArgs()
        {

        }

        public abstract void Clear();
    }
}

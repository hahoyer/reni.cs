using System;
using System.Collections.Generic;
using System.Linq;

namespace ReniUI
{
    public interface IStudioApplication: IApplication
    {
        void Exit();
        void New();
    }
}
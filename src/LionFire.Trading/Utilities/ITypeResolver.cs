﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ITypeResolver
    {
        Type GetTemplateType(string type);
        Type GetType(string type);
        IAccount CreateAccount(string name);
    }
}

﻿using LionFire.Applications;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting.Optimization
{
    public class TOptimizationTask : IHierarchicalTemplate, ITemplate<OptimizationTask>
    {
        public List<ITemplate> Children { get; set; }
    }

    public class OptimizationTask : AppTask
    {
        
    }
}

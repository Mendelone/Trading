﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ITIndicator
    {
        string Symbol { get; set; }
        string TimeFrame { get; set; }

        bool Log { get; set; }

        double SignalThreshold { get; set; }
    }
}
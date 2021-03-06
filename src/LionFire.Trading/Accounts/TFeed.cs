﻿using LionFire.Execution;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Accounts
{
    /// <summary>
    /// Read-only feed of market data.
    /// </summary>
    public abstract class TFeed : IHierarchicalTemplate, ITemplate
    {
        public string AccountId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        /// <summary>
        /// E.g. CTrader, MT4 ECN, MT4 Pro, ...
        /// </summary>
        public string AccountType { get; internal set; }

        // TODO: Set this from filename during load
        public string AccountName { get; set; }

        public string AssetSubPath => BrokerName +"." + AccountName;

        public string BrokerName { get; set; }

        public string Key => BrokerName + "^" + AccountId;

        public List<ITemplate> Children { get; set; }

        // MOVE this to Workspace, don't put startup preferences here.
        public ExecutionState DesiredExecutionState { get; set; }
    }
}

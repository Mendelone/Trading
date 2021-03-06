﻿using LionFire.Execution.Executables;
using LionFire.Notifications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using LionFire.Assets;
using LionFire.Serialization;
using System.IO;
using LionFire.Execution;
using System.Threading.Tasks;
using LionFire.Validation;
using LionFire.Composables;
using System.Diagnostics;

namespace LionFire.Trading.Notifications
{

    public class TradingNotifierHost : InitializableExecutableBase, ITradingNotifierHost, IStartable, IStoppable, IComposition
    {
        // FUTURE: Change this collection type to an interface?  IHandleDirectory?
        public FsObjectCollection<Notifier> Notifiers => notifiers;
        FsObjectCollection<Notifier> notifiers = new FsObjectCollection<Notifier>(Path.Combine(LionFireEnvironment.GetProgramDataDir("Trading"), "Alerts"));

        public IEnumerable<object> Children { get { return Notifiers.Objects; } }

        public TradingNotifierHost()
        {
            notifiers.IsObjectsEnabled = true;
            notifiers.ObjectsCollectionChanged += Notifiers_ObjectsCollectionChanged;
        }

        private void Notifiers_ObjectsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("Notifiers_ObjectsCollectionChanged " + e.ToString());
        }

        #region Lifecycle

        public Task Stop()
        {
            throw new NotImplementedException();
        }

        public async Task Start()
        {
            await this.StartChildren();
        }

        #endregion
    }
}

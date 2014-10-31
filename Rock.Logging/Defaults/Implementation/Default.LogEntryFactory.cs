﻿using System;
using Rock.Defaults;

namespace Rock.Logging.Defaults.Implementation
{
    public static partial class Default
    {
        private static readonly DefaultHelper<ILogEntryFactory> _logEntryFactory = new DefaultHelper<ILogEntryFactory>(() => new LogEntryFactory());

        public static ILogEntryFactory LogEntryFactory
        {
            get { return _logEntryFactory.Current; }
        }

        public static ILogEntryFactory DefaultLogEntryFactory
        {
            get { return _logEntryFactory.DefaultInstance; }
        }

        public static void SetLogEntryFactory(Func<ILogEntryFactory> getLogEntryFactoryInstance)
        {
            _logEntryFactory.SetCurrent(getLogEntryFactoryInstance);
        }

        public static void RestoreLogEntryFactory()
        {
            _logEntryFactory.RestoreDefault();
        }
    }
}
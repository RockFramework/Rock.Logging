﻿using System.Collections.Generic;
using AutoMoq;
using Moq;
using NUnit.Framework;
using Rock.Logging;

// ReSharper disable once CheckNamespace
namespace LoggerTests
{
    public class Wtf
    {
        [Test]
        public void Bbq()
        {
            
        }
    }

    public abstract class LoggerTestsBase
    {
        protected AutoMoqer _mocker;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMoqer();
        }

        protected Logger GetLogger()
        {
            return _mocker.Resolve<Logger>();
        }

        public class TheIsEnabledMethod : LoggerTestsBase
        {
            private Mock<ILogProvider> _mockLogProvider;

            [SetUp]
            public new void Setup()
            {
                _mockLogProvider = new Mock<ILogProvider>();
            }

            [TestCase(LogLevel.Debug, LogLevel.Debug)]
            [TestCase(LogLevel.Info, LogLevel.Debug)]
            [TestCase(LogLevel.Info, LogLevel.Info)]
            [TestCase(LogLevel.Warn, LogLevel.Debug)]
            [TestCase(LogLevel.Warn, LogLevel.Info)]
            [TestCase(LogLevel.Warn, LogLevel.Warn)]
            [TestCase(LogLevel.Error, LogLevel.Debug)]
            [TestCase(LogLevel.Error, LogLevel.Info)]
            [TestCase(LogLevel.Error, LogLevel.Warn)]
            [TestCase(LogLevel.Error, LogLevel.Error)]
            [TestCase(LogLevel.Fatal, LogLevel.Debug)]
            [TestCase(LogLevel.Fatal, LogLevel.Info)]
            [TestCase(LogLevel.Fatal, LogLevel.Warn)]
            [TestCase(LogLevel.Fatal, LogLevel.Error)]
            [TestCase(LogLevel.Fatal, LogLevel.Fatal)]
            public void ReturnsTrueWhenTheLogLevelParameterIsGreaterThanOrEqualToTheConfiguredLogLevel(LogLevel logLevelParameter, LogLevel configuredLogLevel)
            {
                RunTest(logLevelParameter, configuredLogLevel, true, true);
            }

            [TestCase(LogLevel.Debug, LogLevel.Info)]
            [TestCase(LogLevel.Debug, LogLevel.Warn)]
            [TestCase(LogLevel.Debug, LogLevel.Error)]
            [TestCase(LogLevel.Debug, LogLevel.Fatal)]
            [TestCase(LogLevel.Debug, LogLevel.Audit)]
            [TestCase(LogLevel.Info, LogLevel.Warn)]
            [TestCase(LogLevel.Info, LogLevel.Error)]
            [TestCase(LogLevel.Info, LogLevel.Fatal)]
            [TestCase(LogLevel.Info, LogLevel.Audit)]
            [TestCase(LogLevel.Warn, LogLevel.Error)]
            [TestCase(LogLevel.Warn, LogLevel.Fatal)]
            [TestCase(LogLevel.Warn, LogLevel.Audit)]
            [TestCase(LogLevel.Error, LogLevel.Fatal)]
            [TestCase(LogLevel.Error, LogLevel.Audit)]
            [TestCase(LogLevel.Fatal, LogLevel.Audit)]
            public void ReturnsFalseWhenTheLogLevelParameterIsLessThanTheConfiguredLogLevel(LogLevel logLevelParameter, LogLevel configuredLogLevel)
            {
                RunTest(logLevelParameter, configuredLogLevel, true, false);
            }

            [TestCase(LogLevel.NotSet, LogLevel.NotSet)]
            [TestCase(LogLevel.NotSet, LogLevel.Debug)]
            [TestCase(LogLevel.NotSet, LogLevel.Info)]
            [TestCase(LogLevel.NotSet, LogLevel.Warn)]
            [TestCase(LogLevel.NotSet, LogLevel.Error)]
            [TestCase(LogLevel.NotSet, LogLevel.Fatal)]
            [TestCase(LogLevel.NotSet, LogLevel.Audit)]
            public void AlwaysReturnsFalseWhenTheLogLevelParameterIsNotSet(LogLevel logLevelParameter, LogLevel configuredLogLevel)
            {
                RunTest(logLevelParameter, configuredLogLevel, true, false);
            }

            [TestCase(LogLevel.Audit, LogLevel.NotSet)]
            [TestCase(LogLevel.Audit, LogLevel.Debug)]
            [TestCase(LogLevel.Audit, LogLevel.Info)]
            [TestCase(LogLevel.Audit, LogLevel.Warn)]
            [TestCase(LogLevel.Audit, LogLevel.Error)]
            [TestCase(LogLevel.Audit, LogLevel.Fatal)]
            [TestCase(LogLevel.Audit, LogLevel.Audit)]
            public void AlwaysReturnsTrueWhenTheLogLevelParameterIsAudit(LogLevel logLevelParameter, LogLevel configuredLogLevel)
            {
                RunTest(logLevelParameter, configuredLogLevel, true, true);
            }

            [TestCase(LogLevel.Debug, LogLevel.Debug)]
            [TestCase(LogLevel.Info, LogLevel.Debug)]
            [TestCase(LogLevel.Info, LogLevel.Info)]
            [TestCase(LogLevel.Warn, LogLevel.Debug)]
            [TestCase(LogLevel.Warn, LogLevel.Info)]
            [TestCase(LogLevel.Warn, LogLevel.Warn)]
            [TestCase(LogLevel.Error, LogLevel.Debug)]
            [TestCase(LogLevel.Error, LogLevel.Info)]
            [TestCase(LogLevel.Error, LogLevel.Warn)]
            [TestCase(LogLevel.Error, LogLevel.Error)]
            [TestCase(LogLevel.Fatal, LogLevel.Debug)]
            [TestCase(LogLevel.Fatal, LogLevel.Info)]
            [TestCase(LogLevel.Fatal, LogLevel.Warn)]
            [TestCase(LogLevel.Fatal, LogLevel.Error)]
            [TestCase(LogLevel.Fatal, LogLevel.Fatal)]

            [TestCase(LogLevel.Audit, LogLevel.NotSet)]
            [TestCase(LogLevel.Audit, LogLevel.Debug)]
            [TestCase(LogLevel.Audit, LogLevel.Info)]
            [TestCase(LogLevel.Audit, LogLevel.Warn)]
            [TestCase(LogLevel.Audit, LogLevel.Error)]
            [TestCase(LogLevel.Audit, LogLevel.Fatal)]
            [TestCase(LogLevel.Audit, LogLevel.Audit)]
            public void AlwaysReturnFalseWhenIsLoggingEnabledIsFalse(LogLevel logLevelParameter, LogLevel configuredLogLevel)
            {
                RunTest(logLevelParameter, configuredLogLevel, false, false);
            }

            private void RunTest(LogLevel logLevelParameter, LogLevel configuredLogLevel, bool configuredIsLoggingEnabled, bool expected)
            {
                _mocker.GetMock<IEnumerable<ILogProvider>>()
                    .Setup(m => m.GetEnumerator())
                    .Returns(GetMockLogProviders());

                _mocker.GetMock<ILoggerConfiguration>()
                    .Setup(m => m.LoggingLevel)
                    .Returns(configuredLogLevel);

                _mocker.GetMock<ILoggerConfiguration>()
                    .Setup(m => m.IsLoggingEnabled)
                    .Returns(configuredIsLoggingEnabled);

                _mocker.GetMock<ILoggerConfiguration>()
                    .Setup(m => m.ConcurrencyLevel)
                    .Returns(3);

                _mocker.GetMock<IEnumerable<IContextProvider>>()
                    .Setup(m => m.GetEnumerator())
                    .Returns(GetEmptyContextProviders());

                var logger = _mocker.Resolve<Logger>();

                var result = logger.IsEnabled(logLevelParameter);

                Assert.That(result, Is.EqualTo(expected));
            }

            private IEnumerator<ILogProvider> GetMockLogProviders()
            {
                yield return _mockLogProvider.Object;
            }

            private IEnumerator<IContextProvider> GetEmptyContextProviders()
            {
                yield break;
            }
        }

        public class TheLogAsyncMethod
        {
            
        }
    }
}
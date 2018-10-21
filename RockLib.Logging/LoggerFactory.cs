﻿using Microsoft.Extensions.Configuration;
using RockLib.Configuration;
using RockLib.Configuration.ObjectFactory;
using RockLib.Immutable;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RockLib.Logging
{
    /// <summary>
    /// A static class that creates and retrieves instances of the <see cref="ILogger"/> interface by name.
    /// Loggers returned by this factory are defined by instances of the <see cref="IConfiguration"/> interface.
    /// </summary>
    public static class LoggerFactory
    {
        /// <summary>
        /// The section name, relative to <see cref="Config.Root"/>, from which to retrieve logging settings.
        /// </summary>
        public const string SectionName = "RockLib.Logging";

        private static readonly ConditionalWeakTable<IConfiguration, ConcurrentDictionary<string, ILogger>> _cache = new ConditionalWeakTable<IConfiguration, ConcurrentDictionary<string, ILogger>>();

        private static readonly Semimutable<IConfiguration> _configuration =
            new Semimutable<IConfiguration>(() => Config.Root.GetSection(SectionName));

        /// <summary>
        /// Sets the instance of <see cref="IConfiguration"/> that defines the loggers that can be created
        /// or retrieved. Note that once the <see cref="Configuration"/> property has been read from, it
        /// cannot be changed.
        /// </summary>
        /// <param name="configuration">
        /// An instance of <see cref="IConfiguration"/> that defines the loggers that can be retrieved. The
        /// configuration can define a single logger object or a list of logger objects.
        /// </param>
        public static void SetConfiguration(IConfiguration configuration) => _configuration.Value = configuration ?? throw new ArgumentNullException(nameof(configuration));

        /// <summary>
        /// Gets the instance of <see cref="IConfiguration"/> that defines the loggers that can be created
        /// or retrieved.
        /// </summary>
        /// <remarks>
        /// Only the extension methods in this class that do not have an <see cref="IConfiguration"/> parameter
        /// use this property.
        /// </remarks>
        public static IConfiguration Configuration => _configuration.Value;

        /// <summary>
        /// Gets a cached instance of <see cref="ILogger"/> with a name matching the <paramref name="name"/>
        /// parameter that is backed by the value of the <see cref="Configuration"/> property.
        /// </summary>
        /// <param name="name">The name of the logger to retrieve.</param>
        /// <returns>A logger with a matching name.</returns>
        /// <exception cref="KeyNotFoundException">
        /// If a logger with a name specified by the <paramref name="name"/> parameter is not defined in
        /// the value of the <see cref="Configuration"/> property.
        /// </exception>
        public static ILogger GetCached(string name = Logger.DefaultName) => Configuration.GetCachedLogger(name);

        /// <summary>
        /// Gets a cached instance of <see cref="ILogger"/> with a name matching the <paramref name="name"/>
        /// parameter that is backed by the value of the <paramref name="configuration"/> parameter.
        /// </summary>
        /// <param name="configuration">
        /// An instance of <see cref="IConfiguration"/> that defines the loggers that can be retrieved. The
        /// configuration can define a single logger object or a list of logger objects.
        /// </param>
        /// <param name="name">The name of the logger to retrieve.</param>
        /// <returns>A logger with a matching name.</returns>
        /// <exception cref="KeyNotFoundException">
        /// If a logger with a name specified by the <paramref name="name"/> parameter is not defined in
        /// the value of the <paramref name="configuration"/> parameter.
        /// </exception>
        public static ILogger GetCachedLogger(this IConfiguration configuration, string name = Logger.DefaultName)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var configCache = _cache.GetValue(configuration, c => new ConcurrentDictionary<string, ILogger>());
            return configCache.GetOrAdd(name, n => configuration.CreateLogger(n));
        }

        /// <summary>
        /// Creates a new instance of <see cref="ILogger"/> with a name matching the <paramref name="name"/>
        /// parameter that is backed by the value of the <see cref="Configuration"/> property.
        /// </summary>
        /// <param name="name">The name of the logger to create.</param>
        /// <returns>A new logger with a matching name.</returns>
        /// <exception cref="KeyNotFoundException">
        /// If a logger with a name specified by the <paramref name="name"/> parameter is not defined in
        /// the value of the <see cref="Configuration"/> property.
        /// </exception>
        public static ILogger Create(string name = Logger.DefaultName) => Configuration.CreateLogger(name);

        /// <summary>
        /// Creates a new instance of <see cref="ILogger"/> with a name matching the <paramref name="name"/>
        /// parameter that is backed by the value of the <paramref name="configuration"/> parameter.
        /// </summary>
        /// <param name="configuration">
        /// An instance of <see cref="IConfiguration"/> that defines the loggers that can be retrieved. The
        /// configuration can define a single logger object or a list of logger objects.
        /// </param>
        /// <param name="name">The name of the logger to create.</param>
        /// <returns>A new logger with a matching name.</returns>
        /// <exception cref="KeyNotFoundException">
        /// If a logger with a name specified by the <paramref name="name"/> parameter is not defined in
        /// the value of the <paramref name="configuration"/> parameter.
        /// </exception>
        public static ILogger CreateLogger(this IConfiguration configuration, string name = Logger.DefaultName)
        {
            var defaultTypes = new DefaultTypes().Add(typeof(ILogger), typeof(Logger));

            if (configuration.IsList())
            {
                foreach (var child in configuration.GetChildren())
                    if (name.Equals(child.GetSectionName(), StringComparison.OrdinalIgnoreCase))
                        return child.CreateReloadingProxy<ILogger>(defaultTypes);
            }
            else if (name.Equals(configuration.GetSectionName(), StringComparison.OrdinalIgnoreCase))
                return configuration.CreateReloadingProxy<ILogger>(defaultTypes);

            throw new KeyNotFoundException($"No loggers were found matching the name '{name}'.");
        }

        private static bool IsEmpty(this IConfiguration configuration)
        {
            switch (configuration)
            {
                case IConfigurationSection section:
                    return section.Value == null && !section.GetChildren().Any();
                default:
                    return !configuration.GetChildren().Any();
            }
        }

        private static bool IsList(this IConfiguration configuration)
        {
            if (configuration is IConfigurationSection section && section.Value != null)
                return false;
            int i = 0;
            foreach (var child in configuration.GetChildren())
                if (child.Key != i++.ToString())
                    return false;
            return i > 0;
        }

        private static string GetSectionName(this IConfiguration configuration)
        {
            var section = configuration;

            if (configuration["type"] != null && !configuration.GetSection("value").IsEmpty())
                section = configuration.GetSection("value");

            return section["name"] ?? Logger.DefaultName;
        }
    }
}

﻿using System;

namespace StatsdClient
{
    public static class Metrics
    {
        private static IStatsd _statsD = new NullStatsd();
        private static StatsdUDP _statsdUdp;
        private static string _prefix;

        /// <summary>
        /// Configures the Metric class with a configuration. Call this once at application startup (Main(), Global.asax, etc).
        /// </summary>
        /// <param name="config">Configuration settings.</param>
        public static void Configure(MetricsConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _prefix = config.Prefix ?? "";
            _prefix = _prefix.TrimEnd('.');
            CreateStatsD(config);
        }

        private static void CreateStatsD(MetricsConfig config)
        {
            if (_statsdUdp != null)
            {
                _statsdUdp.Dispose();
            }

            _statsdUdp = null;

            if (!string.IsNullOrEmpty(config.StatsdServerName))
            {
                _statsdUdp = new StatsdUDP(config.StatsdServerName, config.StatsdServerPort, config.StatsdMaxUDPPacketSize);
                _statsD = new Statsd(_statsdUdp);
            }
        }

        

        /// <summary>
        /// Modify the current value of the gauge with the given value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="deltaValue"></param>
        public static void GaugeDelta(string statName, double deltaValue)
        {
            _statsD.SendGauge(BuildNamespacedStatName(statName), deltaValue, true);
        }

        /// <summary>
        /// Set the gauge to the given absolute value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="absoluteValue">Absolute value of the gauge to set.</param>
        public static void GaugeAbsoluteValue(string statName, double absoluteValue)
        {
            _statsD.SendGauge(BuildNamespacedStatName(statName), absoluteValue, false);
        }

        [Obsolete("Will be removed in future version. Use explicit GaugeDelta or GaugeAbsoluteValue instead.")]
        public static void Gauge(string statName, double value)
        {
            GaugeAbsoluteValue(statName, value);
        }

		/// <summary>
		/// Send a counter value.
		/// </summary>
		/// <param name="statName">Name of the metric.</param>
		/// <param name="value">Value of the counter. Defaults to 1.</param>
		/// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
		public static void Counter(string statName, int value = 1, double sampleRate = 1)
		{
			_statsD.SendInteger(IntegralMetric.Counter, BuildNamespacedStatName(statName), value, sampleRate);
		}

        /// <summary>
        /// Send a manually timed value.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Elapsed miliseconds of the event.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static void Timer(string statName, int value, double sampleRate = 1)
        {
            _statsD.SendInteger(IntegralMetric.Timer, BuildNamespacedStatName(statName), value, sampleRate);
        }

        /// <summary>
        /// Time a given piece of code (with a using block) and send the elapsed miliseconds
        /// </summary>
        /// <param name="name">Name of the metric.</param>
        /// <returns>A disposable object that will record & send the metric.</returns>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static IDisposable StartTimer(string name, double sampleRate = 1)
        {
            return new MetricsTimer(name, sampleRate);
        }

        /// <summary>
        /// Time a given piece of code (with a lambda) and send the elapsed miliseconds.
        /// </summary>
        /// <param name="action">The code to time.</param>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="sampleRate">Sample rate to reduce the load on your metric server. Defaults to 1 (100%).</param>
        public static void Time(Action action, string statName, int sampleRate = 1)
        {
            _statsD.Send(action, BuildNamespacedStatName(statName), sampleRate);
        }

        /// <summary>
        /// Time a given piece of code (with a lambda) and send the elapsed miliseconds.
        /// </summary>
        /// <param name="func">The code to time.</param>
        /// <param name="statName">Name of the metric.</param>
        /// <returns>Return value of the function.</returns>
        public static T Time<T>(Func<T> func, string statName)
        {
            using (StartTimer(statName))
            {
                return func();
            }
        }

        /// <summary>
        /// Store a unique occurence of an event between flushes.
        /// </summary>
        /// <param name="statName">Name of the metric.</param>
        /// <param name="value">Value to set.</param>
        public static void Set(string statName, string value)
        {
            _statsD.SendSet(BuildNamespacedStatName(statName), value);
        }

        private static string BuildNamespacedStatName(string statName)
        {
            if (string.IsNullOrEmpty(_prefix))
            {
                return statName;
            }

            return _prefix + "." + statName;
        }
    }
}
﻿using System;
using System.Configuration;
using System.Net.Sockets;
using NUnit.Framework;
using Telegraf;

namespace Tests {

	[TestFixture]
	public class UDPSmokeTests {
		static readonly int _serverPort =
			Convert.ToInt32(ConfigurationManager.AppSettings["StatsdServerPort"]);

		static readonly string _serverName = ConfigurationManager.AppSettings["StatsdServerName"];

		[Test]
		public void Sends_a_counter() {
			try {
				var client = new StatsdUDP(_serverName, _serverPort);
				client.Send("socket2:1|c");
			}
			catch (SocketException ex) {
				Assert.Fail(
					"Socket Exception, have you setup your Statsd name and port? It's currently '{0}:{1}'. Error: {2}",
					_serverName, _serverPort, ex.Message);
			}
		}
	}

}
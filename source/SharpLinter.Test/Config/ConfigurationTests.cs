﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpLinter.Config;

namespace SharpLinter.Test.Config
{
	[TestClass]
	public class ConfigurationTests
	{
		[TestMethod]
		public void Config()
		{
			var config = new JsLintConfiguration();

			Assert.AreEqual("maxerr: 50", config.OptionsToString(), "Default has no options set.");
			Assert.AreEqual(String.Empty, config.GlobalsToString(), "Default has no globals set.");
			Assert.AreEqual(50, config.MaxErrors, "Maxerrors=99999");
			Assert.AreEqual(true, config.ErrorOnUnused, "Unused default");
			Assert.AreEqual(config.MaxErrors, config.GetOption<int>("maxerr"), config.MaxErrors, "Maxerrors = options(maxerr)");

			config.SetOption("unused");
			config.SetOption("evil");
			config.SetOption("maxerr", 50);
			config.SetOption("immed");

			Assert.AreEqual("maxerr: 50, unused: true, evil: true, immed: true", config.OptionsToString(),
				"Basic option setting worked.");

			var config2 = new JsLintConfiguration();
			config2.SetOption("eqeqeq", true);
			config2.SetOption("maxerr", 25);
			config2.SetOption("immed", false);

			config.MergeOptions(config2);

			Assert.AreEqual("maxerr: 25, unused: true, evil: true, immed: false, eqeqeq: true", config.OptionsToString(),
				"Basic option setting worked.");
		}

		[TestMethod]
		public void ConfigFile()
		{
			var configFile = TestResources.GetAppRootedPath("resources\\jslint.test.conf");

			var config = new JsLintConfiguration();

			config.MergeOptions(configFile);

			Assert.AreEqual(
				"maxerr: 50, browser: false, nomen: false, plusplus: false, forin: false, wsh: true, laxbreak: true",
				config.OptionsToString(), "Got lint options from conf file");

			Assert.AreEqual("jQuery, HTMLElement, $", config.GlobalsToString());
			Assert.AreEqual(config.ExcludeFiles.Count, 2, "Got 2 files");
			Assert.AreEqual("*.min.js", config.ExcludeFiles.ElementAt(0), "First file was right");
		}
	}
}
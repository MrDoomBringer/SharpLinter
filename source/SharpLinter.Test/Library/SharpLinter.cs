using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpLinter.Config;

namespace SharpLinter.Test.Library
{
	[TestClass]
	public class LinterUnitTests
	{
		[TestMethod]
		public void ErrorLimit()
		{
			var config = TestResources.DefaultConfig;
			config.SetOption("maxerr", 10);

			var lint = new SharpLinter(config);

			var file = TestResources.LoadAppRootedFile("resources\\errors.js");
			var result = lint.Lint(file);

			Assert.AreEqual(10, result.Errors.Count);
			Assert.AreEqual(true, result.Limited);

			config.SetOption("maxerr", 10000);
			result = lint.Lint(file);

			Assert.IsTrue(result.Errors.Count > 100);
			Assert.AreEqual(false, result.Limited);
		}

		[TestMethod]
		public void TestMultipleCalls()
		{
			var config = TestResources.DefaultConfig;

			var lint = new SharpLinter(config);


			var result = lint.Lint(
				@"var i, y; for (i = 0; i < 5; i++) console.Print(message + ' (' + i + ')'); number += i;");

			// original test was 4 errors - jshint defaults?
			Assert.AreEqual(4, result.Errors.Count);

			var result2 = lint.Lint(
				@"function annon() { var i, number; for (i = 0; i === 5; i++) { number += i; } }");

			Assert.AreEqual(1, result2.Errors.Count);

			var result3 = lint.Lint(
				@"function annon() { var i, number, x; for (i = 0; i == 5; i++) { number += i; } }");

			Assert.AreEqual(2, result3.Errors.Count);
		}

		[TestMethod]
		public void TestMultipleDifferentOptions()
		{
			var config = new JsLintConfiguration();
			var lint = new SharpLinter(config);

			config.SetOption("eqeqeq", true);
			config.SetOption("plusplus", true);

			var result = lint.Lint(
				@"function annon() { var i, number, x; for (i = 0; i == 5; i++) { number += ++i; } }"
				);

			Assert.AreEqual(3, result.Errors.Count);

			config = new JsLintConfiguration();
			config.SetOption("unused", false);

			// should fail on ++ since "plusplus=true" -- note that we are testing with JSHINT so the
			// behavior is opposite of JSLINT for this options

			var result2 = lint.Lint(
				@"function annon() { var i, number; for (i = 0; i === 5; i++) { number += i; } }");

			Assert.AreEqual(1, result2.Errors.Count);
		}

		[TestMethod]
		public void TestArgumentParsing()
		{
			var config =
				JsLintConfiguration.ParseString(
					"  maxerr : 2,eqeqeq,unused :    TRUE,predef : test1 TEST2   3 ,evil:false , browser : true", LinterType.JSHint);

			Assert.AreEqual(2, config.MaxErrors);
			Assert.AreEqual("maxerr: 2, eqeqeq: true, unused: true, evil: false, browser: true", config.OptionsToString());
			Assert.AreEqual(true, config.ErrorOnUnused);
			Assert.AreEqual(3, config.Globals.Count());
			Assert.AreEqual("test1", config.Globals.ElementAt(0));
			Assert.AreEqual("TEST2", config.Globals.ElementAt(1));
			Assert.AreEqual("3", config.Globals.ElementAt(2));
		}

		[TestMethod]
		public void TestArgumentParsing2()
		{
			var config =
				JsLintConfiguration.ParseString(
					"  maxerr : 400,eqeqeq : true,unused :    FALSE,predef : 1 window alert,evil:true , browser : false",
					LinterType.JSHint);

			Assert.AreEqual(400, config.MaxErrors);
			Assert.AreEqual("maxerr: 400, eqeqeq: true, unused: false, evil: true, browser: false", config.OptionsToString());
			Assert.AreEqual(false, config.ErrorOnUnused);
			Assert.AreEqual(3, config.Globals.Count());
			Assert.AreEqual("1", config.Globals.ElementAt(0));
		}
	}
}
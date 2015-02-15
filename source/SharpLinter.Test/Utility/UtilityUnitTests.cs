using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpLinter.Config;

namespace SharpLinter.Test.Utility
{
	[TestClass]
	public class UtilityUnitTests
	{
		[TestMethod]
		public void FilePathMatcherTest()
		{
			string[] testFiles =
			{
				"c:/temp/subfolder/file.js",
				"c:/temp/file.cs",
				"c:/projects/temp/file.cs",
				"c:/projects/file.js",
				"c:/projects/file.min.js"
			};


			var matches = new List<string>(FilePathMatcher.MatchFiles("*.js", testFiles, false));
			var expected = new List<string>(new[]
			{
				"c:/temp/subfolder/file.js", "c:/projects/file.js",
				"c:/projects/file.min.js"
			});

			Assert.AreEqual(3, matches.Count, "Matches single extension pattern *.js");
			Assert.AreEqual(string.Join(",", expected), string.Join(",", matches), "List matches");

			matches = new List<string>(FilePathMatcher.MatchFiles("*.min.js", testFiles, true));

			Assert.AreEqual(4, matches.Count, "Matches exclusion pattern *.min.js");

			matches = new List<string>(FilePathMatcher.MatchFiles("temp/", testFiles, true));

			expected = new List<string>(new[]
			{
				"c:/projects/file.js",
				"c:/projects/file.min.js"
			});

			Assert.AreEqual(string.Join(",", expected), string.Join(",", matches), "List matches on excluding a folder path");
		}
	}
}
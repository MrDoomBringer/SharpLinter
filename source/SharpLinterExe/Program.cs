using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SharpLinter.Config;

/*
 * 
 * SharpLinter
 * (c) 2011 James Treworgy
 * 
 * Based on code originally created by Luke Page/ScottLogic: http://www.scottlogic.co.uk/2010/09/js-lint-in-visual-studio-part-1/
 * 
 */

namespace SharpLinter
{
	internal class SharpLinterExe
	{
		private static int Main(string[] args)
		{
			var readKey = false;
			var errorCount = 0;
			try
			{
				var _Configuration = new Lazy<JsLintConfiguration>();
				Func<JsLintConfiguration> Configuration = () => { return _Configuration.Value; };

				if (args.Length == 0)
				{
					Console.WriteLine("SharpLinter [-[r]f /path/*.js] [-o options] ");
					Console.WriteLine("            [-c sharplinter.conf] [-j jslint.js]");
					Console.WriteLine("            [-v[1|2|3]] [--noglobal]");
					Console.WriteLine("            [-i ignore-start ignore-end] [-if text] [-of \"format\"] [file]");
					Console.WriteLine();
					Console.WriteLine(("Options: \n\n" +
										"-[r]f c:\\scripts\\*.js     parse all files matching \"*.js\" in \"c:\\scripts\"\n" +
										"                          if called with \"r\", will recurse subfolders\n" +
										"-o \"option option ...\"    set jslint/jshint options specified, separated by\n" +
										"                          spaces, in format \"option\" or \"option: true|false\"\n" +
										"-v[1|2|3]                 be [terse][verbose-default][really verbose]\n" +
										"\n" +
										"-k                        Wait for a keytroke when done\n" +
										"-c c:\\sharplinter.conf   load config options from file specified\n" +
										"--noglobal                ignore global config file\n" +
										"-j jslint.js              use file specified to parse files instead of embedded\n" +
										"                          (probably old) script\n" +
										"\n" +
										"-i text-start text-end    Ignore blocks bounded by /*text-start*/ and\n" +
										"                          /*text-end*/\n" +
										"-if text-skip             Ignore files that contain /*text-skip*/ anywhere\n" +
										"-of \"output format\"       Use the string as a format for the error output. The\n" +
										"                          default is:\n" +
										"                          \"{0}({1}): ({2}) {3} at character {4}\". The parms are\n" +
										"                          {0}: full file path, {1}: line number, {2}: source\n" +
										"                          (lint or yui), {4}: character\n")
						.Replace("\n", Environment.NewLine));


					Console.Write("Options Format:");
					Console.WriteLine(JsLintConfiguration.GetParseOptions());

					Console.WriteLine();
					Console.WriteLine("E.g.");
					Console.WriteLine("JsLint -f input.js -f input2.js");
					Console.WriteLine("JsLint -f input.js -o \"evil=False,eqeqeq,predef=Microsoft System\"");
					return 0;
				}

				//string commandlineConfig = String.Empty;
				var commandLineOptions = String.Empty;
				var globalConfigFile = "";
				var excludeFiles = "";
				var jsLintSource = "";

				var filePaths = new HashSet<PathInfo>();


				var recurse = false;
				var noGlobal = false;

				LinterType linterType = 0;

				var finalConfig = new JsLintConfiguration();


				for (var i = 0; i < args.Length; i++)
				{
					var arg = args[i].Trim().ToLower();
					var value = args.Length > i + 1 ? args[i + 1] : String.Empty;
					var value2 = args.Length > i + 2 ? args[i + 2] : String.Empty;
					//string filter = null;
					switch (arg)
					{
						case "-of":
							finalConfig.OutputFormat = value.Replace("\\r", "\r").Replace("\\n", "\n");
							i++;
							break;
						case "-i":
							finalConfig.IgnoreStart = value;
							finalConfig.IgnoreFile = value2;
							break;
						case "-ie":
							finalConfig.IgnoreEnd = value;
							break;
						case "-c":
							globalConfigFile = value;
							i++;
							break;
						case "-j":

							if (File.Exists(value))
							{
								jsLintSource = value;
							}
							else
							{
								Console.WriteLine("Cannot find JSLint source file {0}", value);
								goto exit;
							}
							i++;
							break;
						case "-k":
							readKey = true;
							break;
						case "-f":
						case "-rf":
							filePaths.Add(new PathInfo(value, arg == "-rf"));
							i++;
							break;
						case "-r":
							recurse = true;
							break;
						case "-o":
							commandLineOptions = commandLineOptions.AddListItem(value, " ");
							i++;
							break;
						case "-x":
							excludeFiles = excludeFiles.AddListItem(value, " ");
							i++;
							break;
						case "-v":
						case "-v1":
						case "-v2":
						case "-v3":
							finalConfig.Verbosity = arg.Length == 2
								? Verbosity.Debugging
								: (Verbosity) Convert.ToInt32(arg.Substring(2, 1));

							break;
						case "--noglobal":
							noGlobal = true;
							break;
						default:
							if (arg[0] == '-')
							{
								throw new Exception("Unrecognized command line option \"" + arg + "\"");
							}
							filePaths.Add(new PathInfo(arg, recurse));
							break;
					}
				}
				// Done parsing options.. look for linter

				var lintSourcePath = "";
				var lintSource = GetLinter(jsLintSource, out lintSourcePath);
				if (!string.IsNullOrEmpty(lintSource))
				{
					finalConfig.JsLintCode = lintSource;
					finalConfig.JsLintFilePath = lintSourcePath;
				}

				if (!string.IsNullOrEmpty(excludeFiles))
				{
					foreach (var file in excludeFiles.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries))
					{
						finalConfig.SetFileExclude(file);
					}
				}

				// Get global config options
				if (!noGlobal)
				{
					string globalConfigPath;
					var config = GetConfig(globalConfigFile, out globalConfigPath);
					if (!string.IsNullOrEmpty(config))
					{
						try
						{
							finalConfig.MergeOptions(JsLintConfiguration.ParseConfigFile(config, linterType));
							finalConfig.GlobalConfigFilePath = globalConfigPath;
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
							goto exit;
						}
					}
				}


				// add the basic config we built so far
				finalConfig.MergeOptions(Configuration());

				// Overlay any command line options
				if (commandLineOptions != null)
				{
					try
					{
						finalConfig.MergeOptions(JsLintConfiguration.ParseString(commandLineOptions, linterType));
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
						goto exit;
					}
				}

				try
				{
					var batch = new SharpLinterBatch(finalConfig);
					batch.FilePaths = filePaths;
					errorCount = batch.Process();
				}
				catch (Exception e)
				{
					Console.WriteLine("Parser error: " + e.Message +
									(finalConfig.Verbosity == Verbosity.Debugging
										? ". Stack trace (verbose mode): " + e.StackTrace
										: ""));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			exit:
			if (readKey)
			{
				Console.ReadKey();
			}

			return errorCount == 0 ? 0 : 1;
		}

		#region support functions

		/// <summary>
		/// Get a linter, either from default locations, embedded file or user passed
		/// </summary>
		/// <param name="jsLintSource"></param>
		private static string GetLinter(string jsLintSource, out string path)
		{
			// bool parm means fail if missing (only for user-specified source)
			var sources = new List<Tuple<string, bool>>();
			if (!String.IsNullOrEmpty(jsLintSource))
			{
				sources.Add(Tuple.Create(Utility.ResolveRelativePath_AppRoot(jsLintSource), true));
			}
			sources.Add(Tuple.Create(Utility.ResolveRelativePath_AppRoot("jslint.js"), false));
			sources.Add(Tuple.Create(Utility.ResolveRelativePath_AppRoot("jshint.js"), false));

			// also check exe location
			var exeLocation = Assembly.GetAssembly(typeof (SharpLinterExe)).Location;
			sources.Add(Tuple.Create(exeLocation + "\\jslint.js", false));
			sources.Add(Tuple.Create(exeLocation + "\\jshint.js", false));
			return GetFirstMatchingFile(sources, out path);
		}

		private static string GetConfig(string globalConfigFile, out string path)
		{
			//
			var sources = new List<Tuple<string, bool>>();
			if (!String.IsNullOrEmpty(globalConfigFile))
			{
				sources.Add(Tuple.Create(Utility.ResolveRelativePath_AppRoot(globalConfigFile), true));
			}
			sources.Add(Tuple.Create(Utility.ResolveRelativePath_AppRoot("sharplinter.conf"), false));
			sources.Add(
				Tuple.Create(
					Assembly.GetAssembly(typeof (SharpLinterExe)).Location + "\\sharplinter.conf", false));

			return GetFirstMatchingFile(sources, out path);
		}

		private static string GetFirstMatchingFile(IEnumerable<Tuple<string, bool>> files, out string path)
		{
			string fileText = null;
			path = "";
			foreach (var item in files)
			{
				if (File.Exists(item.Item1))
				{
					try
					{
						fileText = File.ReadAllText(item.Item1);
						path = item.Item1;
						break;
					}
					catch
					{
						Console.WriteLine("The file \"{0}\" appears to be invalid.", item.Item1);
						path = "";
						break;
					}
				}
				if (item.Item2) // was required
				{
					Console.WriteLine("The file \"{0}\" does not exist.", item.Item1);
				}
			}
			return fileText;
		}

		private static int LintDataComparer(JsLintData x, JsLintData y)
		{
			return x.Line.CompareTo(y.Line);
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SharpLinter.Config
{
	public enum LinterType
	{
		JSLint = 1,
		JSHint = 2
	}

	public enum InputType
	{
		JavaScript = 1,
		Html = 2
	}

	public enum Verbosity
	{
		DetailOnly = 1,
		Summary = 2,
		Debugging = 3
	}

	/// <summary>
	/// Represents configuring the Js Lint(er)
	/// </summary>
	public class JsLintConfiguration
	{
		/// <summary>
		/// The Js Lint boolean options specified
		/// </summary>
		private readonly Dictionary<string, object> _options = new Dictionary<string, object>();

		private string _jsLintCode;
		protected bool _MinimizeOnSuccess;

		/// <summary>
		/// File masks that will be excluded from wildcard matches
		/// </summary>
		public HashSet<string> ExcludeFiles = new HashSet<string>();

		public JsLintConfiguration()
		{
			LinterType = 0;
			using (var jslintStream = Assembly.Load("SharpLinter")
				.GetManifestResourceStream(
					@"SharpLinter.fulljslint.js"))
			{
				using (var sr = new StreamReader(jslintStream))
				{
					JsLintCode = sr.ReadToEnd();
				}
			}
			JsLintFilePath = "Embedded (R06)";
			SetOption("maxerr", 50);
			InputType = InputType.JavaScript;
			IgnoreStart = "lint-ignore-start";
			IgnoreEnd = "lint-ignore-end";
			IgnoreFile = "lint-ignore-file";
			Verbosity = Verbosity.Summary;
		}

		/// <summary>
		/// The path of the global config file -- used only for reporting.
		/// </summary>
		public string GlobalConfigFilePath { get; set; }

		public string JsLintFilePath { get; set; }
		public InputType InputType { get; set; }

		/// <summary>
		/// The javascript code that will be used to parse the input file.
		/// </summary>
		public string JsLintCode
		{
			get { return _jsLintCode; }
			set
			{
				JsLintVersion = "Unknown version (minimized source?)";
				if (value.IndexOf("var JSHINT") > 0)
				{
					LinterType = LinterType.JSHint;
					JsLintVersion = "Unknown version (JSHINT builds do not include a date or version number)";
				}
				else if (value.IndexOf("var JSLINT") > 0)
				{
					LinterType = LinterType.JSLint;
				}
				else
				{
					throw new Exception("What is this code you are trying to use? It does not appear to be either JSHINT or JSLINT.");
				}

				// even through JSHINT doesn't have the version date, look for it to ID the embedded one
				var useNextLine = false;
				var lineNumber = 0;
				using (var reader = new StringReader(value))
				{
					string line;
					while ((line = reader.ReadLine()) != null && lineNumber < 100)
					{
						lineNumber++;
						if (useNextLine)
						{
							JsLintVersion = "version info: " + line.AfterLast("//").Trim();
							break;
						}
						if (line.StartsWith("// jslint.js"))
						{
							useNextLine = true;
						}
					}
				}
				_jsLintCode = value;
				GetOptionsFromCode();
			}
		}

		public string JsLintVersion { get; set; }
		public string OutputFormat { get; set; }

		/// <summary>
		/// Be verbose
		/// </summary>
		public Verbosity Verbosity { get; set; }

		/// <summary>
		/// A string used to identify a block to skip parsing inside a javascript file, for example, if this
		/// value is "jslint-ignore-start",
		/// then the string /*jslint-ignore-start*/ will identify the start of an ignore block
		/// </summary>
		public string IgnoreStart { get; set; }

		/// <summary>
		/// A string used to identify a block to skip parsing inside a javascript file, for example, if this
		/// value is "jslint-ignore-end",
		/// then the string /*jslint-ignore-end*/ will identify the end of an ignore block
		/// </summary>
		public string IgnoreEnd { get; set; }

		/// <summary>
		/// A string used to indicate that the entire file should be ignored, e.g, if this value is
		/// "jslint-ignore-end" then the comment
		/// /*jslint-ignore-end*/ appearing anywhere in a file will cause it to be skipped
		/// </summary>
		public string IgnoreFile { get; set; }

		/// <summary>
		/// The linter detected.
		/// </summary>
		public LinterType LinterType { get; private set; }

		private Dictionary<string, Tuple<string, Type>> Descriptions
		{
			get
			{
				// This was previously gotten by trying to read the code, which fails with the new JSHint
				//if (_Descriptions ==null)  {
				_Descriptions = GetDescriptions(LinterType.JSHint); // Hard coding to JSHint for now
				//}
				return _Descriptions;
			}
		}

		public int MaxErrors => GetOption<int>("maxerr");

		public bool ErrorOnUnused => !HasOption("unused") || (bool) _options["unused"];

		public IEnumerable<string> Globals
		{
			get
			{
				var list = new HashSet<string>();
				if (HasOption("predef"))
				{
					var predef = GetOption<string>("predef").Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
					foreach (var item in predef)
					{
						yield return item;
					}
				}
			}
		}

		private void GetOptionsFromCode()
		{
			_Descriptions = new Dictionary<string, Tuple<string, Type>>();

			var pos = JsLintCode.IndexOf("boolOptions =");
			if (pos < 0)
			{
				pos = JsLintCode.IndexOf("boolOptions=");
			}
			if (pos < 0)
			{
				return;
			}
			pos = JsLintCode.IndexOf("{", pos);
			var parsedData = String.Empty;
			// if it's not compressed, get rid of comments first
			using (var reader = new StringReader(JsLintCode.Substring(pos + 1)))
			{
				string line;
				var done = false;
				while ((line = reader.ReadLine()) != null && !done)
				{
					if (line.IndexOf("//") >= 0)
					{
						line = line.Before("//") + Environment.NewLine;
					}
					if (line.IndexOf("}") > 0)
					{
						line = line.Before("}");
						done = true;
					}
					parsedData += line.RemoveWhitespace();
				}
			}

			var opts = parsedData.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var key in opts.Select(val => val.Split(':')).Select(parts => parts[0].Trim()).Where(key => !_Descriptions.ContainsKey(key)))
			{
				_Descriptions[key] = new Tuple<string, Type>("Unknown purpose - obtained from source code", typeof (bool));
			}
			// overlay with description info we know
			foreach (var kvp in GetDescriptions(LinterType))
			{
				_Descriptions[kvp.Key] = kvp.Value;
			}
		}

		private static Tuple<string, Type> BoolOpt(string description)
		{
			return Tuple.Create(description, typeof (bool));
		}

		private static Tuple<string, Type> IntOpt(string description)
		{
			return Tuple.Create(description, typeof (int));
		}

		private static Tuple<string, Type> StringOpt(string description)
		{
			return Tuple.Create(description, typeof (string));
		}

		/// <summary>
		/// Gets the options and their descriptions from the hard coded dictionary in this file
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static Dictionary<string, Tuple<string, Type>> GetDescriptions(LinterType type)
		{
			switch (type)
			{
				case LinterType.JSHint:
					return DescriptionsHint;
				case LinterType.JSLint:
					return DescriptionsLint;
				default:
					throw new Exception("Unknown linter type.");
			}
		}

		/// <summary>
		/// Returns human readable text description of what the Parse function will process
		/// </summary>
		public static string GetParseOptions()
		{
			var returner = new StringBuilder();

			returner.AppendLine("[option : value | option][,option: value | option]...");
			returner.AppendLine();
			//returner.AppendLine("maxerr  : Number - maximum number of errors");
			//returner.AppendLine("predef  : String - space seperated list of predefined globals");
			//returner.AppendLine("unused  : Boolean - If true, errors on unused local vars")m,
			returner.AppendLine("JSLINT options:");
			returner.AppendLine();
			AddDescriptionGroup(DescriptionsLint, returner);

			returner.AppendLine();
			returner.AppendLine("JSHINT options: options that function opposite");
			returner.AppendLine("heir JSLint counterpart are marked with (!)");
			returner.AppendLine();

			AddDescriptionGroup(DescriptionsHint, returner);

			return returner.ToString();
		}

		private static void AddDescriptionGroup(Dictionary<string, Tuple<string, Type>> dictionary, StringBuilder returner)
		{
			foreach (var optInfo in dictionary)
			{
				returner.AppendFormat("{0} : ({1}) {1}", optInfo.Key,
					optInfo.Value.Item2.ToString().AfterLast("."),
					optInfo.Value.Item1);
				returner.AppendLine();
			}
		}

		/// <summary>
		/// Merges the options from another config object into this one. The new options supercede if the same
		/// are specified.
		/// </summary>
		/// <param name="configuration"></param>
		public void MergeOptions(JsLintConfiguration configuration)
		{
			foreach (var kvp in configuration._options)
			{
				_options[kvp.Key] = kvp.Value;
			}

			foreach (var file in configuration.ExcludeFiles)
			{
				ExcludeFiles.Add(file);
			}
			if (configuration.IgnoreStart != null)
			{
				IgnoreStart = configuration.IgnoreStart;
			}
			if (configuration.IgnoreEnd != null)
			{
				IgnoreEnd = configuration.IgnoreEnd;
			}
			if (configuration.IgnoreFile != null)
			{
				IgnoreFile = configuration.IgnoreFile;
			}
		}

		public void MergeOptions(string configFile)
		{
			var data = File.ReadAllText(configFile);
			var config = ParseConfigFile(data, LinterType);
			MergeOptions(config);
		}

		public IEnumerable<string> GetMatchedFiles(IEnumerable<PathInfo> paths)
		{
			var hasItems = false;
			foreach (var item in paths)
			{
				var value = Utility.ResolveRelativePath_Current(item.Path);

				string filter = null;
				if (value.Contains("*"))
				{
					filter = Path.GetFileName(value);
					value = value.Substring(0, value.Length - filter.Length);
				}
				// is directory or file?


				if (!Directory.Exists(value))
				{
					// perhaps its a file?
					if (filter == null && !File.Exists(value))
					{
						Console.WriteLine("Ignoring missing file or directory: {0}", value);
						continue;
					}
					yield return value;
					continue;
				}
				var directorys = new List<DirectoryInfo>
				{
					new DirectoryInfo(value)
				};
				while (directorys.Count > 0)
				{
					var di = directorys[0];
					directorys.RemoveAt(0);
					var files  = !string.IsNullOrWhiteSpace(filter) ? di.GetFiles(filter) : di.GetFiles();

					var allFiles = files.Select(fi => fi.FullName).ToList();

					if (item.Recurse)
					{
						directorys.AddRange(di.GetDirectories());
					}


					foreach (var matchedFile in FilePathMatcher.MatchFiles(ExcludeFiles, allFiles, true))
					{
						yield return matchedFile;
					}
				}
			}
			if (!hasItems)
			{
			}
		}

		/// <summary>
		/// Parse a global format config file
		/// </summary>
		/// <param name="configFileData"></param>
		/// <returns></returns>
		public static JsLintConfiguration ParseConfigFile(string configFileData, LinterType linterType)
		{
			var config = new JsLintConfiguration {LinterType = linterType};
			var parser = new ConfigFileParser {ConfigData = configFileData};

			foreach (var kvp in parser.GetKVPSection("jslint"))
			{
				config.SetOption(kvp.Key, kvp.Value);
			}
			foreach (var kvp in parser.GetKVPSection("global"))
			{
				config.SetGlobal(kvp.Key);
			}
			foreach (var item in parser.GetValueSection("exclude", "\n,"))
			{
				config.SetFileExclude(item);
			}
			foreach (var item in parser.GetKVPSection("sharplinter"))
			{
				switch (item.Key.ToLower())
				{
					case "ignorestart":
						config.IgnoreStart = item.Value;
						break;
					case "ignoreend":
						config.IgnoreEnd = item.Value;
						break;
					case "ignorefile":
						config.IgnoreFile = item.Value;
						break;
					default:
						throw new Exception("Unknown sharpLinter option '" + item.Key + "'");
				}
			}
			return config;
		}

		/// <summary>
		/// Parses a string and extracts the options, returning a new JsLintConfiguration object
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static JsLintConfiguration ParseString(string s, LinterType type)
		{
			var returner = new JsLintConfiguration {LinterType = type};
			// if there are no options we return an empty default object
			if (string.IsNullOrWhiteSpace(s)) return returner;
			// otherwise, wipe the bool options
			//returner.BoolOptions = (JsLintBoolOption)0;

			// now go through each string
			var options = s.Split(',');
			foreach (var optionValue in options.Select(option => option.Split(':', '=')))
			{
				// test if it is a single value without assigment ("evil" == "evil:true")
				switch (optionValue.Length)
				{
					case 1:
						returner.SetOption(optionValue[0], true);
						break;
					case 2:
						// otherwise we have key value pair

						var key = optionValue[0].Trim();
						returner.SetOption(optionValue[0], optionValue[1].Trim());
						break;
					default:
						throw new Exception("Unrecognised option format - too many colons");
				}
			}

			return returner;
		}

		/// <summary>
		/// Creates an (javascript compatible) object that JsLint can use for options.
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, object> ToJsOptionVar()
		{
			var returner = new Dictionary<string, object>();

			foreach (var kvp in _options)
			{
				var value = kvp.Value;
				switch (kvp.Key)
				{
					case "predef":
						value = ((string) kvp.Value).Split(' ');
						break;
					case "maxerr":
						value = 99999;
						break;
				}
				returner[kvp.Key] = value;
			}

			return returner;
		}

		public string OptionsToString()
		{
			var result = String.Empty;
			foreach (var kvp in _options.Where(kvp => kvp.Key != "predef"))
			{
				result += (result == String.Empty ? String.Empty : ", ");
				result += kvp.Key + ": " +
						(kvp.Value is bool
							? ((bool) kvp.Value ? "true" : "false")
							: kvp.Value.ToString());
			}
			return result;
		}

		public string GlobalsToString()
		{
			return Globals.Aggregate(string.Empty, (current, item) => current + ((current == string.Empty ? string.Empty : ", ") + item));
		}

		private void SetGlobal(string varName)
		{
			var predef = GetOption("predef", String.Empty);
			SetOption("predef", predef.AddListItem(varName, " "));
		}

		public void SetFileExclude(string mask)
		{
			ExcludeFiles.Add(mask);
		}

		/// <summary>
		/// Returns default value for type if doesn't exist
		/// </summary>
		/// <param name="option"></param>
		/// <returns></returns>
		public T GetOption<T>(string option)
		{
			return GetOption(option, default(T));
		}

		private T GetOption<T>(string option, T defaultValue)
		{
			option = option.Trim().ToLower();
			object value;
			if (!_options.TryGetValue(option, out value)) return defaultValue;
			if (value is T)
			{
				return (T) value;
			}
			throw new Exception("The option '" + option + "' is not of type " + typeof (T));
		}

		private bool HasOption(string option)
		{
			return _options.ContainsKey(option.ToLower().Trim());
		}

		public void SetOption(string option)
		{
			SetOption(option, true);
		}

		public void SetOption(string option, object value)
		{
			option = option.Trim().ToLower();

			Tuple<string, Type> optInfo;
			if (Descriptions.TryGetValue(option, out optInfo))
			{
				if (optInfo.Item2 == typeof (bool))
				{
					bool? val;
					if (value is bool)
					{
						val = (bool) value;
					}
					else
					{
						if (value == null)
						{
							_options.Remove(option);
							return;
						}
						val = Utility.StringToBool(value.ToString(), null);
						if (val == null)
						{
							throw new Exception("Unable to interpret boolean value passed with option '" + option + "'");
						}
					}

					_options[option] = (bool) val;
				}
				else if (optInfo.Item2 == typeof (int))
				{
					int val;
					if (value is int)
					{
						val = (int) value;
					}
					else
					{
						if (!Int32.TryParse(value.ToString(), out val))
						{
							throw new Exception("Unable to interpret integer value passed with option '" + option + "'");
						}
					}
					_options[option] = val;
				}
				else
				{
					// we might be processing a value that can be a bool as well as a string, so try booleanising it first
					// (this is used for example for 'unused' in JSHint, which can be either true, false, "vars" or "strict")
					if (value is bool)
					{
						_options[option] = value;
					}
					else
					{
						var val = Utility.StringToBool(value.ToString(), null);
						if (val == null)
						{
							_options[option] = value.ToString();
						}
						else
						{
							_options[option] = (bool) val;
						}
					}
				}
			}
			else
			{
				throw new Exception("Unknown option '" + option + "'");
			}
		}

		private static Dictionary<string, Tuple<string, Type>> _Descriptions;

		/// <summary>
		/// Descriptions of each option, pulled from Js Lint
		/// </summary>
		private static readonly Dictionary<string, Tuple<string, Type>> DescriptionsLint = new Dictionary
			<string, Tuple<string, Type>>
		{
			{"adsafe", BoolOpt("if ADsafe should be enforced")},
			{"bitwise", BoolOpt("allow (lint) or enforce (hint) use of bitwise operators")},
			{"browser", BoolOpt("if the standard browser globals should be predefined")},
			{"cap", BoolOpt("if upper case HTML should be allowed")},
			{"css", BoolOpt("if CSS workarounds should be tolerated")},
			{"debug", BoolOpt("if debugger statements should be allowed")},
			{"devel", BoolOpt("if logging should be allowed (console, alert, etc.)")},
			{"eqeq", BoolOpt("tolerate != and ==")},
			{"es5", BoolOpt("if ES5 syntax should be allowed")},
			{"evil", BoolOpt("if eval should be allowed")},
			{"forin", BoolOpt("if unfiltered for in statements should be allowed.")},
			{"fragment", BoolOpt("if HTML fragments should be allowed")},
			{"maxerr", Tuple.Create("maximum number of errors", typeof (int))},
			{"newcap", BoolOpt("tolerate initial caps on constructor functions")},
			{"nomen", BoolOpt("if names should not be checked for initial or trailing underbars")},
			{"on", BoolOpt("if HTML event handlers should be allowed")},
			{"onevar", BoolOpt("if only one var statement per function should be allowed")},
			{"passfail", BoolOpt("if the scan should stop on first error")},
			{"predef", Tuple.Create("space seperated list of predefined globals", typeof (string))},
			{"plusplus", BoolOpt("if increment/decrement should be allowed")},
			{"regexp", BoolOpt("if the . should not be allowed in regexp literals")},
			{"rhino", BoolOpt("if the Rhino environment globals should be predefined")},
			{"safe", BoolOpt("if the safe subset rules are enforced.")},
			{"sub", BoolOpt("if subscript notation may be used for expressions better expressed in dot notation.")},
			{
				"sloppy",
				BoolOpt(
					"if the ES5 'use strict'; pragma is not required. Do not use this pragma unless you know what you are doing.")
			},
			{"strict", BoolOpt("require the \"use strict\"; pragma")},
			{"undef", BoolOpt("if variables and functions need not be declared before used. ")},
			{"unparam", BoolOpt("if warnings should not be given for unused parameters.")},
			{"unused", BoolOpt("show unused local variables")},
			{"vars", BoolOpt("if multiple var statement per function should be allowed")},
			{"windows", BoolOpt("if MS Windows-specigic globals should be predefined")},
			{"white", BoolOpt("if strict whitespace rules apply")},
			{"widget", BoolOpt("if the Yahoo Widgets globals should be predefined")}
		};

		private static readonly Dictionary<string, Tuple<string, Type>> DescriptionsHint = new Dictionary
			<string, Tuple<string, Type>>
		{
			{"asi", BoolOpt("tolerate omission of semicolons - not recommended, may break when minified")},
			{"bitwise", BoolOpt("(!) if bitwise operators should not be allowed")},
			{"boss", BoolOpt("allow the use of assignments inside structured elements")},
			{"browser", BoolOpt("if the standard browser globals should be predefined")},
			{"camelcase", BoolOpt("force all variable names to use either camelCase style or UPPER_CASE with underscores.")},
			{"couch", BoolOpt("if CouchDB globals should be predefined")},
			{"curly", BoolOpt("require curly braces around structured blocks")},
			{"debug", BoolOpt("if debugger statements should be allowed")},
			{"devel", BoolOpt("if logging should be allowed (console, alert, etc.)")},
			{"dojo", BoolOpt("if Dojo Toolkit globals should be predefined")},
			{"eqeqeq", BoolOpt("if === should be required")},
			{"eqnull", BoolOpt("if == null comparisons should be tolerated")},
			{"es3", BoolOpt("if ES3 syntax should be allowed")},
			{"evil", BoolOpt("if eval should be allowed")},
			{"esnext", BoolOpt("if es.next specific syntax should be allowed")},
			{"expr", BoolOpt("if ExpressionStatement should be allowed as Programs")},
			{"freeze", BoolOpt("prohibits overwriting prototypes of native objects such as Array, Date")},
			{"forin", BoolOpt("(!) if for in statements must filter")},
			{"funcscope", BoolOpt("(!) if only function scope should be used for scope tests")},
			{"globalstrict", BoolOpt("if global \"use strict\"; should be allowed (also enables 'strict')")},
			{"immed", BoolOpt("if immediate invocations must be wrapped in parens")},
			{"indent", IntOpt(" enforces specific tab width for your code")},
			{"iterator", BoolOpt("if the `__iterator__` property should be disallowed")},
			{"jquery", BoolOpt("if jQuery globals should be predefined")},
			{"lastsemic", BoolOpt("if semicolons may be ommitted for the trailing statements inside of a one-line blocks.")},
			{"latedef", Tuple.Create("if the use before definition should not be tolerated", typeof (object))},
			{"laxbreak", BoolOpt("if line breaks should not be checked")},
			{"laxcomma", BoolOpt(" if line breaks should not be checked around commas")},
			{"loopfunc", BoolOpt("if functions should be allowed to be defined within loops")},
			{"maxcomplexity", IntOpt("control cyclomatic complexity throughout your code")},
			{"maxdepth", IntOpt("control how nested do you want your blocks to be")},
			{"maxerr", IntOpt("maximum number of errors")},
			{"maxlen", IntOpt("set the maximum length of a line")},
			{"maxparams", IntOpt("set the max number of formal parameters allowed per function")},
			{"maxstatements", IntOpt("set the max number of statements allowed per function")},
			{"mootools", BoolOpt("if MooTools globals should be predefined")},
			{"moz", BoolOpt("code uses Mozilla JavaScript extensions")},
			{"multistr", BoolOpt("suppresses warnings about multi-line strings")},
			{"newcap", BoolOpt("if constructor names must be capitalized")},
			{"noarg", BoolOpt("if arguments.caller and arguments.callee should be disallowed")},
			{"node", BoolOpt("if the Node.js environment globals should be predefined")},
			{"noempty", BoolOpt("if empty blocks should be disallowed")},
			{"nonew", BoolOpt("if using \"new\" for side-effects should be disallowed")},
			{"nonstandard", BoolOpt("if non-standard (but widely adopted) globals should be predefined")},
			{"nomen", BoolOpt("(!) if names should be checked for initial or trailing underbars (DEPRECATED SOON)")},
			{"notypeof", BoolOpt("suppresses warnings about invalid typeof operator value")},
			{"onevar", BoolOpt("f only one var statement per function should be allowed (DEPRECATED SOON)")},
			{"passfail", BoolOpt("if the scan should stop on first error (DEPRECATED SOON)")},
			{
				"phantom",
				BoolOpt("defines globals available when your core is running inside of the PhantomJS runtime environment")
			},
			{"plusplus", BoolOpt("(!) if increment/decrement should not be allowed")},
			{"predef", StringOpt("space seperated list of predefined globals")},
			{"proto", BoolOpt("if the `__proto__` property should be disallowed")},
			{"prototypejs", BoolOpt("if Prototype and Scriptaculous globals should be predefined")},
			{"quotmark", StringOpt("enforces the consistency of quotation marks used throughout your code: true/single/double")},
			{"rhino", BoolOpt("if the Rhino environment globals should be predefined")},
			{"scripturl", BoolOpt("if script-targeted URLs should be tolerated")},
			{"strict", BoolOpt("require the \"use strict\"; pragma")},
			{"smarttabs", BoolOpt("if smarttabs should be tolerated http://www.emacswiki.org/emacs/SmartTabs")},
			{"sub", BoolOpt("if all forms of subscript notation are tolerated")},
			{"supernew", BoolOpt("if \"new function () { ... };\" and \"new Object;\"  should be tolerated")},
			{"shadow", BoolOpt("if variable shadowing should be tolerated")},
			{"trailing", BoolOpt("if trailing whitespace rules apply")},
			{"undef", BoolOpt("if variables should be declared before used")},
			{"unused", BoolOpt("show unused local variables")},
			{"validthis", BoolOpt("if \"this\" inside a non-constructor function is valid")},
			{"white", BoolOpt("(!)if strict whitespace rules apply (DEPRECATED SOON)")},
			{"worker", BoolOpt("defines globals available when your code is running inside of a Web Worker")},
			{"wsh", BoolOpt("if the Windows Scripting Host environment globals should be predefined")},
			{"yui", BoolOpt("defines globals exposed by the YUI JavaScript framework")}
		};
	}
}
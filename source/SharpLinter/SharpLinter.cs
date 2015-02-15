using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using JTC.SharpLinter.Config;
using JTC.SharpLinter.Engines;

namespace JTC.SharpLinter
{
	/// <summary>
	/// Constructs an object capable of linting javascript files and returning the result of JS Lint
	/// </summary>
	public class SharpLinter
	{
		public JavascriptExecutor Engine = new JavascriptExecutor();

		#region constructor

		public SharpLinter(JsLintConfiguration config)
		{
			Configuration = config;
			Process();
		}

		protected void Process()
		{
			//var _context = new Engines.JavascriptExecutor();
			//_context = new JavascriptContext();

			if (String.IsNullOrEmpty(Configuration.JsLintCode))
			{
				throw new Exception("No JSLINT/JSHINT code was specified in the configuration.");
			}
			JSLint = Configuration.JsLintCode;

			Engine.Run(JSLint);

			var func = Configuration.LinterType == LinterType.JSHint ? "JSHINT" : "JSLINT";

			// a bug (apparently) in the Noesis wrapper causes a StackOverflow exception when returning data sometimes.
			// not sure why but removing "functions" from the returned object resolves it. we don't need that
			// anyway.

			var run =
				@"function lintRunner(dataCollector, javascript, options) {
                    var data, result = JSLINT(javascript,options);
                    
                    if (!result) {
                        data = JSLINT.data();
                        if (data.functions) {
                            delete data.functions;
                        }
                        dataCollector.ProcessData(data);
                    }
                }
            ".Replace("JSLINT", func);

			//_context.SetRunFunction(run);

			Engine.Run(run);
		}

		#endregion

		#region private methods

		private readonly object _lock = new Object();

		/// <summary>
		/// Map of lines that should be excluded (true for an index means exclude that line)
		/// </summary>
		protected List<bool> LineExclusion;

		/// <summary>
		/// The script that gets run
		/// </summary>
		protected string JSLint;

		protected JsLintConfiguration Configuration;

		#endregion

		#region public methods

		protected void Configure()
		{
			_isStartScriptRegex = new Regex(@"<script (.|\n)*?type\s*=\s*[""|']text/javascript[""|'](.|\n)*?>");
			// skip anything with a src=".."
			_isStartScriptRegexFail = new Regex(@"<script (.|\n)*?src\s*=\s*[""|'].*?[""|'](.|\n)*?>");
			if (!String.IsNullOrEmpty(Configuration.IgnoreEnd) &&
				!String.IsNullOrEmpty(Configuration.IgnoreStart))
			{
				_isIgnoreStart = new Regex(@"/\*\s*" + Configuration.IgnoreStart + @"\s*\*/");
				_isIgnoreEnd = new Regex(@"/\*\s*" + Configuration.IgnoreEnd + @"\s*\*/");
				isIgnoreStart = isIgnoreStartImpl;
				isIgnoreEnd = isIgnoreEndImpl;
			}
			else
			{
				isIgnoreStart = notImplemented;
				isIgnoreEnd = notImplemented;
			}
			if (!String.IsNullOrEmpty(Configuration.IgnoreFile))
			{
				_isIgnoreFile = new Regex(@"/\\s*" + Configuration.IgnoreFile + @"\s*\*/");
				isIgnoreFile = isIgnoreFileImpl;
			}
			else
			{
				isIgnoreFile = notImplemented;
			}
		}

		public JsLintResult Lint(string javascript)
		{
			var finalJs = new StringBuilder();
			Configure();

			lock (_lock)
			{
				var hasSkips = false;
				var hasUnused = false;
				if (Configuration.LinterType == LinterType.JSLint)
				{
					hasUnused = Configuration.GetOption<bool>("unused");
				}
				else if (Configuration.LinterType == LinterType.JSLint)
				{
					// we consider the "unused" option to be activated if the config value is either empty
					// (since the default is "true") or anything other than "false"
					var unusedConfig = Configuration.GetOption<string>("unused");
					hasUnused = string.IsNullOrEmpty(unusedConfig) || unusedConfig != "false";
				}
				var dataCollector = new LintDataCollector(hasUnused);

				LineExclusion = new List<bool>();
				// lines are evaluated, but errors are ignored: we want to use this for blocks excluded 
				// within a javascript file, because otherwise the parser will freak out if other parts of the
				// code wouldn't validate if that block were missing
				var ignoreErrors = false;

				// lines are not evaluted by the parser at all - in HTML files we want to pretend non-JS lines
				// are not even there.
				var ignoreLines = Configuration.InputType == InputType.Html;

				var startSkipLine = 0;

				using (var reader = new StringReader(javascript))
				{
					string text;
					var line = 0;

					while ((text = reader.ReadLine()) != null)
					{
						line++;

						if (!ignoreLines
							&& Configuration.InputType == InputType.Html && isEndScript(text))
						{
							ignoreLines = true;
						}

						if (!ignoreErrors && isIgnoreStart(text))
						{
							startSkipLine = line;
							ignoreErrors = true;
							hasSkips = true;
						}
						// always check for end - if they both appear on a line, don't do anything. should 
						// always fall back to continuing to check.
						if (ignoreErrors && isIgnoreEnd(text))
						{
							ignoreErrors = false;
						}
						LineExclusion.Add(ignoreErrors);

						finalJs.AppendLine(ignoreLines ? "" : text);

						if (ignoreLines
							&& Configuration.InputType == InputType.Html
							&& isStartScript(text))
						{
							ignoreLines = false;
						}
					}
				}
				if (ignoreErrors)
				{
					// there was no ignore-end found, so cancel the results 
					var err = new JsLintData();
					err.Line = startSkipLine;
					err.Character = 0;
					err.Reason = "An ignore-start marker was found, but there was no ignore-end. Nothing was ignored.";
					dataCollector.Errors.Add(err);

					hasSkips = false;
				}


				if (finalJs.Length == 0)
				{
					var err = new JsLintData();
					err.Line = 0;
					err.Character = 0;
					err.Reason = "The file was empty.";
					dataCollector.Errors.Add(err);
				}
				else
				{
					//var _context = new Engines.JavascriptExecutor();

					// Setting the externals parameters of the context
					/*_context.SetParameter("dataCollector", dataCollector);
                    _context.SetParameter("javascript", finalJs.ToString());
                    _context.SetParameter("options", Configuration.ToJsOptionVar());*/


					// Running the script
					//_context.Run("lintRunner(dataCollector, javascript, options);");
					Engine.CallFunction("lintRunner", dataCollector, finalJs.ToString(), Configuration.ToJsOptionVar());
				}

				var result = new JsLintResult();
				result.Errors = new List<JsLintData>();

				var index = 0;
				while (result.Errors.Count <= Configuration.MaxErrors
						&& index < dataCollector.Errors.Count)
				{
					var error = dataCollector.Errors[index++];
					if (!hasSkips)
					{
						result.Errors.Add(error);
					}
					else
					{
						if (error.Line >= 0 && error.Line < LineExclusion.Count)
						{
							if (!LineExclusion[error.Line - 1])
							{
								result.Errors.Add(error);
							}
						}
						else
						{
							result.Errors.Add(error);
						}
					}
				}
				// if we went over, mark that there were more errors and remove last one
				if (result.Errors.Count > Configuration.MaxErrors)
				{
					result.Errors.RemoveAt(result.Errors.Count - 1);
					result.Limited = true;
				}

				return result;
			}
		}

		#endregion

		#region private methods

		private Regex _isStartScriptRegex = new Regex(@"<script (.|\n)*?type\s*=\s*[""|']text/javascript[""|'](.|\n)*?>");
		// skip anything with a src=".."
		private Regex _isStartScriptRegexFail = new Regex(@"<script (.|\n)*?src\s*=\s*[""|'].*?[""|'](.|\n)*?>");


		/// <summary>
		/// Returns true if a script appears on this line.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		protected bool isStartScript(string text)
		{
			var result = false;
			var matches = _isStartScriptRegex.Matches(text);
			if (matches.Count > 0)
			{
				// just check the last one, if for some reason there's a block
				// opened & closed and followed by an include all on one line, then
				// just don't deal with it.
				result = !_isStartScriptRegexFail.IsMatch(matches[matches.Count - 1].Value);
			}
			return result;
		}

		private readonly Regex _isEndScriptRegex = new Regex(@"</script.*?>");

		protected bool isEndScript(string text)
		{
			return _isEndScriptRegex.IsMatch(text);
		}

		private Regex _isIgnoreStart;
		private Regex _isIgnoreEnd;
		private Regex _isIgnoreFile;

		protected bool notImplemented(string what)
		{
			return false;
		}

		protected Func<string, bool> isIgnoreStart;

		protected bool isIgnoreStartImpl(string text)
		{
			return _isIgnoreStart.IsMatch(text);
		}

		protected Func<string, bool> isIgnoreEnd;

		protected bool isIgnoreEndImpl(string text)
		{
			return _isIgnoreEnd.IsMatch(text);
		}

		protected Func<string, bool> isIgnoreFile;

		protected bool isIgnoreFileImpl(string text)
		{
			return _isIgnoreFile.IsMatch(text);
		}

		#endregion
	}
}
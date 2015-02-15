using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SharpLinter.Config;
using SharpLinter.Engines;

namespace SharpLinter
{
	/// <summary>
	/// Constructs an object capable of linting javascript files and returning the result of JS Lint
	/// </summary>
	public class SharpLinter
	{
		private readonly JsLintConfiguration _configuration;
		private readonly JavascriptExecutor _engine = new JavascriptExecutor();
		private readonly Regex _isEndScriptRegex = new Regex(@"</script.*?>");
		private readonly object _lock = new object();
		private Regex _isIgnoreEndRegex;
		private Regex _isIgnoreFileRegex;
		private Regex _isIgnoreStartRegex;
		private Regex _isStartScriptRegex = new Regex(@"<script (.|\n)*?type\s*=\s*[""|']text/javascript[""|'](.|\n)*?>");
		// skip anything with a src=".."
		private Regex _isStartScriptRegexFail = new Regex(@"<script (.|\n)*?src\s*=\s*[""|'].*?[""|'](.|\n)*?>");

		/// <summary>
		/// The script that gets run
		/// </summary>
		private string _jsLint;

		/// <summary>
		/// Map of lines that should be excluded (true for an index means exclude that line)
		/// </summary>
		private List<bool> _lineExclusion;

		private Func<string, bool> _isIgnoreEnd;
		private Func<string, bool> _isIgnoreFile;
		private Func<string, bool> _isIgnoreStart;

		public SharpLinter(JsLintConfiguration config)
		{
			_configuration = config;
			Process();
		}

		private void Process()
		{
			//var _context = new Engines.JavascriptExecutor();
			//_context = new JavascriptContext();

			if (String.IsNullOrEmpty(_configuration.JsLintCode))
			{
				throw new Exception("No JSLINT/JSHINT code was specified in the configuration.");
			}
			_jsLint = _configuration.JsLintCode;

			_engine.Run(_jsLint);

			var func = _configuration.LinterType == LinterType.JSHint ? "JSHINT" : "JSLINT";

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

			_engine.Run(run);
		}

		private void Configure()
		{
			_isStartScriptRegex = new Regex(@"<script (.|\n)*?type\s*=\s*[""|']text/javascript[""|'](.|\n)*?>");
			// skip anything with a src=".."
			_isStartScriptRegexFail = new Regex(@"<script (.|\n)*?src\s*=\s*[""|'].*?[""|'](.|\n)*?>");
			if (!String.IsNullOrEmpty(_configuration.IgnoreEnd) &&
				!String.IsNullOrEmpty(_configuration.IgnoreStart))
			{
				_isIgnoreStartRegex = new Regex(@"/\*\s*" + _configuration.IgnoreStart + @"\s*\*/");
				_isIgnoreEndRegex = new Regex(@"/\*\s*" + _configuration.IgnoreEnd + @"\s*\*/");
				_isIgnoreStart = IsIgnoreStartImpl;
				_isIgnoreEnd = IsIgnoreEndImpl;
			}
			else
			{
				_isIgnoreStart = NotImplemented;
				_isIgnoreEnd = NotImplemented;
			}
			if (!String.IsNullOrEmpty(_configuration.IgnoreFile))
			{
				_isIgnoreFileRegex = new Regex(@"/\\s*" + _configuration.IgnoreFile + @"\s*\*/");
				_isIgnoreFile = IsIgnoreFileImpl;
			}
			else
			{
				_isIgnoreFile = NotImplemented;
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
				if (_configuration.LinterType == LinterType.JSLint)
				{
					hasUnused = _configuration.GetOption<bool>("unused");
				}
				else if (_configuration.LinterType == LinterType.JSLint)
				{
					// we consider the "unused" option to be activated if the config value is either empty
					// (since the default is "true") or anything other than "false"
					var unusedConfig = _configuration.GetOption<string>("unused");
					hasUnused = string.IsNullOrEmpty(unusedConfig) || unusedConfig != "false";
				}
				var dataCollector = new LintDataCollector(hasUnused);

				_lineExclusion = new List<bool>();
				// lines are evaluated, but errors are ignored: we want to use this for blocks excluded 
				// within a javascript file, because otherwise the parser will freak out if other parts of the
				// code wouldn't validate if that block were missing
				var ignoreErrors = false;

				// lines are not evaluted by the parser at all - in HTML files we want to pretend non-JS lines
				// are not even there.
				var ignoreLines = _configuration.InputType == InputType.Html;

				var startSkipLine = 0;

				using (var reader = new StringReader(javascript))
				{
					string text;
					var line = 0;

					while ((text = reader.ReadLine()) != null)
					{
						line++;

						if (!ignoreLines
							&& _configuration.InputType == InputType.Html && IsEndScript(text))
						{
							ignoreLines = true;
						}

						if (!ignoreErrors && _isIgnoreStart(text))
						{
							startSkipLine = line;
							ignoreErrors = true;
							hasSkips = true;
						}
						// always check for end - if they both appear on a line, don't do anything. should 
						// always fall back to continuing to check.
						if (ignoreErrors && _isIgnoreEnd(text))
						{
							ignoreErrors = false;
						}
						_lineExclusion.Add(ignoreErrors);

						finalJs.AppendLine(ignoreLines ? "" : text);

						if (ignoreLines
							&& _configuration.InputType == InputType.Html
							&& IsStartScript(text))
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
					_engine.CallFunction("lintRunner", dataCollector, finalJs.ToString(), _configuration.ToJsOptionVar());
				}

				var result = new JsLintResult();
				result.Errors = new List<JsLintData>();

				var index = 0;
				while (result.Errors.Count <= _configuration.MaxErrors
						&& index < dataCollector.Errors.Count)
				{
					var error = dataCollector.Errors[index++];
					if (!hasSkips)
					{
						result.Errors.Add(error);
					}
					else
					{
						if (error.Line >= 0 && error.Line < _lineExclusion.Count)
						{
							if (!_lineExclusion[error.Line - 1])
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
				if (result.Errors.Count > _configuration.MaxErrors)
				{
					result.Errors.RemoveAt(result.Errors.Count - 1);
					result.Limited = true;
				}

				return result;
			}
		}

		/// <summary>
		/// Returns true if a script appears on this line.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		private bool IsStartScript(string text)
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

		private bool IsEndScript(string text)
		{
			return _isEndScriptRegex.IsMatch(text);
		}

		private static bool NotImplemented(string what)
		{
			return false;
		}

		private bool IsIgnoreStartImpl(string text)
		{
			return _isIgnoreStartRegex.IsMatch(text);
		}

		private bool IsIgnoreEndImpl(string text)
		{
			return _isIgnoreEndRegex.IsMatch(text);
		}

		private bool IsIgnoreFileImpl(string text)
		{
			return _isIgnoreFileRegex.IsMatch(text);
		}
	}
}
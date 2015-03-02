using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Reflection;

namespace SharpLinter
{
	public class LintDataCollector
	{
		private readonly bool _processUnuseds;

		public LintDataCollector(bool processUnuseds)
		{
			_processUnuseds = processUnuseds;
		}

		public List<JsLintData> Errors { get; } = new List<JsLintData>();

        public void ProcessData(ExpandoObject data)
		{
            ExpandoObject errors = data.Get("errors");

            if (errors!=null)
            {
                ProcessErrors(errors, error =>
                {
                    var jsError = new JsLintData { Source = "lint" };

                    if (error.Key=="line")
                    {
                        jsError.Line = Convert.ToInt32(error.Value);
                    }

                    if (error.Key == "character")
                    {
                        jsError.Character = Convert.ToInt32(error.Value);
                    }

                    if (error.Key == "reason")
                    {
                        jsError.Reason = (string)error.Value;
                    }

                    Errors.Add(jsError);
                });
            }

            ExpandoObject unuseds = data.Get("unused");

            if (_processUnuseds && unuseds!=null)
            {
                var lastLine = -1;
                JsLintData jsError = null;
                var unusedList = String.Empty;
                var unusedCount = 0;
                ProcessErrors(unuseds, unused =>
                {
                    var line = 0;
                    if (unused.Key=="line")
                    {
                        line = Convert.ToInt32(unused.Value);
                    }
                    if (line != lastLine)
                    {
                        if (jsError != null)
                        {
                            jsError.Reason = "Unused Variable" + (unusedCount > 1 ? "s " : " ") + unusedList;
                            Errors.Add(jsError);
                        }
                        jsError = new JsLintData { Source = "lint", Character = -1, Line = line };
                        unusedCount = 0;
                        unusedList = String.Empty;
                    }

                    if (unused.Key=="name")
                    {
                        unusedList += (unusedCount == 0 ? String.Empty : ", ") + unused.Value;
                        unusedCount++;
                    }
                    lastLine = line;
                });
                jsError.Reason = "Unused Variable" + (unusedCount > 1 ? "s " : " ") + unusedList;
                Errors.Add(jsError);
            }
        }

        private static void ProcessListOfObject(object obj, Action<Dictionary<string, object>> processor)
        {
            var array = obj as object[];

            if (array == null) return;
            foreach (var objItemDictionary in array.OfType<Dictionary<string, object>>())
            {
                processor(objItemDictionary);
            }
        }

        private static void ProcessErrors(ExpandoObject errors, Action<KeyValuePair<string, object>> processor)
        {
            foreach (var objItemDictionary in errors.ToDictionary())
            {
                processor(objItemDictionary);
            }
        }
    }
}
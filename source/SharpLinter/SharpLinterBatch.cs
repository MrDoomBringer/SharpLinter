﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JTC.SharpLinter.Config
{
    public class SharpLinterBatch
    {
        public JsLintConfiguration Configuration
        {
            get;
            set;
        }
        public string OutputFormat
        {
            get
            {
                if (HasCustomOutputFormat())
                {
                    return Configuration.OutputFormat;
                }
                else
                {
                    return _OutputFormat;
                }
            }
            set
            {
                _OutputFormat = value;
            }
        }

        private string _OutputFormat;
        public IEnumerable<PathInfo> FilePaths { get; set; }

        public SharpLinterBatch(JsLintConfiguration configuration)
        {
            Configuration = configuration;
            OutputFormat = "{0}({1}): ({2}) {3} {4}";
        }

        protected string StringOrMissingDescription(string text)
        {
            return String.IsNullOrEmpty(text) ? "[None Specified]" : text;

        }

        public int Process()
        {
            SharpLinter lint = new SharpLinter(Configuration);
            List<string> SummaryInfo = new List<string>();
            
            if (Configuration.Verbosity == Verbosity.Debugging)
            {
                Console.WriteLine(String.Format("SharpLinter: Beginning processing at {0:MM/dd/yy H:mm:ss zzz}", DateTime.Now));
                Console.WriteLine(String.Format("Global configuration file: {0}",StringOrMissingDescription(Configuration.GlobalConfigFilePath)));
                Console.WriteLine(String.Format("JSLINT path: {0}", StringOrMissingDescription(Configuration.JsLintFilePath)));
                Console.WriteLine(String.Format("Using linter options for {0}, {1}",Configuration.LinterType.ToString(),StringOrMissingDescription(Configuration.JsLintVersion)));
                Console.WriteLine("LINT options: " + StringOrMissingDescription(Configuration.OptionsToString()));
                Console.WriteLine("LINT globals: " + StringOrMissingDescription(Configuration.GlobalsToString()));
                Console.WriteLine("Sharplint: ignorestart={0}, ignoreend={1}, ignorefile={2}",Configuration.IgnoreStart,Configuration.IgnoreEnd,Configuration.IgnoreFile);
                Console.WriteLine("Input paths: (working directory=" + Directory.GetCurrentDirectory()+")");
                foreach (var file in FilePaths)
                {
                    Console.WriteLine("    " + file.Path);
                }
                Console.WriteLine("Exclude file masks:");
                foreach (var file in Configuration.ExcludeFiles)
                {
                    Console.WriteLine("    " + file);
                }
                Console.WriteLine("----------------------------------------");
            }
            int fileCount = 0;

            List<JsLintData> allErrors = new List<JsLintData>();
            
            
            foreach (string file in Configuration.GetMatchedFiles(FilePaths))
            {
                List<JsLintData> fileErrors = new List<JsLintData>();
                bool lintErrors=false;
                bool YUIErrors=false;
                fileCount++;
                string javascript = File.ReadAllText(file);
                if (javascript.IndexOf("/*" + Configuration.IgnoreFile + "*/") >= 0)
                {
                    continue;
                }

                var ext = Path.GetExtension(file).ToLower();
                Configuration.InputType = (ext == ".js" || ext == ".javascript") ?
                    InputType.JavaScript :
                    InputType.Html;

                JsLintResult result = lint.Lint(javascript);
                bool hasErrors = result.Errors.Count > 0;

                if (hasErrors)
                {
                    lintErrors = true;
                    foreach (JsLintData error in result.Errors)
                    {
                        error.FilePath = file;
                        fileErrors.Add(error);
                    }
                    string leadIn = String.Format("{0}: Lint found {1} errors.", file, result.Errors.Count);
                   
                   
                    if (result.Limited) {
                        leadIn += String.Format(" Stopped processing due to maxerr={0} option.", Configuration.MaxErrors);
                    }
                    SummaryInfo.Add(leadIn);
                }
				
                string successLine = String.Empty;
                if (!(lintErrors || YUIErrors))
                {
                    successLine = String.Format("{0}: No errors found.", file);
					
                    SummaryInfo.Add(successLine);
                }
                fileErrors.Sort(LintDataComparer);
                allErrors.AddRange(fileErrors);                
            }
            if (Configuration.Verbosity == Verbosity.Debugging || Configuration.Verbosity == Verbosity.Summary)
            {
                // Output file-by-file results at beginning
                foreach (string item in SummaryInfo)
                {
                    Console.WriteLine(item);
                }
            }
            

            if (allErrors.Count > 0)
            {
                if (Configuration.Verbosity == Verbosity.Debugging) {
                    Console.WriteLine();
                    Console.WriteLine("Error Details:");
                    Console.WriteLine();
                }

                foreach (JsLintData error in allErrors)
                {
                    string character = error.Character.ToString();
                    // add a small introducing string before the character number if we are using the default output string
                    if (!HasCustomOutputFormat())
                    {
                        character = error.Character >= 0 ? "at character " + error.Character : String.Empty;
                    }
                    Console.WriteLine(string.Format(OutputFormat, error.FilePath, error.Line, error.Source, error.Reason, character));
                }
            }
            if (Configuration.Verbosity == Verbosity.Debugging)
            {
                Console.WriteLine();
                Console.WriteLine("SharpLinter: Finished processing at {0:MM/dd/yy H:mm:ss zzz}. Processed {1} files.", DateTime.Now, fileCount);
            }

            return allErrors.Count;
        }

        private string MapFileName(string path, string mask)
        {
            if (mask.OccurrencesOf("*") != 1)
            {
                throw new Exception("Invalid mask '" + mask + "' for compressing output. It must have a single wildcard.");
            }

            string maskStart = mask.Before("*");
            string maskEnd = mask.AfterLast("*").BeforeLast(".");
            string maskExt = mask.AfterLast(".");

            string pathBase = path.BeforeLast(".");
            return maskStart + pathBase + maskEnd + "." + maskExt;

        }

        private int LintDataComparer(JsLintData x, JsLintData y)
        {
            return x.Line.CompareTo(y.Line);
        }

        private bool HasCustomOutputFormat()
        {
            return !String.IsNullOrEmpty(Configuration.OutputFormat);
        }
    }
}

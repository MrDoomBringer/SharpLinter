using System;
using Jint;

namespace JTC.SharpLinter.Engines
{
    public class JavascriptExecutor
    {
        public JavascriptExecutor()
        {
            Context = new Engine();
        }

		/// <summary>
		/// Gets or sets the Javascript Context.
		/// </summary>
        protected Engine Context { get; private set; }

        public object Run(string code)
        {
            try
            {
                return Context.Execute(code);
            }
            catch(Exception e)
            {
                throw new Exception("An error was reported by the javascript engine: " + e.Message);
            }
        }

		public void SetParameter()
		{
			// Set parameters of context?
			Context.
		}
    }
}

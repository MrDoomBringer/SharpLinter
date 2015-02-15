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
		protected Engine Context { get; }

		public void Run(string code)
		{
			try
			{
				Context.Execute(code);
			}
			catch (Exception e)
			{
				throw new Exception("An error was reported by the javascript engine: " + e.Message);
			}
		}

		public void CallFunction(string funcName, params object[] arguments)
		{
			try
			{
				Context.Invoke(funcName, arguments);
			}
			catch (Exception e)
			{
				throw new Exception("An error was reported by the javascript engine: " + e.Message);
			}
		}

		public void SetParameter(string name, object value)
		{
			Context.SetValue(name, value);
		}
	}
}
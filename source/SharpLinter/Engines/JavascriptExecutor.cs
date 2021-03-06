﻿using System;
using Jint;

namespace SharpLinter.Engines
{
	public sealed class JavascriptExecutor
	{
		public JavascriptExecutor()
		{
			Context = new Engine();
		}

		/// <summary>
		/// Gets or sets the Javascript Context.
		/// </summary>
		private Engine Context { get; }

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

		public Jint.Native.JsValue CallFunction(string funcName, params object[] arguments)
		{
			try
			{
				return Context.Invoke(funcName, arguments);
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
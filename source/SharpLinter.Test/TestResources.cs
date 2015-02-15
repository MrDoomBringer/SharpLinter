using System.IO;
using System.Reflection;
using SharpLinter.Config;

namespace SharpLinter.Test
{
	internal class TestResources
	{
		public static JsLintConfiguration DefaultConfig
		{
			get
			{
				var config = new JsLintConfiguration();
				config.SetOption("browser");
				config.SetOption("bitwise");
				config.SetOption("evil");
				config.SetOption("eqeqeq");
				config.SetOption("plusplus");
				config.SetOption("forin");
				config.SetOption("immed");
				config.SetOption("newcap");
				config.SetOption("undef");
				return config;
			}
		}

		public static string GetAppRootedPath(string relativePath)
		{
			var exePath = Assembly.GetAssembly(typeof (TestResources)).Location;
			var rootFolder = "SharpLinter.Test";
			var rootPos = exePath.IndexOf(rootFolder + "\\") + rootFolder.Length + 1;
			if (relativePath.Length > 0 && relativePath[0] == '\\')
			{
				relativePath = relativePath.Substring(1);
			}
			return exePath.Substring(0, rootPos) + relativePath.Replace("/", "\\");
		}

		public static string LoadAppRootedFile(string relativePath)
		{
			return File.ReadAllText(GetAppRootedPath(relativePath));
		}
	}
}
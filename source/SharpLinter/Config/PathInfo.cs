namespace JTC.SharpLinter.Config
{
	public class PathInfo
	{
		public PathInfo(string path, bool recurse)
		{
			Path = path;
			Recurse = recurse;
		}

		public string Path { get; set; }
		public bool Recurse { get; set; }
	}
}
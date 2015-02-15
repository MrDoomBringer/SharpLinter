namespace SharpLinter.Config
{
	public class PathInfo
	{
		public PathInfo(string path, bool recurse)
		{
			Path = path;
			Recurse = recurse;
		}

		public string Path { get; }
		public bool Recurse { get; }
	}
}
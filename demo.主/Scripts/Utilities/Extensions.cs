using Godot;

namespace YourProject.Scripts.Utilities
{
	public static class Extensions
	{
		public static T GetNodeSafe<T>(this Node node, string path) where T : class
		{
			return node.GetNodeOrNull(path) as T;
		}

		public static void SafeConnect(this Button button, string signal, Callable callable)
		{
			if (!button.IsConnected(signal, callable))
			{
				button.Connect(signal, callable);
			}
		}
	}
}

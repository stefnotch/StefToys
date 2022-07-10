using System.Runtime.InteropServices;
using Wox.Infrastructure;
using Wox.Plugin;

namespace StefToys
{
    public class Main : IPlugin
    {
        private PluginInitContext? Context { get; set; }

        public string Name => "VS Code Launcher";
        public string Description => "Opens Visual Studio Code in the currently open directory";

        public void Init(PluginInitContext context)
        {

            Context = context;
        }

        public static string TrimEndToLength(string text, int maxLength)
        {
            string Ellipses = "…";
            if (maxLength < Ellipses.Length) return text;

            maxLength -= Ellipses.Length;
            if (text.Length <= maxLength) return text;

            int charactersToTrim = text.Length - maxLength;
            return Ellipses + text[charactersToTrim..];
        }

        public List<Result> Query(Query query)
        {
            var openExplorerPaths = GetOpenExplorerPaths();

            int maxPathLength = 28;
            return openExplorerPaths
                .Select(path => new Result()
                {
                    Title = "Open VS Code in " + TrimEndToLength(path, maxPathLength),
                    Action = e =>
                    {
                        Helper.OpenInShell("code", ".", path, runWithHiddenWindow: true);
                        return true;
                    }
                })
                .ToList();
        }

        public static IEnumerable<string> GetOpenExplorerPaths()
        {
            Type? type = Type.GetTypeFromProgID("Shell.Application");
            if (type == null) yield break;
            dynamic? shell = Activator.CreateInstance(type);
            if (shell == null) yield break;
            try
            {
                var openWindows = shell.Windows();
                for (int i = 0; i < openWindows.Count; i++)
                {
                    var window = openWindows.Item(i);
                    if (window == null) continue;
                    var fileName = Path.GetFileName((string)window.FullName);
                    if (fileName.ToLower() == "explorer.exe")
                    {
                        yield return new Uri(window.LocationURL).LocalPath; // window.document.focuseditem.path;
                    }
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
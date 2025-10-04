
namespace fast_cli_tool.Models
{
    public class PathItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string SelectedCli { get; set; } = "gemini.cmd"; // 預設值
    }
}

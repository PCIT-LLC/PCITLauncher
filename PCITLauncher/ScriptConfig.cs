namespace PCITLauncher;

using System.Text.Json;
using System.Text.Json.Serialization;

public class LauncherConfig
{
    public string RepoUrl { get; set; } = "";
    public string Branch { get; set; } = "main";
    public bool AutoUpdate { get; set; } = true;
}

public class ScriptConfig
{
    public List<Category> Categories { get; set; } = new();
}

public class Category
{
    public string Name { get; set; } = "";
    public List<ScriptEntry> Scripts { get; set; } = new();
}

public class ScriptEntry
{
    public string Label { get; set; } = "";
    public string Script { get; set; } = "";
    public bool Elevated { get; set; }

    [JsonIgnore]
    public string FullPath { get; set; } = "";
}

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    public NotesPlugin() => Directory.CreateDirectory("../Notes");
}

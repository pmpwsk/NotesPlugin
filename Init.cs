namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    public NotesPlugin()
    {
        Directory.CreateDirectory("../Notes");

        //backwards compatibility
        foreach (var userTableDir in new DirectoryInfo("../Notes").GetDirectories("*", SearchOption.TopDirectoryOnly))
        {
            var files = userTableDir.GetFiles("*", SearchOption.TopDirectoryOnly);
            foreach (var userId in files.Select(x => x.Name.Before('-')).Distinct())
                Directory.CreateDirectory($"../Notes/{userTableDir.Name}/{userId}");
            foreach (var file in files)
                file.MoveTo($"../Notes/{userTableDir.Name}/{file.Name.Before('-')}/{file.Name.After('-')}");
        }
    }
}

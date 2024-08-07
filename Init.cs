﻿namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    public NotesPlugin()
    {
        Directory.CreateDirectory("../NotesPlugin.Profiles");

        //backwards compatibility
        foreach (var userTableDir in new DirectoryInfo("../NotesPlugin.Profiles").GetDirectories("*", SearchOption.TopDirectoryOnly))
        {
            var files = userTableDir.GetFiles("*", SearchOption.TopDirectoryOnly);
            foreach (var userId in files.Select(x => x.Name.Before('-')).Distinct())
                Directory.CreateDirectory($"../NotesPlugin.Profiles/{userTableDir.Name}/{userId}");
            foreach (var file in files)
                file.MoveTo($"../NotesPlugin.Profiles/{userTableDir.Name}/{file.Name.Before('-')}/{file.Name.After('-')}");
        }
    }
}

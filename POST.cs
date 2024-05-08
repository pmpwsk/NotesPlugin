using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    public override async Task Handle(PostRequest req, string path, string pathPrefix)
    {
        if (!req.LoggedIn)
        {
            req.Status = 403;
            return;
        }

        var notes = GetOrCreate(req.User.Id, req.UserTable.Name);

        string pluginHome = pathPrefix == "" ? "/" : pathPrefix;

        switch (path)
        {
            case "/save":
                {
                    if (req.IsForm)
                        req.Status = 400;
                    else if (!req.Query.TryGetValue("id", out string? id))
                        req.Status = 400;
                    else if (!notes.Notes.TryGetValue(id, out var note))
                        req.Status = 404;
                    else if (note.IsFolder)
                        req.Status = 400;
                    else
                    {
                        notes.Lock();
                        File.WriteAllText($"../Notes/{req.UserTable.Name}/{req.User.Id}/{id}.txt", await req.GetBodyText());
                        note.Changed = DateTime.UtcNow;
                        notes.UnlockSave();
                        if (note.ParentId == "default")
                            await req.Context.Response.WriteAsync(pluginHome);
                        else await req.Context.Response.WriteAsync(pluginHome + "?id=" + note.ParentId);
                    }
                }
                break;
            default:
                req.Status = 404;
                break;
        }
    }
}

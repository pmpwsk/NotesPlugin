namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    public override async Task Handle(ApiRequest req, string path, string pathPrefix)
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
            case "/get":
                {
                    if (!req.Query.TryGetValue("id", out string? id)) req.Status = 400;
                    else if (!notes.Notes.TryGetValue(id, out var note)) req.Status = 404;
                    else if (note.IsFolder) req.Status = 400;
                    else
                    {
                        var content = File.ReadAllText($"../Notes/{req.UserTable.Name}/{req.User.Id}-{id}.txt");
                        if (content == "")
                        {
                            req.Status = 201;
                        }
                        else
                        {
                            await req.Write(content);
                        }
                    }
                }
                break;
            case "/create-note":
                {
                    if (!req.Query.TryGetValue("id", out string? id)) req.Status = 400;
                    else if (!req.Query.TryGetValue("name", out string? name)) req.Status = 400;
                    else if (!notes.Notes.TryGetValue(id, out var note)) req.Status = 404;
                    else if (!note.IsFolder) req.Status = 400;
                    else
                    {
                        var n = new NoteItem(name, id, false);
                        string nId;
                        do nId = Parsers.RandomString(7);
                        while (notes.Notes.ContainsKey(nId));
                        notes.Lock();
                        notes.Notes[nId] = n;
                        notes.UnlockSave();
                        File.AppendAllLines($"../Notes/{req.UserTable.Name}/{req.User.Id}-{id}.txt", [ nId ]);
                        File.WriteAllText($"../Notes/{req.UserTable.Name}/{req.User.Id}-{nId}.txt", "");
                        await req.Write(pluginHome + "?id=" + nId);
                    }
                }
                break;
            case "/create-folder":
                {
                    if (!req.Query.TryGetValue("id", out string? id)) req.Status = 400;
                    else if (!req.Query.TryGetValue("name", out string? name)) req.Status = 400;
                    else if (!notes.Notes.TryGetValue(id, out var note)) req.Status = 404;
                    else if (!note.IsFolder) req.Status = 400;
                    else
                    {
                        var n = new NoteItem(name, id, true);
                        string nId;
                        do nId = Parsers.RandomString(7);
                        while (notes.Notes.ContainsKey(nId));
                        notes.Lock();
                        notes.Notes[nId] = n;
                        notes.UnlockSave();
                        File.AppendAllLines($"../Notes/{req.UserTable.Name}/{req.User.Id}-{id}.txt", [ nId ]);
                        File.WriteAllLines($"../Notes/{req.UserTable.Name}/{req.User.Id}-{nId}.txt", []);
                        await req.Write(pluginHome + "?id=" + nId);
                    }
                }
                break;
            case "/delete":
                {
                    if (!req.Query.TryGetValue("id", out string? id)) req.Status = 400;
                    else if (!notes.Notes.TryGetValue(id, out var note)) req.Status = 404;
                    else if (note.ParentId == null) req.Status = 400;
                    else
                    {
                        notes.Lock();
                        notes.Delete(req.User.Id, req.UserTable.Name, id, true);
                        notes.UnlockSave();
                        if (note.ParentId != "default")
                            await req.Write(pluginHome + "?id=" + note.ParentId);
                        else await req.Write(pluginHome);
                    }
                }
                break;
            case "/rename":
                {
                    if (!req.Query.TryGetValue("id", out string? id)) req.Status = 400;
                    else if (!req.Query.TryGetValue("name", out string? name)) req.Status = 400;
                    else if (!notes.Notes.TryGetValue(id, out var note)) req.Status = 404;
                    else if (note.ParentId == null) req.Status = 400;
                    else
                    {
                        if (note.Name != name)
                        {
                            notes.Lock();
                            note.Name = name;
                            notes.UnlockSave();
                        }
                        await req.Write(pluginHome + "?id=" + id);
                    }
                }
                break;
            default:
                req.Status = 404;
                break;
        }
    }
}

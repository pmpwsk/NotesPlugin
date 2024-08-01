using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    private async Task HandleOther(Request req)
    {
        switch (req.Path)
        {
            case "/":
            case "/list":
            { CreatePage(req, "Notes", out var page, out var e, out var notes);
                var id = req.Query.TryGet("id");
                if (req.Path == "/")
                    if (id == null)
                        id = "default";
                    else throw new BadRequestSignal();
                else if (id == null)
                    throw new BadRequestSignal();
                if (!notes.Notes.TryGetValue(id, out NoteItem? note))
                    throw new NotFoundSignal();
                if (!note.IsFolder)
                    throw new BadRequestSignal();
                string filePath = $"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{id}.txt";
                page.Title = note.ParentId == null ? "Notes" : $"{note.Name} - Notes";
                string parentLink = note.ParentId == null ? "/" : (note.ParentId == "default" ? "." : $"list?id=" + note.ParentId);
                page.Navigation.Add(note.IsFolder ? new Button("Back", parentLink, "right") : new ButtonJS("Back", $"Back('{parentLink}')", "right", id: "back"));
                page.Navigation.Add(note.ParentId == null ? new Button("Search", "search", "right") : new Button("More", $"more?id={id}", "right"));
                if (id != "default")
                {
                    page.Sidebar.Add(new ButtonElement(null, "Go up a level", parentLink));
                    string parentFilePath = $"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{note.ParentId}.txt";
                    var siblings = File.ReadAllLines(parentFilePath).Select(x => new KeyValuePair<string, NoteItem>(x, notes.Notes[x])).OrderByDescending(x => x.Value.IsFolder).ThenBy(x => x.Value.Name);
                    foreach (var sibling in siblings)
                        if (sibling.Key == id)
                            page.Sidebar.Add(new ContainerElement(null, sibling.Value.Name, "green"));
                        else page.Sidebar.Add(new ButtonElement(null, sibling.Value.Name, $"{(sibling.Value.IsFolder ? "list" : "edit")}?id={sibling.Key}"));
                }
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("list.js"));
                e.Add(new LargeContainerElement(note.Name));
                e.Add(new ContainerElement("New", new TextBox("Enter a name...", null, "name", onEnter: "Create(false)", autofocus: true))
                { Buttons = [
                    new ButtonJS("Note", "Create(false)", "green"),
                    new ButtonJS("Folder", "Create(true)", "green")
                ]});
                page.AddError();
                var items = File.ReadAllLines(filePath).Select(x => new KeyValuePair<string, NoteItem>(x, notes.Notes[x])).OrderByDescending(x => x.Value.IsFolder).ThenBy(x => x.Value.Name);
                foreach (var item in items)
                    e.Add(new ButtonElement(item.Value.Name, item.Value.IsFolder ? null : $"Note - {item.Value.Changed.Date.ToLongDateString()}", $"{(item.Value.IsFolder ? "list" : "edit")}?id=" + item.Key));
                if (!items.Any())
                    e.Add(new ContainerElement("No items!", "", "red"));
            } break;

            case "/list/create":
            { POST(req, out var notes);
                if (!(req.Query.TryGetValue("id", out string? id) && req.Query.TryGetValue("folder", out bool folder) && req.Query.TryGetValue("name", out string? name)))
                    throw new BadRequestSignal();
                if (!notes.Notes.TryGetValue(id, out var note))
                    throw new NotFoundSignal();
                if (!note.IsFolder)
                    throw new BadRequestSignal();
                if (File.ReadAllLines($"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{id}.txt").Any(siblingId => notes.Notes.TryGetValue(siblingId, out var sibling) && sibling.Name == name))
                    throw new HttpStatusSignal(302);
                var n = new NoteItem(name, id, folder);
                string nId;
                do nId = Parsers.RandomString(7);
                    while (notes.Notes.ContainsKey(nId));
                notes.Lock();
                notes.Notes[nId] = n;
                File.AppendAllLines($"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{id}.txt", [ nId ]);
                if (folder)
                    File.WriteAllLines($"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{nId}.txt", []);
                else File.WriteAllText($"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{nId}.txt", "");
                notes.UnlockSave();
                await req.Write($"id={nId}");
            } break;

            case "/changed-event":
            { GET(req, out var notes);
                if (!req.Query.TryGetValue("id", out string? id))
                    throw new BadRequestSignal();
                if (!notes.Notes.ContainsKey(id))
                    throw new NotFoundSignal();
                lock(ChangeListeners)
                    if (ChangeListeners.TryGetValue(id, out var set))
                        set.Add(req);
                    else ChangeListeners[id] = [req];
                req.KeepEventAliveCancelled += RemoveChangeListener;
                await req.KeepEventAlive();
            } break;




            // 404
            default:
                req.CreatePage("Error");
                req.Status = 404;
                break;
        }
    }
}
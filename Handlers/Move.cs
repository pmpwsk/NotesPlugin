using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    private Task HandleMove(Request req)
    {
        switch (req.Path)
        {
            case "/move":
            { CreatePage(req, "Notes", out var page, out var e, out var notes);
                if (!(req.Query.TryGetValue("id", out var id) && id != "default" && req.Query.TryGetValue("to", out var to)))
                    throw new BadRequestSignal();
                if (!(notes.Notes.TryGetValue(id, out var note) && notes.Notes.TryGetValue(to, out var target)))
                    throw new NotFoundSignal();
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("move.js"));
                page.Title = "Move " + note.Name;
                if (target.ParentId != null)
                    page.Navigation.Add(new Button("Back", $"move?id={id}&to={target.ParentId}", "right"));
                if (to != "default")
                {
                    page.Sidebar.Add(new ButtonElement(null, "Go up a level", $"move?id={id}&to={target.ParentId}"));
                    string parentFilePath = $"../Notes/{req.UserTable.Name}/{req.User.Id}/{target.ParentId}.txt";
                    var siblings = File.ReadAllLines(parentFilePath).Select(x => new KeyValuePair<string, NoteItem>(x, notes.Notes[x])).Where(x => x.Value.IsFolder).OrderBy(x => x.Value.Name);
                    foreach (var sibling in siblings)
                        if (sibling.Key == to)
                            page.Sidebar.Add(new ContainerElement(null, sibling.Value.Name, "green"));
                        else page.Sidebar.Add(new ButtonElement(null, sibling.Value.Name, $"move?id={id}&to={sibling.Key}"));
                }
                e.Add(new LargeContainerElement(target.Name, $"You are moving: {note.Name}") { Button = new Button("Cancel", $"more?id={id}", "red")});
                if (note.ParentId != to)
                    e.Add(new ButtonElementJS("Move here", null, "Move()", "green"));
                page.AddError();
                var items = File.ReadAllLines($"../Notes/{req.UserTable.Name}/{req.User.Id}/{to}.txt").Select(x => new KeyValuePair<string, NoteItem>(x, notes.Notes[x])).Where(x => x.Value.IsFolder).OrderBy(x => x.Value.Name);
                foreach (var item in items)
                    e.Add(new ButtonElement(item.Value.Name, null, $"move?id={id}&to={item.Key}"));
                if (!items.Any())
                    e.Add(new ContainerElement("No items!", "", "red"));
            } break;

            case "/move/do":
            { POST(req, out var notes);
                if (!(req.Query.TryGetValue("id", out var id) && id != "default" && req.Query.TryGetValue("to", out var to)))
                    throw new BadRequestSignal();
                if (!(notes.Notes.TryGetValue(id, out var note) && notes.Notes.TryGetValue(to, out var target)))
                    throw new NotFoundSignal();
                if (note.ParentId == to)
                    break;
                if (File.ReadAllLines($"../Notes/{req.UserTable.Name}/{req.User.Id}/{to}.txt").Any(siblingId => notes.Notes.TryGetValue(siblingId, out var sibling) && sibling.Name == note.Name))
                    throw new HttpStatusSignal(302);
                notes.Lock();
                string file = $"../Notes/{req.UserTable.Name}/{req.User.Id}/{note.ParentId}.txt";
                File.WriteAllLines(file, File.ReadAllLines(file).Where(x => x != id));
                file = $"../Notes/{req.UserTable.Name}/{req.User.Id}/{to}.txt";
                File.WriteAllLines(file, [.. File.ReadAllLines(file), id]);
                note.ParentId = to;
                notes.UnlockSave();
            } break;




            // 404
            default:
                req.CreatePage("Error");
                req.Status = 404;
                break;
        }

        return Task.CompletedTask;
    }
}
using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    private async Task HandleEdit(Request req)
    {
        switch (req.Path)
        {
            case "/edit":
            { CreatePage(req, "Notes", out var page, out var e, out var notes);
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                if (!notes.Notes.TryGetValue(id, out NoteItem? note))
                    throw new NotFoundSignal();
                if (note.IsFolder)
                    throw new BadRequestSignal();
                string filePath = $"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{id}.txt";
                page.Title = note.ParentId == null ? "Notes" : $"{note.Name} - Notes";
                string parentLink = note.ParentId == null ? "/" : (note.ParentId == "default" ? "." : $"list?id=" + note.ParentId);
                page.Navigation.Add(note.IsFolder ? new Button("Back", parentLink, "right") : new ButtonJS("Back", $"Back('{parentLink}')", "right", id: "back"));
                page.Navigation.Add(new Button("More", $"more{(id == "default" ? "" : $"?id={id}")}", "right"));
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
                page.Scripts.Add(new Script("edit.js"));
                page.Styles.Add(new CustomStyle(
                    "div.editor { display: flex; flex-flow: column; }",
                    "div.editor textarea { flex: 1 1 auto; }",
                    "div.editor h1, div.editor h2, div.editor div.buttons { flex: 0 1 auto; }"
                ));
                page.AddError();
                page.HideFooter = true;
                e.Add(new LargeContainerElementIsoTop(note.Name, new TextArea("Loading...", null, "text", null, onInput: "TextChanged(); Resize();"), classes: "editor", id: "editor")
                {
                    Button = new ButtonJS("Saved!", $"Save()", null, id: "save")
                });
            } break;

            case "/edit/load":
            { GET(req, out var notes);
                if (!req.Query.TryGetValue("id", out string? id))
                    throw new BadRequestSignal();
                if (!notes.Notes.TryGetValue(id, out var note))
                    throw new NotFoundSignal();
                if (note.IsFolder)
                    throw new BadRequestSignal();
                var content = File.ReadAllText($"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{id}.txt");
                if (content == "")
                    req.Status = 201;
                else await req.Write(content);
            } break;

            case "/edit/save":
            { POST(req, out var notes);
                if (req.IsForm || !req.Query.TryGetValue("id", out string? id))
                    throw new BadRequestSignal();
                if (!notes.Notes.TryGetValue(id, out var note))
                    throw new NotFoundSignal();
                if (note.IsFolder)
                    throw new BadRequestSignal();
                notes.Lock();
                File.WriteAllText($"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{id}.txt", await req.GetBodyText());
                note.Changed = DateTime.UtcNow;
                notes.UnlockSave();
            } break;




            // 404
            default:
                req.CreatePage("Error");
                req.Status = 404;
                break;
        }
    }
}
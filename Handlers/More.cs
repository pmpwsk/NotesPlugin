using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    private async Task HandleMore(Request req)
    {
        switch (req.Path)
        {
            case "/more":
            { CreatePage(req, "Notes", out var page, out var e, out var notes);
                if (!(req.Query.TryGetValue("id", out var id) && id != "default"))
                    throw new BadRequestSignal();
                if (!notes.Notes.TryGetValue(id, out NoteItem? note))
                    throw new NotFoundSignal();
                page.Title = note.Name;
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("more.js"));
                string parentLink = note.ParentId == null ? "/" : (note.ParentId == "default" ? "." : $"list?id=" + note.ParentId);
                page.Navigation.Add(new Button("Back", parentLink, "right"));
                page.Navigation.Add(new Button("Less", $"{(note.IsFolder ? "list" : "edit")}?id={id}", "right"));
                e.Add(new HeadingElement(note.Name));
                page.AddError();
                e.Add(new ContainerElement("Rename:", new TextBox("Enter a new name...", note.Name, "rename", onEnter: "Rename()")) { Button = new ButtonJS("Save", "Rename()", "green") });
                e.Add(new ButtonElement("Move", null, $"move?id={id}&to={note.ParentId}"));
                e.Add(new ButtonElementJS("Delete", null, "Delete()", "red", id: "delete"));
            } break;

            case "/more/delete":
            { POST(req, out var notes);
                if (!req.Query.TryGetValue("id", out string? id))
                    throw new BadRequestSignal();
                if (!notes.Notes.TryGetValue(id, out var note))
                    throw new NotFoundSignal();
                if (note.ParentId == null)
                    throw new BadRequestSignal();
                notes.Lock();
                notes.Delete(req.User.Id, req.UserTable.Name, id, true);
                notes.UnlockSave();
                if (note.ParentId != "default")
                    await req.Write($"list?id={note.ParentId}");
                else await req.Write(".");
                await NotifyChangeListeners(note.ParentId);
            } break;

            case "/more/rename":
            { POST(req, out var notes);
                if (!(req.Query.TryGetValue("id", out string? id) && req.Query.TryGetValue("name", out string? name)))
                    throw new BadRequestSignal();
                if (!notes.Notes.TryGetValue(id, out var note))
                    throw new NotFoundSignal();
                if (note.ParentId == null)
                    throw new BadRequestSignal();
                if (note.Name != name)
                {
                    if (File.ReadAllLines($"../NotesPlugin.Profiles/{req.UserTable.Name}/{req.User.Id}/{note.ParentId}.txt").Any(siblingId => notes.Notes.TryGetValue(siblingId, out var sibling) && sibling.Name == name))
                        throw new HttpStatusSignal(302);
                    notes.Lock();
                    note.Name = name;
                    notes.UnlockSave();
                }
                await req.Write(note.IsFolder ? "list" : "edit");
                await NotifyChangeListeners(note.ParentId);
            } break;




            // 404
            default:
                req.CreatePage("Error");
                req.Status = 404;
                break;
        }
    }
}
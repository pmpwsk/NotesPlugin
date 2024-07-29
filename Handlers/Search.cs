using uwap.Database;
using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    private Task HandleSearch(Request req)
    {
        switch (req.Path)
        {
            case "/search":
            { CreatePage(req, "Search for notes", out var page, out var e, out var notes);
                page.Navigation.Add(new Button("Back", ".", "right"));
                page.Scripts.Add(new Script("search.js"));
                string? query = req.Query.TryGet("q");
                e.Add(new LargeContainerElement("Notes", new TextBox("Search notes...", query, "search", onEnter: "Search()", autofocus: true))
                {
                    Button = new ButtonJS("Search", "Search()", "green")
                });
                if (query != null)
                {
                    Search<KeyValuePair<string, NoteItem>> search = new(notes.Notes.Where(x => x.Key != "default"), query);
                    search.Find(x => x.Value.Name);
                    search.Find(x => x.Value.Changed.ToLongDateString());
                    var results = search.Sort(x => !x.Value.IsFolder, x => x.Value.Name).ToList();
                    foreach (var item in results)
                        e.Add(new ButtonElement(item.Value.Name, item.Value.IsFolder ? null : $"Note - {item.Value.Changed.Date.ToLongDateString()}", $"{(item.Value.IsFolder ? "list" : "edit")}?id={item.Key}"));
                    if (results.Count == 0)
                        e.Add(new ContainerElement("No items!", "", "red"));
                }
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
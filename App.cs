﻿using uwap.Database;
using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    public override Task Handle(AppRequest req, string path, string pathPrefix)
    {
        Presets.CreatePage(req, "Notes", out Page page, out List<IPageElement> e);
        Presets.Navigation(req, page);
        if (!req.LoggedIn)
        {
            e.Add(new HeadingElement("Not logged in!", "You need to be logged in to use this application.", "red"));
            return Task.CompletedTask;
        }

        var notes = GetOrCreate(req.User.Id, req.UserTable.Name);

        string pluginHome = pathPrefix == "" ? "/" : pathPrefix;

        page.Favicon = pathPrefix + "/icon.ico";
        page.Head.Add($"<link rel=\"manifest\" href=\"{pathPrefix}/manifest.json\" />");
        switch (path)
        {
            case "":
                {
                    string id = req.Query.TryGet("id") ?? "default";
                    if (!notes.Notes.TryGetValue(id, out NoteItem? note))
                    {
                        req.Status = 404;
                        break;
                    }
                    string filePath = $"../Notes/{req.UserTable.Name}/{req.User.Id}/{id}.txt";

                    page.Scripts.Add(new Script(pathPrefix + "/notes.js"));
                    page.Title = note.ParentId == null ? "Notes" : $"{note.Name} - Notes";

                    string parentLink;
                    if (note.ParentId == null)
                        parentLink = "/";
                    else if (note.ParentId == "default")
                        parentLink = pluginHome;
                    else parentLink = $"{pluginHome}?id=" + note.ParentId;
                    string idQuery = id == "default" ? "" : $"?id={id}";
                    IButton backButton = note.IsFolder ? new Button("Back", parentLink, "right") : new ButtonJS("Back", $"Back('{parentLink}')", "right", id: "back");
                    IButton homeButton = page.Navigation.Count != 0 ? page.Navigation.First() : new Button(req.Domain, "/");
                    page.Navigation =
                    [
                        homeButton, backButton, new Button("More", $"{pathPrefix}/more" + idQuery, "right"),
                        note.ParentId != null ? new ButtonJS("Delete", "Delete()", "right", id: "delete") : new Button("Search", $"{pathPrefix}/search", "right"),
                        new Button("Notes", pluginHome),
                    ];

                    //sidebar (shared)
                    if (id != "default")
                    {
                        page.Sidebar.Add(new ButtonElementJS(null, "Go up a level", $"Navigate('{parentLink}')"));
                        string parentFilePath = $"../Notes/{req.UserTable.Name}/{req.User.Id}/{note.ParentId}.txt";
                        var siblings = File.ReadAllLines(parentFilePath).Select(x => new KeyValuePair<string, NoteItem>(x, notes.Notes[x])).OrderByDescending(x => x.Value.IsFolder).ThenBy(x => x.Value.Name);
                        foreach (var sibling in siblings)
                            if (sibling.Key == id)
                                page.Sidebar.Add(new ContainerElement(null, sibling.Value.Name, "green"));
                            else page.Sidebar.Add(new ButtonElementJS(null, sibling.Value.Name, $"Navigate('{pluginHome}?id={sibling.Key}')"));
                    }

                    if (note.IsFolder) //folder
                    {
                        e.Add(new LargeContainerElement(note.Name, new TextBox("New note...", null, "name", onEnter: "CreateNote()")));
                        page.AddError();

                        var items = File.ReadAllLines(filePath).Select(x => new KeyValuePair<string, NoteItem>(x, notes.Notes[x])).OrderByDescending(x => x.Value.IsFolder).ThenBy(x => x.Value.Name);
                        foreach (var item in items)
                            e.Add(new ButtonElement(item.Value.Name, item.Value.IsFolder ? null : $"Note - {item.Value.Changed.Date.ToLongDateString()}", $"{pluginHome}?id=" + item.Key));
                        if (!items.Any())
                            e.Add(new ContainerElement("No items!", "", "red"));
                    }
                    else //note
                    {
                        page.Scripts.Add(new Script(pathPrefix + "/note.js"));
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
                    }
                }
                break;
            case "/more":
                {
                    string id = req.Query.TryGet("id") ?? "default";
                    if (!notes.Notes.TryGetValue(id, out NoteItem? note))
                    {
                        req.Status = 404;
                        break;
                    }
                    string filePath = $"../Notes/{req.UserTable.Name}/{req.User.Id}/{id}.txt";

                    page.Title = note.Name;
                    page.Scripts.Add(new Script(pathPrefix + "/more.js"));

                    string parentLink;
                    if (note.ParentId == null)
                        parentLink = "/";
                    else if (note.ParentId == "default")
                        parentLink = pluginHome;
                    else parentLink = $"{pluginHome}?id=" + note.ParentId;
                    string lessLink = pluginHome + (id == "default" ? "" : $"?id={id}");
                    IButton homeButton = page.Navigation.Count != 0 ? page.Navigation.First() : new Button(req.Domain, "/");
                    page.Navigation = [homeButton, new Button("Back", parentLink, "right"), new Button("Less", lessLink, "right"), new Button("Notes", pluginHome)];

                    e.Add(new HeadingElement(note.Name));
                    page.AddError();
                    if (note.IsFolder)
                        e.Add(new ContainerElement("Create new...", new TextBox("Enter a name...", null, "name", onEnter: "CreateFolder()"))
                        { Buttons = [new ButtonJS("Folder", "CreateFolder()", "green"), new ButtonJS("Note", "CreateNote()", "green")] });
                    if (note.ParentId != null)
                    {
                        e.Add(new ContainerElement("Rename:", new TextBox("Enter a new name...", note.Name, "rename", onEnter: "Rename()")) { Button = new ButtonJS("Save", "Rename()", "green") });
                        e.Add(new ButtonElement("Move", null, $"{pathPrefix}/move?id={id}&to={note.ParentId}"));
                        e.Add(new ButtonElementJS("Delete", null, "Delete()", "red", id: "delete"));
                    }
                }
                break;
            case "/move":
                {
                    if (req.Query.TryGetValue("id", out var id) && id != "default" && req.Query.TryGetValue("to", out var to))
                    {
                        if (notes.Notes.TryGetValue(id, out var note) && notes.Notes.TryGetValue(to, out var target))
                        {
                            page.Scripts.Add(new Script($"{pathPrefix}/move.js"));

                            page.Title = "Move " + note.Name;
                            
                            IButton homeButton = page.Navigation.Count != 0 ? page.Navigation.First() : new Button(req.Domain, "/");
                            page.Navigation = [homeButton, ..target.ParentId == null ? (IEnumerable<IButton>)[] : [new Button("Back", $"{pathPrefix}/move?id={id}&to={target.ParentId}", "right")], new Button("Notes", pluginHome)];
                            
                            if (to != "default")
                            {
                                page.Sidebar.Add(new ButtonElement(null, "Go up a level", $"{pathPrefix}/move?id={id}&to={target.ParentId}"));
                                string parentFilePath = $"../Notes/{req.UserTable.Name}/{req.User.Id}/{target.ParentId}.txt";
                                var siblings = File.ReadAllLines(parentFilePath).Select(x => new KeyValuePair<string, NoteItem>(x, notes.Notes[x])).Where(x => x.Value.IsFolder).OrderBy(x => x.Value.Name);
                                foreach (var sibling in siblings)
                                    if (sibling.Key == to)
                                        page.Sidebar.Add(new ContainerElement(null, sibling.Value.Name, "green"));
                                    else page.Sidebar.Add(new ButtonElement(null, sibling.Value.Name, $"{pathPrefix}/move?id={id}&to={sibling.Key}"));
                            }
                            
                            e.Add(new LargeContainerElement(target.Name, $"You are moving: {note.Name}") { Button = new Button("Cancel", $"{pathPrefix}/more?id={id}", "red")});
                            if (note.ParentId != to)
                                e.Add(new ButtonElementJS("Move here", null, "Move()", "green"));
                            page.AddError();

                            var items = File.ReadAllLines($"../Notes/{req.UserTable.Name}/{req.User.Id}/{to}.txt").Select(x => new KeyValuePair<string, NoteItem>(x, notes.Notes[x])).Where(x => x.Value.IsFolder).OrderBy(x => x.Value.Name);
                            foreach (var item in items)
                                e.Add(new ButtonElement(item.Value.Name, null, $"{pathPrefix}/move?id={id}&to={item.Key}"));
                            if (!items.Any())
                                e.Add(new ContainerElement("No items!", "", "red"));
                        }
                        else req.Status = 404;
                    }
                    else req.Status = 400;
                }
                break;
            case "/search":
                {
                    page.Title = "Search for notes";
                    IButton homeButton = page.Navigation.Count != 0 ? page.Navigation.First() : new Button(req.Domain, "/");
                    page.Navigation = [homeButton, new Button("Back", pluginHome, "right"), new Button("Notes", pluginHome)];
                    string? query = req.Query.TryGet("q");
                    page.Scripts.Add(new Script(pathPrefix + "/search.js"));
                    e.Add(new LargeContainerElement("Notes", new TextBox("Search notes...", query, "search", onEnter: "Search()", autofocus: true))
                    {
                        Button = new ButtonJS("Search", "Search()", "green")
                    });
                    if (query != null)
                    {
                        Search<KeyValuePair<string, NoteItem>> search = new(notes.Notes.Where(x => !x.Value.IsFolder), query);
                        search.Find(x => x.Value.Name);
                        search.Find(x => x.Value.Changed.ToLongDateString());
                        var results = search.Sort(x => x.Value.Name);
                        foreach (var item in results)
                            e.Add(new ButtonElement(item.Value.Name, item.Value.IsFolder ? null : $"Note - {item.Value.Changed.Date.ToLongDateString()}", $"{pluginHome}?id=" + item.Key));
                        if (!results.Any())
                            e.Add(new ContainerElement("No items!", "", "red"));
                    }
                }
                break;
            default:
                req.Status = 404;
                break;
        }

        return Task.CompletedTask;
    }
}

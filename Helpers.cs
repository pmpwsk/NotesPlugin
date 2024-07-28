using System.Web;
using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    private void CreatePage(Request req, string title, out Page page, out List<IPageElement> e, out NoteGroup notes)
    {
        req.ForceGET();
        req.CreatePage(title, out page, out e);
        req.ForceLogin();
        page.Head.Add($"<link rel=\"manifest\" href=\"{req.PluginPathPrefix}/manifest.json\" />");
        page.Favicon = $"{req.PluginPathPrefix}/icon.ico";
        page.Navigation =
        [
            page.Navigation.Count != 0 ? page.Navigation.First() : new Button(req.Domain, "/"),
            new Button("Notes", $"{req.PluginPathPrefix}/")
        ];
        notes = GetOrCreate(req.User.Id, req.UserTable.Name);
    }

    private void POST(Request req, out NoteGroup notes)
    {
        req.ForcePOST();
        req.ForceLogin(false);
        notes = GetOrCreate(req.User.Id, req.UserTable.Name);
    }

    private void GET(Request req, out NoteGroup notes)
    {
        req.ForceGET();
        req.ForceLogin(false);
        notes = GetOrCreate(req.User.Id, req.UserTable.Name);
    }
}
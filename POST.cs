namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    public override async Task Handle(PostRequest req, string path, string pathPrefix)
    {
        if (req.User == null || (!req.LoggedIn))
        {
            req.Status = 403;
            return;
        }

        var notes = GetOrCreate(req.User.Id);

        string pluginHome = pathPrefix == "" ? "/" : pathPrefix;

        switch (path)
        {
            default:
                req.Status = 404;
                break;
        }
    }
}

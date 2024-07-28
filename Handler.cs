namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
    public override async Task Handle(Request req)
    {
        switch (Parsers.GetFirstSegment(req.Path, out _))
        {
            // EDIT A NOTE
            case "edit":
                await HandleEdit(req);
                break;

            // MORE OPTIONS
            case "more":
                await HandleMore(req);
                break;

            // MOVE A NODE
            case "move":
                await HandleMove(req);
                break;

            // SEARCH FOR NODES
            case "search":
                await HandleSearch(req);
                break;

            // LIST / 404
            default:
                await HandleOther(req);
                break;
        }
    }
}
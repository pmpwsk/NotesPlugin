namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin
{
    public override Task Work()
    {
        foreach (var group in Table)
        {
            group.Value.Lock();
            bool dirty = false;
            foreach (var item in group.Value.Notes)
            {
                if (item.Value.ParentId != null && !group.Value.Notes.ContainsKey(item.Value.ParentId))
                {
                    dirty = true;
                    group.Value.Delete(group.Key, item.Key, false);
                }
            }
            if (dirty) group.Value.UnlockSave();
            else group.Value.UnlockIgnore();
        }
        return Task.CompletedTask;
    }
}

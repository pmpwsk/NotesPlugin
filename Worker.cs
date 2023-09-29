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
                    string[] key = group.Key.Split('_');
                    group.Value.Delete(key[1], key[0], item.Key, false);
                }
            }
            if (dirty) group.Value.UnlockSave();
            else group.Value.UnlockIgnore();
        }
        return Task.CompletedTask;
    }
}

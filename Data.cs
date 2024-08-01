using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin
{
    private readonly NoteGroupTable Table = NoteGroupTable.Import("NotesPlugin.Profiles");

    private readonly Dictionary<string, HashSet<Request>> ChangeListeners = [];

    private NoteGroup GetOrCreate(string userId, string userTable)
    {
        if (Table.TryGetValue(userTable + "_" + userId, out var notes))
            return notes;
        else
        {
            notes = new NoteGroup();
            var note = new NoteItem("Notes", null, true);
            notes.Notes["default"] = note;
            Directory.CreateDirectory($"../NotesPlugin.Profiles/{userTable}/{userId}");
            File.WriteAllLines($"../NotesPlugin.Profiles/{userTable}/{userId}/default.txt", []);
            Table[userTable + "_" + userId] = notes;
            return notes;
        }
    }

    private Task RemoveChangeListener(Request req)
    {
        lock(ChangeListeners)
            if (req.Query.TryGetValue("id", out var id) && ChangeListeners.TryGetValue(id, out var set) && set.Remove(req) && set.Count == 0)
                ChangeListeners.Remove(id);
        return Task.CompletedTask;
    }

    private class NoteGroupTable : Table<NoteGroup>
    {
        private NoteGroupTable(string name) : base(name) { }

        protected static new NoteGroupTable Create(string name)
        {
            if (!name.All(Tables.KeyChars.Contains))
                throw new Exception($"This name contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
            if (Directory.Exists("../Database/" + name))
                throw new Exception("A table with this name already exists, try importing it instead.");

            Directory.CreateDirectory("../Database/" + name);
            NoteGroupTable table = new(name);
            Tables.Dictionary[name] = table;
            return table;
        }

        public static new NoteGroupTable Import(string name, bool skipBroken = false)
        {
            if (Tables.Dictionary.TryGetValue(name, out ITable? table))
                return (NoteGroupTable)table;
            if (!name.All(Tables.KeyChars.Contains))
                throw new Exception($"This name contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
            if (!Directory.Exists("../Database/" + name))
                return Create(name);

            if (Directory.Exists("../Database/Buffer/" + name) && Directory.GetFiles("../Database/Buffer/" + name, "*.json", SearchOption.AllDirectories).Length > 0)
                Console.WriteLine($"The database buffer of table '{name}' contains an entry because a database operation was interrupted. Please manually merge the files and delete the file from the buffer.");

            NoteGroupTable result = new(name);
            result.Reload(skipBroken);
            Tables.Dictionary[name] = result;
            return result;
        }

        protected override IEnumerable<string> EnumerateDirectoriesToClear()
        {
            yield return "../NotesPlugin.Profiles";
        }

        protected override IEnumerable<string> EnumerateOtherDirectories(TableEntry<NoteGroup> entry)
        {
            yield return $"../NotesPlugin.Profiles/{entry.Key.Replace('_', '/')}";
        }
    }

    [DataContract]
    public class NoteGroup : ITableValue
    {
        [DataMember] public Dictionary<string, NoteItem> Notes = [];

        public void Delete(string userId, string userTable, string noteId, bool deleteFromParent)
        {
            var note = Notes[noteId];
            if (note.ParentId == null)
            {
                Console.WriteLine("Attempted to delete the default notes folder!");
                return;
            }
            if (note.IsFolder)
                foreach (var child in File.ReadAllLines($"../NotesPlugin.Profiles/{userTable}/{userId}/{noteId}.txt"))
                    Delete(userId, userTable, child, false);
            Notes.Remove(noteId);
            if (deleteFromParent)
                File.WriteAllLines($"../NotesPlugin.Profiles/{userTable}/{userId}/{note.ParentId}.txt", File.ReadAllLines($"../NotesPlugin.Profiles/{userTable}/{userId}/{note.ParentId}.txt").Where(x => x != noteId));
            File.Delete($"../NotesPlugin.Profiles/{userTable}/{userId}/{noteId}.txt");
        }
    }

    [DataContract]
    public class NoteItem(string name, string? parent, bool isFolder)
    {
        [DataMember] public string Name = name;
        [DataMember] public string? ParentId = parent;
        [DataMember] public bool IsFolder = isFolder;
        [DataMember] public DateTime Changed = DateTime.UtcNow;
    }
}

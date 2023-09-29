using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin
{
    public readonly static Table<NoteGroup> Table = Table<NoteGroup>.Import("Notes");

    private static NoteGroup GetOrCreate(string userId, string userTable)
    {
        if (Table.TryGetValue(userId, out var notes))
            return notes;
        else
        {
            notes = new NoteGroup();
            var note = new NoteItem("Notes", null, true);
            notes.Notes["default"] = note;
            Directory.CreateDirectory($"../Notes/{userTable}");
            File.WriteAllLines($"../Notes/{userTable}/{userId}-default.txt", Array.Empty<string>());
            Table[userId] = notes;
            return notes;
        }
    }

    [DataContract]
    public class NoteGroup : ITableValue
    {
        [DataMember] public Dictionary<string, NoteItem> Notes = new();

        public void Delete(string userId, string userTable, string noteId, bool deleteFromParent)
        {
            var note = Notes[noteId];
            if (note.ParentId == null)
            {
                Console.WriteLine("Attempted to delete the default notes folder!");
                return;
            }
            if (note.IsFolder)
            {
                foreach (var child in File.ReadAllLines($"../Notes/{userTable}/{userId}-{noteId}.txt"))
                {
                    Delete(userId, userTable, child, false);
                }
            }
            Notes.Remove(noteId);
            if (deleteFromParent) File.WriteAllLines($"../Notes/{userTable}/{userId}-{note.ParentId}.txt", File.ReadAllLines($"../Notes/{userTable}/{userId}-{note.ParentId}.txt").Where(x => x != noteId));
            File.Delete($"../Notes/{userTable}/{userId}-{noteId}.txt");
        }
    }

    [DataContract]
    public class NoteItem
    {
        [DataMember] public string Name;
        [DataMember] public string? ParentId;
        [DataMember] public bool IsFolder;
        [DataMember] public DateTime Changed;

        public NoteItem(string name, string? parent, bool isFolder)
        {
            Name = name;
            ParentId = parent;
            IsFolder = isFolder;
            Changed = DateTime.UtcNow;
        }
    }
}

using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin
{
    public readonly Table<NoteGroup> Table = Table<NoteGroup>.Import("Notes");

    private NoteGroup GetOrCreate(string userId, string userTable)
    {
        if (Table.TryGetValue(userTable + "_" + userId, out var notes))
            return notes;
        else
        {
            notes = new NoteGroup();
            var note = new NoteItem("Notes", null, true);
            notes.Notes["default"] = note;
            Directory.CreateDirectory($"../Notes/{userTable}");
            File.WriteAllLines($"../Notes/{userTable}/{userId}/default.txt", []);
            Table[userTable + "_" + userId] = notes;
            return notes;
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
            {
                foreach (var child in File.ReadAllLines($"../Notes/{userTable}/{userId}/{noteId}.txt"))
                {
                    Delete(userId, userTable, child, false);
                }
            }
            Notes.Remove(noteId);
            if (deleteFromParent) File.WriteAllLines($"../Notes/{userTable}/{userId}/{note.ParentId}.txt", File.ReadAllLines($"../Notes/{userTable}/{userId}/{note.ParentId}.txt").Where(x => x != noteId));
            File.Delete($"../Notes/{userTable}/{userId}/{noteId}.txt");
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

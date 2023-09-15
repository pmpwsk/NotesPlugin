# NotesPlugin
Plugin for [WebFramework](https://github.com/pmpwsk/WebFramework) that adds a simple notes app for users.

There is also a manifest so most browsers should offer installing this plugin as a progressive web app (PWA).

Website: https://uwap.org/projects/notes-plugin

Changelog: https://uwap.org/changes/notes-plugin

## Main features
- Creating, editing, saving/discarding and renaming notes
- Searching for notes by name or day
- Creating folders for notes with unlimited depth

## Installation
You can add this plugin to your WF project by installing the NuGet package: [uwap.NotesPlugin](https://www.nuget.org/packages/uwap.NotesPlugin/)

You can also download the source code and reference it in your project file.

Once installed, add the following things to your program start code:
- Add <code>using uwap.WebFramework.Plugins;</code> to the top, otherwise you have to prepend it to <code>NotesPlugin</code>
- Create a new object of the plugin: <code>NotesPlugin notesPlugin = new();</code>
- Map the plugin to a path of your choosing (like any/notes): <code>PluginManager.Map("any/notes", notesPlugin);</code>

You can do all that with a single line of code before starting the WF server:<br/><code>PluginManager.Map("any/notes", new uwap.WebFramework.Plugins.NotesPlugin());</code>

## Plans for the future
- Moving notes
- Sharing notes publically or privately
- Allowing other users to edit certain notes

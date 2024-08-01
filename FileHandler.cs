namespace uwap.WebFramework.Plugins;

public partial class NotesPlugin : Plugin
{
	public override byte[]? GetFile(string relPath, string pathPrefix, string domain)
		=> relPath switch
		{
			"/edit.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File0"),
			"/icon.ico" => (byte[]?)PluginFiles_ResourceManager.GetObject("File1"),
			"/icon.png" => (byte[]?)PluginFiles_ResourceManager.GetObject("File2"),
			"/icon.svg" => (byte[]?)PluginFiles_ResourceManager.GetObject("File3"),
			"/list.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File4"),
			"/manifest.json" => System.Text.Encoding.UTF8.GetBytes($"{{\r\n  \"name\": \"Notes ({Parsers.DomainMain(domain)})\",\r\n  \"short_name\": \"Notes\",\r\n  \"start_url\": \"{pathPrefix}/\",\r\n  \"display\": \"minimal-ui\",\r\n  \"background_color\": \"#000000\",\r\n  \"theme_color\": \"#202024\",\r\n  \"orientation\": \"portrait-primary\",\r\n  \"icons\": [\r\n    {{\r\n      \"src\": \"{pathPrefix}/icon.svg\",\r\n      \"type\": \"image/svg+xml\",\r\n      \"sizes\": \"any\"\r\n    }},\r\n    {{\r\n      \"src\": \"{pathPrefix}/icon.png\",\r\n      \"type\": \"image/png\",\r\n      \"sizes\": \"512x512\"\r\n    }},\r\n    {{\r\n      \"src\": \"{pathPrefix}/icon.ico\",\r\n      \"type\": \"image/x-icon\",\r\n      \"sizes\": \"16x16 24x24 32x32 48x48 64x64 72x72 96x96 128x128 256x256\"\r\n    }}\r\n  ],\r\n  \"launch_handler\": {{\r\n    \"client_mode\": \"navigate-new\"\r\n  }},\r\n  \"related_applications\": [\r\n    {{\r\n      \"platform\": \"webapp\",\r\n      \"url\": \"{pathPrefix}/manifest.json\"\r\n    }}\r\n  ],\r\n  \"offline_enabled\": false,\r\n  \"omnibox\": {{\r\n    \"keyword\": \"notes\"\r\n  }},\r\n  \"version\": \"0.4.4\"\r\n}}\r\n"),
			"/more.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File5"),
			"/move.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File6"),
			"/search.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File7"),
			_ => null
		};
	
	public override string? GetFileVersion(string relPath)
		=> relPath switch
		{
			"/edit.js" => "1722523356044",
			"/icon.ico" => "1694963322000",
			"/icon.png" => "1687982107000",
			"/icon.svg" => "1695860848000",
			"/list.js" => "1722212415161",
			"/manifest.json" => "1722176354326",
			"/more.js" => "1722212421717",
			"/move.js" => "1722212655810",
			"/search.js" => "1722175053932",
			_ => null
		};
	
	private static readonly System.Resources.ResourceManager PluginFiles_ResourceManager = new("NotesPlugin.Properties.PluginFiles", typeof(NotesPlugin).Assembly);
}
let id = GetId();

function GetId() {
    try {
        var query = new URLSearchParams(window.location.search);
        if (query.has("id"))
            return query.get("id");
        else return "default";
    } catch {
        return "null";
    }
}

async function Create(folder) {
    var name = document.getElementById("name");
    if (name.value === "")
        ShowError("Enter a name!");
    else try {
        var response = await fetch(`list/create?id=${id}&name=${encodeURIComponent(name.value)}&folder=${folder}`, {method: "POST"});
        if (response.status === 200) {
            var text = await response.text();
            if (text.startsWith("id="))
                window.location.assign(`${(folder ? "list" : "edit")}?${text}`);
            else ShowError("Connection failed.");
        } else ShowError("Connection failed.");
    } catch {
        ShowError("Connection failed.");
    }
}
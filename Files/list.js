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
    HideError();
    var name = document.getElementById("name");
    if (name.value === "")
        ShowError("Enter a name!");
    else {
        var response = await SendRequest(`list/create?id=${id}&name=${encodeURIComponent(name.value)}&folder=${folder}`, "POST");
        if (typeof response === "string" && response.startsWith("id="))
            window.location.assign(`${(folder ? "list" : "edit")}?${response}`);
        else ShowError("Connection failed.");
    }
}
let id = GetId();
let ignoreChanges = false;
let changedEvent = new EventSource(`changed-event?id=${id}`);
onbeforeunload = (event) => { changedEvent.close(); };
changedEvent.onmessage = async (event) => {
    if (event.data === "changed" && !ignoreChanges)
        window.location.reload();
};

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
        ignoreChanges = true;
        var response = await SendRequest(`list/create?id=${id}&name=${encodeURIComponent(name.value)}&folder=${folder}`, "POST");
        if (typeof response === "string" && response.startsWith("id="))
            window.location.assign(`${(folder ? "list" : "edit")}?${response}`);
        else if (response === 302)
            ShowError("Another note or folder already uses this name!");
        else ShowError("Connection failed.");
        ignoreChanges = false;
    }
}
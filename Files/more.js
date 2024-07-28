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

async function Rename() {
    var rename = document.getElementById("rename");
    if (rename.value === "")
        ShowError("Enter a new name!");
    else try {
        var response = await fetch(`more/rename?id=${id}&name=${encodeURIComponent(rename.value)}`, {method: "POST"});
        if (response.status === 200) {
            var text = await response.text();
            if (text === "list" || text === "edit")
                window.location.assign(`${text}?id=${id}`);
            else ShowError("Connection failed.");
        } else ShowError("Connection failed.");
    } catch {
        ShowError("Connection failed.");
    }
}

async function Delete() {
    var deleteElement = document.getElementById("delete");
    if (deleteElement.firstElementChild.innerText === "Really?")
        try {
            var response = await fetch(`more/delete?id=${id}`, {method: "POST"});
            if (response.status === 200) {
                var text = await response.text();
                if (text === "." || text.startsWith("list?id="))
                    window.location.assign(text);
                else ShowError("Connection failed.");
            } else ShowError("Connection failed.");
        } catch {
            ShowError("Connection failed.");
        }
    else deleteElement.firstElementChild.innerText = "Really?";
}
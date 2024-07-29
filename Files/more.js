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
    HideError();
    var rename = document.getElementById("rename");
    if (rename.value === "")
        ShowError("Enter a new name!");
    else {
        var response = await SendRequest(`more/rename?id=${id}&name=${encodeURIComponent(rename.value)}`, "POST");
        switch (response) {
            case "list":
            case "edit":
                window.location.assign(`${response}?id=${id}`);
                break;
            case 302:
                ShowError("Another note or folder already uses this name!");
                break;
            default:
                ShowError("Connection failed.");
                break;
        }
    }
}

async function Delete() {
    HideError();
    var deleteElement = document.getElementById("delete");
    if (deleteElement.firstElementChild.innerText !== "Really?")
        deleteElement.firstElementChild.innerText = "Really?";
    else {
        var response = await SendRequest(`more/delete?id=${id}`, "POST");
        if (typeof response === "string" && (response === "." || response.startsWith("list?id=")))
            window.location.assign(response);
        else ShowError("Connection failed.");
    }
}
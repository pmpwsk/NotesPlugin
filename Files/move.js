async function Move() {
    HideError();
    var to = GetQuery("to");
    switch (await SendRequest(`move/do?id=${GetQuery("id")}&to=${to}`, "POST", true)) {
        case 200:
            window.location.assign(to === "default" ? "." : `list?id=${to}`);
            break;
        case 302:
            ShowError("This folder contains a note or folder with the same name!");
            break;
        default:
            ShowError("Connection failed.");
            break;
    }
}

function GetQuery(q) {
    try {
        var query = new URLSearchParams(window.location.search);
        if (query.has(q))
            return query.get(q);
        else return "null";
    } catch {
        return "null";
    }
}
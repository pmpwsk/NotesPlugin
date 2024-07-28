async function Move() {
    HideError();
    var to = GetQuery("to");
    if (await SendRequest(`move/do?id=${GetQuery("id")}&to=${to}`, "POST", true) === 200)
        window.location.assign(to === "default" ? "." : `list?id=${to}`);
    else ShowError("Connection failed.");
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
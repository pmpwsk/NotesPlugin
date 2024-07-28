async function Move() {
    try {
        var to = GetQuery("to");
        if ((await fetch(`move/do?id=${GetQuery("id")}&to=${to}`, {method: "POST"})).status === 200) {
            if (to === "default")
                window.location.assign(".");
            else window.location.assign(`list?id=${to}`);
        } else ShowError("Connection failed.");
    } catch {
        ShowError("Connection failed.");
    }
}

function GetQuery(q) {
    try {
        let query = new URLSearchParams(window.location.search);
        if (query.has(q))
            return query.get(q);
        else return "null";
    } catch {
        return "null";
    }
}
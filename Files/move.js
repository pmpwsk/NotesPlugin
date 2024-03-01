async function Move() {
    try {
        var to = GetQuery("to");
        if ((await fetch("/api[PATH_PREFIX]/move?id=" + GetQuery("id") + "&to=" + to)).status === 200) {
            if (to === "default")
                window.location.assign("[PATH_HOME]");
            else window.location.assign("[PATH_HOME]?id=" + to);
            return;
        }
    } catch {
    }
    ShowError("Connection failed.");
}

function GetQuery(q) {
    try {
        let query = new URLSearchParams(window.location.search);
        if (query.has(q)) {
            return query.get(q);
        } else {
            return "null";
        }
    } catch {
        return "null";
    }
}
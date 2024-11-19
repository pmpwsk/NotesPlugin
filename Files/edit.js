let id = GetId();
let changedAlready = false;
let savedValue = null;
let ta = document.getElementById("editor");
let save = document.getElementById("save");
let back = document.getElementById("back");
Load();
document.addEventListener('keydown', e => {
    if (e.ctrlKey && e.key === 's') {
        e.preventDefault();
        Save();
    }
});
window.addEventListener("beforeunload", e => {
    if (save.innerText === "Save" && back.innerText === "Back") {
        var confirmationMessage = "You have unsaved changes!";
        (e || window.event).returnValue = confirmationMessage;
        return confirmationMessage;
    }
});
let changedEvent = new EventSource(`changed-event${window.location.search}`);
onbeforeunload = (event) => { changedEvent.close(); };
changedEvent.onmessage = async (event) => {
    if (event.data === "changed" && save.innerText === "Saved!")
        await Load();
};

async function Load() {
    try {
        var response = await fetch(`edit/load?id=${id}`, {cache:"no-store"});
        switch (response.status) {
            case 200:
            case 201:
                savedValue = response.status === 201 ? "" : await response.text();
                if (save.innerText === "Save") {
                    if (ta.value === savedValue) {
                        save.innerText = "Saved!";
                        save.className = "";
                    }
                } else ta.value = savedValue;
                ta.placeholder = "Enter something...";
                if (ta.value === "")
                    ta.focus();
                break;
            default:
                ta.value = "";
                ta.placeholder = "Error loading this note's content! Try reloading the page.";
                save.innerText = "Error!";
                save.className = "red";
        }
    } catch {
        ta.value = "";
        ta.placeholder = "Error loading this note's content! Try reloading the page.";
        save.innerText = "Error!";
        save.className = "red";
    }
}

function TextChanged() {
    if (changedAlready || ta.value !== savedValue) {
        save.innerText = "Save";
        save.className = "green";
        changedAlready = true;
        savedValue = null;
    }
}

async function Save() {
    back.innerText = "Back";
    save.innerText = "Saving...";
    save.className = "green";
    try {
        if ((await fetch(`edit/save?id=${id}`, { method: "POST", body: ta.value })).status === 200) {
            save.innerText = "Saved!";
            save.className = "";
        } else {
            save.innerText = "Error!";
            save.className = "red";
        }
    } catch {
        save.innerText = "Error!";
        save.className = "red";
    }
}

async function Back(parentLink) {
    if (save.innerText === "Save" && back.innerText === "Back")
        back.innerText = "Discard?";
    else window.location.assign(parentLink);
}

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
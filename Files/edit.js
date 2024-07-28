let id = GetId();
let ch = 0;
let ta = document.getElementById("text");
let editor = document.getElementById("editor");
let sidebar = document.querySelector(".sidebar");
let full = document.querySelector(".full");
let save = document.getElementById("save");
let back = document.getElementById("back");
window.onresize = Resize;
ta.onclick = Refocus;
Resize();
Load();
document.addEventListener('keydown', e => {
    if (e.ctrlKey && e.key === 's') {
        e.preventDefault();
        Save();
    }
});
window.addEventListener("beforeunload", e => {
    if (save.innerText === "Save" && back.innerText == "Back") {
        var confirmationMessage = "You have unsaved changes!";
        (e || window.event).returnValue = confirmationMessage;
        return confirmationMessage;
    }
});

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

function Resize() {
    var fullComp = window.getComputedStyle(full);
    var editorComp = window.getComputedStyle(editor);
    var newHeight = Math.floor(window.visualViewport.height - parseFloat(editorComp['marginTop']) - parseFloat(fullComp['paddingTop']) - parseFloat(fullComp['paddingBottom']));
    editor.style.flex = '1';
    editor.style.height = newHeight + 'px';
    Refocus();
}

function Refocus() {
    var nh = ta.clientHeight;
    if (ch > nh && document.activeElement === ta) {
        ta.blur();
        ta.focus();
    }
    ch = nh;
}

async function Load() {
    try {
        var response = await fetch(`edit/load?id=${id}`);
        switch (response.status) {
            case 200:
                ta.value = await response.text();
                ta.placeholder = "Enter something...";
                break;
            case 201:
                ta.value = "";
                ta.placeholder = "Enter something...";
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
    save.innerText = "Save";
    save.className = "green";
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
    if (save.innerText === "Save" && back.innerText == "Back")
        back.innerText = "Discard?";
    else window.location.assign(parentLink);
}
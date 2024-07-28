function Search() {
    var elem = document.getElementById("search");
    if (elem.value != "")
        window.location.assign("search?q=" + encodeURIComponent(elem.value));
}
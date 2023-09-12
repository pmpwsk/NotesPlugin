function Search() {
    let elem = document.querySelector("#search");
    if (elem.value != "") {
        window.location.assign("[PATH_PREFIX]/search?q=" + encodeURIComponent(elem.value));
    }
}
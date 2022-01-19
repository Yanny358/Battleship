// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function boardClicked(x, y){
    let urlParams = new URLSearchParams(location.search.substr(1));

    let checkbox = document.querySelector(".orientation")
    let check = "true"
    if (checkbox != null){
        check = document.querySelector(".orientation").checked
        urlParams.set("o",check)
    }
    else {
        urlParams.delete("o")
    }

    urlParams.delete("gameToLoad")

    urlParams.set("x",x)
    urlParams.set("y",y)
    location.search = urlParams.toString();
    
}
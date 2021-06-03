function bindSync() {
    let syncButton = document.getElementsByClassName("sync-start")[0];
    if (syncButton) {
        syncButton.addEventListener("click", function () {

            let sourceHub = document.getElementById("sourcehub-input").value;
            let targetHub = document.getElementById("targethub-input").value;
            let days = document.getElementById("days-input").value;
            var xhttp = new XMLHttpRequest();
            var url = "https://localhost:44380/api/content/syncupdated?days=" + days + "&sourcehub=" + sourceHub + "&targethub=" + targetHub;
            //make api call
            xhttp.open("GET", url, true);
            xhttp.setRequestHeader("Content-type", "application/json");
            xhttp.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) {
                    console.log(this.responseText);
                    if (this.responseText) {
                        const data = JSON.parse(this.responseText);
                        data.forEach(function (item) {

                            var itemLibrary = item.libraryName;
                            var itemName = item.itemName;
                            var itemId = item.itemId;
                            var libraryId = item.libraryId;
                            var fileName = item.filename;

                            CreateElement(itemId, itemLibrary, libraryId, itemName,fileName);


                        });
                        var contentItems = document.getElementById("content-items");

                        if (contentItems) {

                            var pushButton = document.getElementsByClassName("button-push")[0];
                            pushButton.classList.remove("hide");
                        }
                    }
                }
            };
            xhttp.send();
        });
    }



}

function CreateElement(itemId, libraryName, libraryId, itemName,fileName) {
    var libraryTable = document.getElementById(libraryId);
    if (!libraryTable) {
        var libraryDiv = document.createElement("div");
        libraryDiv.id = "content-items";

        var table = document.createElement("table");
        table.id = libraryId;
        table.classList.add("item-details");
        var tableHead = document.createElement("thead");

        var tableRow = document.createElement("tr");

        var thName = document.createElement("th");
        let itemNameEle = document.createTextNode("ItemName");
        thName.appendChild(itemNameEle);

        let itemIdEle = document.createTextNode("ItemId");
        var thItemId = document.createElement("th");
        thItemId.appendChild(itemIdEle);

        let libraryEle = document.createTextNode("Library");
        var thLibrary = document.createElement("th");
        thLibrary.appendChild(libraryEle);


        var thOveride = document.createElement("th");
        let overrideTh = document.createTextNode("Override");

        thOveride.appendChild(overrideTh);

        tableRow.append(thName);
        tableRow.append(thItemId);
        tableRow.append(thLibrary);
        tableRow.append(thOveride);
        tableHead.append(tableRow);
        table.append(tableHead);

        var tablebody = document.createElement("tbody");
        table.append(tablebody);

        var resultsContainer = document.getElementsByClassName("sync-result")[0];
        libraryDiv.appendChild(table);
        resultsContainer.appendChild(libraryDiv);
    }

    var table = document.getElementById(libraryId);
    if (table) {

        var tBody = document.getElementById(libraryId).getElementsByTagName('tbody')[0];
        var tableRow = document.createElement("tr");
        let itemIdElem = document.createTextNode(itemId);
        let libraryEle = document.createTextNode(libraryName);
        let ItemNameEle = document.createTextNode(itemName);

        var thName = document.createElement("td");
        thName.appendChild(ItemNameEle);

        var thItemId = document.createElement("td");
        thItemId.appendChild(itemIdElem);

        var thLibrary = document.createElement("td");
        thLibrary.appendChild(libraryEle);

        var thOveride = document.createElement("td");
        var checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.id = "item_status" + itemId;
        checkbox.name = "item_status";
        checkbox.classList.add("item_override");
        checkbox.value = fileName;
        thOveride.appendChild(checkbox);

        var newRow = tBody.insertRow();
        var newCell = newRow.insertCell();
        newCell.append(thName);

        var newCell2 = newRow.insertCell();
        newCell2.append(thItemId);

        var newCell3 = newRow.insertCell();
        newCell3.append(thLibrary);

        var newCell4 = newRow.insertCell();
        newCell4.append(thOveride);
    }

}


function pushContent() {

    let pushSync = document.getElementsByClassName("push-sync")[0];
    if (pushSync) {
        pushSync.addEventListener("click", function () {

            var checkedValue = [];
            var inputElements = document.getElementsByClassName('item_override');
            for (var i = 0; inputElements[i]; ++i) {
                if (inputElements[i].checked) {
                    checkedValue.push(inputElements[i].value);
                    
                }
            }

            console.log(checkedValue);

            let targetHub = document.getElementById("targethub-input").value;
            var pushParams = {};
            
            var itemIds = checkedValue.join('|');           
            pushParams["filePaths"] = itemIds;
            pushParams["targethub"] = targetHub;

            var xHttp = new XMLHttpRequest();
            var url = "https://localhost:44380/api/content/pushcontent?filepaths=" + itemIds;
            //make api call
            xHttp.open("GET", url, true);;
            xHttp.setRequestHeader("Content-type", "application/json");
            xHttp.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) {
                    console.log(this.responseText);
                    if (this.responseText) {
                       
                    }
                }
            };
            var jsonparams = JSON.stringify(pushParams);

            //xHttp.send("data=" + jsonparams);  
            xHttp.send();
        });
    }
}



window.onload = function () {
    bindSync();
    pushContent();
}
function bindSync() {
    let syncButton = document.getElementsByClassName("sync-start")[0];
    //let overlay = document.getElementsByClassName("overlay")[0];
    if (syncButton) {
        syncButton.addEventListener("click", function () {
            DeleteTable();
            DeletePushOutput();
            let syncId = syncButton.getAttribute("data-sync-id");
            let sourceHub = document.getElementById("sourcehub-input").value;
            let targetHub = document.getElementById("targethub-input").value;
            if (sourceHub == targetHub) { window.alert("Source Hub and Target Hub cannot be the same!"); return; }

            let library = document.getElementById("library-input").value;
            let datefrom = document.getElementById("date-from").value;
            let dateto = document.getElementById("date-to").value;
            let date = new Date(datefrom).toISOString();
            let date2 = new Date(dateto).toISOString();
            /*
            let days = document.getElementById("days-input").value;
            if (days.length == 0) {
                window.alert("Days should be greater than 0"); return; 
            }*/
            var xhttp = new XMLHttpRequest();
            var url = "/api/content/syncupdated?startDate=" + date + "&endDate=" + date2 + "&sourcehub=" + sourceHub + "&targethub=" + targetHub + "&syncId=" + syncId + "&library=" + library;
            ToggleOverlay("Pulling Artifacts from " + sourceHub + " from " + date);
            //make api call
            xhttp.open("GET", url, true);
            xhttp.setRequestHeader("Content-type", "application/json");
            xhttp.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) {
                    console.log(this.responseText);
                    if (this.responseText) {
                        const data = JSON.parse(this.responseText);
                        let renderedId = [];
                        data.forEach(function (item) {

                            var itemLibrary = item.LibraryName;
                            var itemName = item.ItemName;
                            var itemId = item.ItemId;
                            var libraryId = item.LibraryId;
                            var fileName = item.Filename;
                            var modifiedDate = new Date(item.ModifiedDate).toLocaleString();
                            var targetHubItemName = item.TargetHubItemName;
                            var targetHubModifiedDate = new Date(item.TargetHubModifiedDate).toLocaleString();
                            var assets = item.Assets;

                            UpdateLocalStorage(fileName, assets);

                            var associatedItems = item.ReferencedItemIds;

                            if (!renderedId.includes(itemId)) {
                                CreateElement(itemId, itemLibrary, libraryId, itemName, fileName, modifiedDate, targetHubItemName, targetHubModifiedDate, false);
                                if (associatedItems.length > 0) {
                                    var associatedItemsRendered = renderSubItems(data, associatedItems, renderedId);
                                    renderedId = renderedId.concat(associatedItemsRendered);
                                }
                                renderedId.push(itemId);
                            }

                            console.log('Item with id ' + itemId + 'has reference to ' + associatedItems);


                        });
                        var contentItems = document.getElementsByClassName("content-items");

                        if (contentItems) {

                            var pushButton = document.getElementsByClassName("button-push")[0];
                            pushButton.classList.remove("hide");
                        }
                        //overlay.style.display = "none";
                        ToggleOverlay();
                        updateAllCheckbox();
                    }
                }
            };
            xhttp.send();


        });


    }
}

function renderSubItems(originList, associatedIds, renderedIds) {
    originList.forEach(function (item) {
        var itemId = item.ItemId;
        if (associatedIds.includes(itemId)) {
            if (!renderedIds.includes(itemId)) {

                var itemLibrary = item.LibraryName;
                var itemName = item.ItemName;
                var libraryId = item.LibraryId;
                var fileName = item.Filename;
                var modifiedDate = new Date(item.ModifiedDate).toLocaleString();
                var targetHubItemName = item.TargetHubItemName;
                var targetHubModifiedDate = new Date(item.TargetHubModifiedDate).toLocaleString();
                var assets = item.Assets;
                UpdateLocalStorage(fileName, assets);
                CreateElement(itemId, itemLibrary, libraryId, itemName, fileName, modifiedDate, targetHubItemName, targetHubModifiedDate, true);
                renderedIds.push(itemId);
            }
        }
    });

    return renderedIds;



}

function ToggleOverlay(msg) {
    let overlay = document.getElementsByClassName("overlay")[0];
    let overlaymsg = document.getElementsByClassName("overlay-msg-text")[0];
    let mainContentPage = document.getElementsByClassName("main-content-page")[0];

    if (overlay.style.display == "none") {
        overlay.style.display = "block";
        overlaymsg.innerHTML = msg;
        mainContentPage.classList.add("blurred-background");
    }
    else {
        overlay.style.display = "none";
        overlaymsg.innerHTML = "";
        mainContentPage.classList.remove("blurred-background");
    }
}

function UpdateLocalStorage(fileName, assets) {
    if (assets != null && assets.length > 0) {
        let assetsPath = ''
        for (let i = 0; i < assets.length; i++) {
            assetsPath = assetsPath + '|' + assets[i].Path;
        }
        localStorage.setItem(fileName, assetsPath);
    }
}

function DeleteTable() {
    let contentItem = document.getElementsByClassName("sync-result")[0];
    if (contentItem != null) {
        contentItem.innerHTML = "";
    }
    var pushButton = document.getElementsByClassName("button-push")[0];
    pushButton.classList.add("hide");
}

function CreateElement(itemId, libraryName, libraryId, itemName, fileName, modifiedDate, targetItemName, targetModifiedDate, isChild) {
    var libraryTable = document.getElementById(libraryId);
    if (!libraryTable) {
        var libraryDiv = document.createElement("div");
        libraryDiv.id = "content-items" + libraryId;
        libraryDiv.classList.add("content-items");

        var table = document.createElement("table");
        table.id = libraryId;
        table.classList.add("item-details");
        table.classList.add("table");
        table.classList.add("table-striped");
        var tableHead = document.createElement("thead");

        var tableRow = document.createElement("tr");

        var thName = document.createElement("th");
        let itemNameEle = document.createTextNode("ItemName");
        thName.appendChild(itemNameEle);

        let itemIdEle = document.createTextNode("ItemId");
        var thItemId = document.createElement("th");
        thItemId.appendChild(itemIdEle);

        let modifiedDateEle = document.createTextNode("ModifiedDate");
        var thModifiedDate = document.createElement("th");
        thModifiedDate.appendChild(modifiedDateEle);


        let existEle = document.createTextNode("Exist in the target hub");
        var thExist = document.createElement("th");
        thExist.appendChild(existEle);

        var thOveride = document.createElement("th");
        let overrideTh = document.createTextNode("Override");

        let divContainer = document.createElement("div");
        let overrideAll = document.createElement("input");
        overrideAll.type = "checkbox";
        overrideAll.id = "override-all_" + libraryId;
        overrideAll.value = libraryId;
        overrideAll.classList.add("override-all-chk");
        overrideAll.innerText = "Override All";
        var label = document.createElement('label')
        label.htmlFor = "override-all_" + libraryId;
        label.appendChild(document.createTextNode('Override All'));
        label.classList.add("label-chk");

        divContainer.appendChild(label);
        divContainer.appendChild(overrideAll);

        //thOveride.appendChild(overrideTh);
        thOveride.appendChild(divContainer);

        tableRow.append(thName);
        tableRow.append(thItemId);
        tableRow.append(thModifiedDate);
        tableRow.append(thExist);
        tableRow.append(thOveride);
        tableHead.append(tableRow);
        table.append(tableHead);

        var tablebody = document.createElement("tbody");
        table.append(tablebody);

        var titleEle = document.createTextNode("Library Name: " + libraryName);
        var pTitle = document.createElement("p");
        pTitle.classList.add("lead");
        pTitle.classList.add("text-left");
        pTitle.appendChild(titleEle);

        var resultsContainer = document.getElementsByClassName("sync-result")[0];
        libraryDiv.appendChild(pTitle);
        libraryDiv.appendChild(table);
        resultsContainer.appendChild(libraryDiv);
    }

    var table = document.getElementById(libraryId);
    if (table) {

        var tBody = document.getElementById(libraryId).getElementsByTagName('tbody')[0];
        var tableRow = document.createElement("tr");
        let itemIdElem = document.createTextNode(itemId);
        let modifiedDateEle = document.createTextNode(modifiedDate);
        let ItemNameEle = document.createTextNode(itemName);
        let existText = ""
        if (targetItemName != null && targetModifiedDate != null) {
            existText = "Yes<br/>Item Name:" + targetItemName + "<br/>Modified Date:" + targetModifiedDate;
        } else {
            existText = "No"
        }
        //let existEle = document.createTextNode(existText);

        var thName = document.createElement("td");
        if (isChild)
        {
            thName.classList.add("inner-child");
        }
        thName.appendChild(ItemNameEle);

        var thItemId = document.createElement("td");
        thItemId.appendChild(itemIdElem);

        var thModifiedDate = document.createElement("td");
        thModifiedDate.appendChild(modifiedDateEle);

        var thExist = document.createElement("td");
        thExist.innerHTML = existText;//appendChild(existEle);

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
        newCell3.append(thModifiedDate);

        var newCell4 = newRow.insertCell();
        newCell4.append(thExist);

        var newCell5 = newRow.insertCell();
        newCell5.append(thOveride);
    }



}


function updateAllCheckbox() {
    var checkboxes = document.getElementsByClassName("override-all-chk");
    for (var i = 0; i < checkboxes.length; i++) {
        checkboxes[i].addEventListener("click", function () {
            var table = document.getElementById(this.value);
            var checkboxesInTable = table.getElementsByClassName("item_override");

            if (this.checked) {
                for (var j = 0; j < checkboxesInTable.length; j++) {
                    checkboxesInTable[j].checked = true;
                }
            }
            else {
                for (var k = 0; k < checkboxesInTable.length; k++) {
                    checkboxesInTable[k].checked = false;
                }
            }

        });
    }
}

function pushContent() {

    let pushSync = document.getElementsByClassName("push-sync")[0];
    let overlay = document.getElementsByClassName("overlay")[0];
    if (pushSync) {
        let syncId = pushSync.getAttribute("data-sync-id");
        pushSync.addEventListener("click", function () {

            var checkedValue = [];
            var inputElements = document.getElementsByClassName('item_override');
            for (var i = 0; inputElements[i]; ++i) {
                if (inputElements[i].checked) {
                    checkedValue.push(inputElements[i].value);

                }
            }

            let targetHub = document.getElementById("targethub-input").value;
            var pushParams = {};

            var itemIds = checkedValue.join('|');
            pushParams["filePaths"] = itemIds;
            pushParams["targethub"] = targetHub;
            overlay.style.display = "block";
            var xHttp = new XMLHttpRequest();
            var url = "/api/content/pushcontent?filepaths=" + itemIds + "&syncId=" + syncId;
            //make api call
            xHttp.open("GET", url, true);;
            xHttp.setRequestHeader("Content-type", "application/json");
            xHttp.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) {
                    console.log(this.responseText);
                    if (this.responseText) {
                        overlay.style.display = "none";
                    }
                }
            };
            var jsonparams = JSON.stringify(pushParams);

            //xHttp.send("data=" + jsonparams);  
            xHttp.send();
        });
    }
}


function pushArtifacts() {
    let pushSync = document.getElementsByClassName("push-sync")[0];
    DeletePushOutput();
    if (pushSync) {
        let syncId = pushSync.getAttribute("data-sync-id");
        pushSync.addEventListener("click", function () {
            var checkedValue = [];
            var inputElements = document.getElementsByClassName('item_override');
            for (var i = 0; i < inputElements.length; i++) {
                if (inputElements[i].checked) {
                    //get assets
                    let contentDetails = {};
                    contentDetails.item = inputElements[i].value;
                    let assets = [];
                    if (window.localStorage.getItem(inputElements[i].value)) {
                        assets = window.localStorage.getItem(inputElements[i].value).split("|");
                    }
                    contentDetails.assets = assets;
                    checkedValue.push(contentDetails);
                }
            }
            if (checkedValue.length < 1) {
                window.alert("Please select which item to push");
                return;
            }
            ToggleOverlay("Pushing Artifacts... Please wait!");
            window.localStorage.clear();
            let targetHub = document.getElementById("targethub-input").value;
            let sourceHub = document.getElementById("sourcehub-input").value;

            var pushParams = {};
            pushParams["contentDetails"] = checkedValue;
            pushParams["syncId"] = syncId;
            pushParams['sourceHub'] = sourceHub;
            pushParams['targetHub'] = targetHub;

            var xHttp = new XMLHttpRequest();
            var url = "/api/content/PushContentV2";
            xHttp.open("POST", url, true);
            xHttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            xHttp.onreadystatechange = function () {
                if (this.readyState == 4 && this.status == 200) {
                    console.log(this.responseText);
                    if (this.responseText) {
                        ToggleOverlay();
                        const data = JSON.parse(this.responseText);
                        showPushOutput(data);
                    }
                }
            };
            var jsonparams = JSON.stringify(pushParams)

            xHttp.send("pushParams=" + jsonparams);


        });
    }
}

function showPushOutput(output) {
    DeleteTable();
    var outputElement = document.createElement("div");
    outputElement.classList.add("push-output");

    var table = document.createElement("table");
    table.id = "push-output-res";
    table.classList.add("table");
    var tableHead = document.createElement("thead");

    var tableRow = document.createElement("tr");

    var thName = document.createElement("th");
    let result = document.createTextNode("Response");
    thName.appendChild(result);

    tableRow.append(thName);
    tableHead.append(tableRow);
    table.append(tableHead);

    var tablebody = document.createElement("tbody");
    table.append(tablebody);

    var resultsContainer = document.getElementsByClassName("sync-result")[0];
    outputElement.appendChild(table);
    resultsContainer.appendChild(outputElement);

    if (output.length > 0) {
        for (let i = 0; i < output.length; i++) {
            var tBody = document.getElementById("push-output-res").getElementsByTagName("tbody")[0];
            let responseTextNode = document.createTextNode(output[i]);
            var thName = document.createElement("td");
            thName.appendChild(responseTextNode);
            var newRow = tBody.insertRow();
            var newCell = newRow.insertCell();
            newCell.append(thName);
        }
    }
}

function DeletePushOutput() {
    let contentItem = document.getElementById('push-output-res');
    if (contentItem != null) {
        contentItem.remove();
    }
}

function InitDatePickerMax() {
    var dateElements = document.querySelectorAll('input[type=date]');
    var dateNow = new Date();
    var month = (dateNow.getMonth() + 1) + "";
    month = month.length > 1 ? month : "0" + month;
    var day = dateNow.getDate().toString();
    day = day.length > 1 ? day : "0" + day;
    dateElements.forEach(function (dateElement) {
        dateElement.setAttribute("max", dateNow.toISOString().slice(0, 10));

    });
}

function SetDateBoundary() {
    const dateFromElement = document.getElementById('date-from');
    const dateToElement = document.getElementById('date-to');

    dateFromElement.addEventListener('change', (event) => {
        dateToElement.setAttribute("min", dateFromElement.value);
        /*var format = dateFromElement.getAttribute("data-date-format");
        dateFromElement.setAttribute("data-date", moment(dateFromElement.value, "YYYY-MM-DD").format(format));*/
    });

    dateToElement.addEventListener('change', (event) => {
        dateFromElement.setAttribute("max", dateToElement.value);
    });
}


window.onload = function () {
    bindSync();
    //pushContent();
    pushArtifacts();
    InitDatePickerMax();
    SetDateBoundary();

}
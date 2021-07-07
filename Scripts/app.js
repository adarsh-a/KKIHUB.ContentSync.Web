function bindSync() {
	let syncButton = document.getElementsByClassName("sync-start")[0];
	if (syncButton) {
		syncButton.addEventListener("click", function () {

			let sourceHub = document.getElementById("sourcehub-input").value;
			let targetHub = document.getElementById("targethub-input").value;
			let library = document.getElementById("library-input").value;
			let datefrom = document.getElementById("date-from").value;
			let date = new Date(datefrom).toISOString();
			/*let days = (new Date(datefrom).getTime() - Date.now())/(1000*3600*24); 
			days = Math.ceil(Math.abs(days));*/
			var xhttp = new XMLHttpRequest();
			var url = "/api/content/syncupdated?startDate=" + date + "&sourcehub=" + sourceHub + "&targethub=" + targetHub + "&library=" + library;
			//make api call
			xhttp.open("GET", url, true);
			xhttp.setRequestHeader("Content-type", "application/json");
			xhttp.onreadystatechange = function () {
				if (this.readyState == 4 && this.status == 200) {
					console.log(this.responseText);
					if (this.responseText) {
						const data = JSON.parse(this.responseText);
						/*var libraries = data.map(a => a.LibraryName);
						console.log(libraries);*/
						data.forEach(function (item) {

							var itemLibrary = item.LibraryName;
							var itemName = item.ItemName;
							var itemId = item.ItemId;
							var libraryId = item.LibraryId;
							var fileName = item.Filename;
							var modifiedDate = item.ModifiedDate;

							CreateElement(itemId, itemLibrary, libraryId, itemName, fileName, modifiedDate);


						});
						var contentItems = document.getElementsByClassName("content-items");

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

function CreateElement(itemId, libraryName, libraryId, itemName, fileName, modifiedDate) {
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

		var thOveride = document.createElement("th");
		let overrideTh = document.createTextNode("Override");

		thOveride.appendChild(overrideTh);

		tableRow.append(thName);
		tableRow.append(thItemId);
		tableRow.append(thModifiedDate);
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

		var thName = document.createElement("td");
		thName.appendChild(ItemNameEle);

		var thItemId = document.createElement("td");
		thItemId.appendChild(itemIdElem);

		var thModifiedDate = document.createElement("td");
		thModifiedDate.appendChild(modifiedDateEle);

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
			var url = "/api/content/pushcontent?filepaths=" + itemIds;
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


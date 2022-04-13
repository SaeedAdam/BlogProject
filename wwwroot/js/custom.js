let index = 0;

function AddTag() {
    var tagEntry = document.getElementById("TagEntry");

    //LOOK FOR ERRO STATE
    let searchResult = Search(tagEntry.value);

    if (searchResult != null) {
        swalWithDarkButton.fire({
            html: `<span class = 'font-weight-bolder'>${searchResult.toUpperCase()}</span>`
        });
    }
    else {
        let newOption = new Option(tagEntry.value, tagEntry.value);

        document.getElementById("TagList").options[index++] = newOption;
    }



    tagEntry.value = "";

    return true;
}

function DeleteTag() {
    let tagCount = 1;

    let tagList = document.getElementById("TagList");

    if (!tagList) {
        return false;
    }

    if (tagList.selectedIndex == -1) {
        swalWithDarkButton.fire({
            html: "<span class = 'font-weight-bolder'>CHOOSE A TAG BEFORE DELETING</>"
        });
        return true;
    }

    while (tagCount > 0) {

        if (tagList.selectedIndex >= 0) {
            tagList.options[tagList.selectedIndex] = null;
            --tagCount;
        }
        else {
            tagCount = 0;
            index--;
        }
    }
}

$("form").on("submit", function () {
    $("#TagList option").prop("selected", "selected");
})

if (tagValues != "") {
    let tagArray = tagValues.split(",");
    for (let loop = 0; loop < tagArray.length; loop++) {
        // Load or replace current options
        ReplaceTag(tagArray[loop], loop);
        index++;
    }
}

function ReplaceTag(tag, index) {
    let newOption = new Option(tag, tag);
    document.getElementById("TagList").options[index] = newOption;
}

//SEARCH FUNCTION WILL DETECT EITHER AN EMPTY OR A DUPLICATE TAG IN A POST
//RETURN ERROR STRING IF ERROR IS FOUND
function Search(str) {
    if (str == "") {
        return "Empty tags are not permited";
    }

    var tagsElement = document.getElementById("TagList");

    if (tagsElement) {
        let options = tagsElement.options;

        for (let i = 0; i < options.length; i++) {
            if (options[i].value == str)
                return `The Tag #${str} was detected as a duplicate and not permitted`
        }
    }

}


const swalWithDarkButton = Swal.mixin({
    customClass: {
        confirmButton: 'btn btn-danger btn-sm note-btn-block btn-outline-dark'
    },
    timer: 5000,
})
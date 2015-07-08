var pieceSize = 1024 * 1024 * 1;


function fileSelected() {
    document.getElementById('fileInfo').innerHTML = '';
    var event = arguments.callee.caller.arguments[0] || event;
    var evt = event.srcElement || event.target;
    var fileCount = evt.files.length;
    for (var i = 0; i < fileCount; i++) {
        var file = evt.files[i];
        var fileSize = 0;
        if (file.size > 1024 * 1024 * 1024) {
            fileSize = (Math.round(file.size * 100 / (1024 * 1024 * 1024)) / 100).toString() + 'GB';
        } else if (file.size > 1024 * 1024) {
            fileSize = (Math.round(file.size * 100 / (1024 * 1024)) / 100).toString() + 'MB';
        } else {
            fileSize = (Math.round(file.size * 100 / 1024) / 100).toString() + 'KB';
        }
        var fileinfo = '<ul><li>Name: ' + file.name + '</li><li>Size: ' + fileSize + '</li><li>Type: ' + file.type + '</li></ul>';
        document.getElementById('fileInfo').innerHTML += fileinfo;
    }
}

function uploadFileTogether() {
    var files = document.getElementById('fileToUpload').files;
    var fd = new FormData();
    for (var i = 0; i < files.length; i++) {
        fd.append(files[i].name, files[i]);
    }
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            var date = new Date();
            console.log('file compelate: ' + date.getHours() + ':' + date.getMinutes() + ':' + date.getSeconds() + '.' + date.getMilliseconds());
        }
    }
    xhr.onloadstart = function (e) {
        var date = new Date();
        console.log('file start: ' + date.getHours() + ':' + date.getMinutes() + ':' + date.getSeconds() + '.' + date.getMilliseconds());
    }
    xhr.onerror = function (e) {
        var date = new Date();
        console.log('file upload error' + e.message + ' - ' + date.getHours() + ':' + date.getMinutes() + ':' + date.getSeconds() + '.' + date.getMilliseconds());
    }
    xhr.open("POST", "../Home/MyUpload", true);
    xhr.send(fd);
}

function uploadBigFileByBlob() {
    var files = document.getElementById('fileToUpload').files;
    if (!files.length) {
        return false;
    }
    var file = files[0];
    var filesize = file.size;
    var pieceCount = Math.round(filesize / pieceSize);
    var reader = new FileReader();
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            var index = xhr.response.index;
            console.log(index / 1024 / 1024 + '    ' + index);
            if (index < file.size) {
                var lastindex = index + pieceSize;
                lastindex = lastindex < file.size ? lastindex : file.size;
                var blob = file.slice(index, lastindex);
                xhr.open('post', '../Home/TransferData', true);
                xhr.responseType = 'json';
                xhr.send(blob);
            }
        }
    }
    var fileinfo = { filename: file.name, filesize: file.size };
    xhr.open('post', '../Home/SetFileInfo', true);
    xhr.responseType = 'json';
    xhr.send(JSON.stringify(fileinfo));
    console.log(file.size);
}

function uploadfiles() {
    var files = $("#fileToUpload")[0].files;
    for (var i = 0; i < files.length; i++) {
        uploadBigFileByFormData(files[i]);
    }

}

function uploadBigFileByFormData(file) {
    var pieceCount = Math.ceil(file.size / pieceSize);
    var xhr = new XMLHttpRequest();
    xhr.responseType = 'json';
    var index = 1;
    var formdata = new FormData();
    formdata.append("filename", file.name);
    formdata.append("pieceCount", pieceCount);
    formdata.append("curPiece", index);
    formdata.append("data", file.slice((index - 1) * pieceSize, index * pieceSize - 1));
    xhr.open('post', '../Home/UploadByPiece', true);
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            var result = xhr.response;
            console.log(result.nxtPiece);
            index = result.nxtPiece - 1;
            index++;
            if (index <= pieceCount) {
                formdata = new FormData();
                formdata.append("filename", file.name);
                formdata.append("pieceCount", pieceCount);
                formdata.append("curPiece", index);
                formdata.append("data", file.slice((index - 1) * pieceSize, index * pieceSize - 1));
                xhr.open('post', '../Home/UploadByPiece', true);
                xhr.send(formdata);
            }
        }
    }
    xhr.send(formdata);
}

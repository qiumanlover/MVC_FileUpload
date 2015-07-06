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

function uploadBigFilePiece() {
    var files = document.getElementById('fileToUpload').files;
    if (!files.length) {
        return false;
    }
    var file = files[0];
    var filesize = file.size;
    var pieceCount = Math.round(filesize / (1024 * 1024));
    var reader = new FileReader();
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            var index = xhr.response.index;
            console.log(index / 1024 /1024 + '    ' + index);
            if (index < file.size) {
                var lastindex = index + 1024 * 1024;
                lastindex = lastindex < file.size ? lastindex : file.size;
                var blob = file.slice(index, lastindex);
                xhr.open('post', '../Home/TransferData', true);
                xhr.responseType = 'json';
                xhr.send(blob);
            }
        }
    }
    //reader.onload = function (evt) {
    //    alert(evt.target.result);
    //}
    //reader.readAsBinaryString(blob);
    var fileinfo = { filename:file.name, filesize:file.size };
    xhr.open('post', '../Home/SetFileInfo', true);
    xhr.responseType = 'json';
    xhr.send(JSON.stringify(fileinfo));
    console.log(file.size);
}
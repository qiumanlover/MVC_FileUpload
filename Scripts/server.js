    var http = require('http');
    var querystring = require('querystring');
    var util = require('util');
    var buffer = require('buffer');
    var fs = require('fs');

    http.createServer(function (req, res) {
        req.on('data', function (chunk) {
            console.log("buffer type: " + typeof (buffer));
            // for (var key in chunk) {
            //     buffer += key;
            // }
            buffer = chunk;
            console.log(chunk);
            console.log("chunk type: " + typeof (chunk));
            console.log("chunk length: " + chunk.length);
            console.log(buffer);
            console.log("buffer type: " + typeof (buffer));
            console.log("buffer length: " + buffer.length);
            console.log(buffer.length);
            console.log();
        });
        req.on('end', function () {
            var str = buffer.toString();
            console.log(str.length);
            var re = / name="(\w+\b)"\r\n\r\n(\w+)\r\n-/g;
            var r, filename;
            while (r = re.exec(str)) {
                console.log(r[1] + "    " + r[2]);
            }
            re = /name="data"; filename="(.+\.\w+)"\r\nContent-Type:/g;
            r = re.exec(str);
            filename = r[1];
            console.log(filename);

            var ctype = str.match(/Content-Type:.*/g)[0];
            var startindex = str.indexOf("Content-Type:") + ctype.length + 4;
            str = strao.substr(startindex);
            str = str.substring(0, str.indexOf('\r\n-') - 2);

            fs.open('D:\\' + filename, 'w', 0644, function (err, fd) {
                if (err) throw err;
                fs.write(fd, str.substr(0, str.length - 2), function (err) {
                    if (err) throw err;
                    fs.closeSync(fd);
                });
            });

            // while(r = re.exec(buffer)) {
            //     buffer = r[2];
            //     filename = r[1];
            // }
            // buffer = buffer.slice(0, buffer.length-2);
            // fs.open('D:\\' + filename, 'w', 0644, function(err,fd) {
            //     if(err) throw err;
            //     fs.write(fd, buffer, 0, buffer.length, function(err) {
            //        if (err) throw err;
            //        fs.closeSync(fd);
            //     });
            // });
            buffer = new Buffer(0);
            res.end(util.inspect(buffer));
        });
    }).listen(3000);
    console.log("HTTP server is listening at port 3000.");

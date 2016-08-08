var TelegramBot = require('node-telegram-bot-api');
var uuid = require('node-uuid');
var express = require('express');
var request = require('then-request');
var fs = require('fs');
var path = require('path');
 
const execSync = require('child_process').execSync;
const execAsync = require('child_process').exec;
var app = express();
var token = '224831807:AAGNkaCtG-yML_yqw-ZEnU_fvTugyM3D5cM';
// Setup polling way
var bot = new TelegramBot(token, {polling: true});

var lastmsg=[];

// Matches /echo [whatever]
/*bot.onText(/\/echo (.+)/, function (msg, match) {
   chatId = msg.from.id;
  lastmsg.push(match[1]);
 // bot.sendMessage(chatId, resp);
  console.log("chatId"+chatId);
});
*/
// Any kind of message
bot.on('message', function (msg) {
  var chatId = msg.chat.id;
  var idmsg=uuid.v4();
  var textmsg=msg.text;
  lastmsg.push({idmsg,chatId,textmsg});
  //bot.sendMessage(chatId,"ti sto per mandare i gattini:");
  // photo can be: a file path, a stream or a Telegram file_id
  //var photo = 'cats.png';
  //bot.sendPhoto(chatId, photo, {caption: 'Lovely kittens'});
  //console.log("send photo");
  //console.log("msg"+msg);
   console.log("chatId"+chatId);
});



app.get('/receive', function (req, res) {
   res.send(lastmsg);
 //  lastmsg='';
})

app.get('/pop',function(req,res) {
	res.send(lastmsg.pop());
})

app.get('/shift',function(req,res) {
	res.send(lastmsg.shift());
})
app.get('/video/:idchat',function(req,res) {
	var idchat=req.params.idchat
	var idvideo=uuid.v4();
	bot.sendMessage(idchat,"sending video...");
	var width = req.query.width;
	var height = req.query.height;
	var time = req.query.time;
	var msg = req.query.msg;
	
	var filename = '/tmp/'+idvideo+'.h264'
	var cmd = 'raspivid -o ' + filename;
	if (undefined != width)  cmd = cmd + ' -w ' + width;
	if (undefined != height) cmd = cmd + ' -h ' + height; 
	if (undefined != time) cmd = cmd + ' -t ' + time; 
	console.log(cmd);
	code = execSync(cmd);
	code2 = execSync('avconv -r 30 -i /tmp/'+idvideo+'.h264 -vcodec copy /tmp/'+idvideo+'.mp4');
	if (undefined != msg) 
		bot.sendVideo(idchat,'/tmp/'+idvideo+'.mp4', {caption: msg});
	else 
		bot.sendVideo(idchat,'/tmp/'+idvideo+'.mp4');
	res.send('send video');
 
	
})
app.get('/cmd',function(req,res){
	code = execSync('dir /w');
	res.send(code);
});
app.get('/photo/:idchat',function(req,res) {
	var idchat=req.params.idchat
	 var idphoto=uuid.v4();
	 var width = req.query.width;
	 var height = req.query.height;
	 var quality = req.query.quality;
	 var msg = req.query.msg;
	 var filename = '/tmp/'+idphoto+'.jpg'
	 var cmd = 'raspistill -o ' + filename;
     bot.sendMessage(idchat,"sending photo...");
	 if (undefined != width)  cmd = cmd + ' -w ' + width 
	 if (undefined != height) cmd = cmd + ' -h ' + height 
	 if (undefined != quality) cmd = cmd + ' -q ' + quality;
	 //if (undefined != time) cmd = cmd + ' -t ' + time; 
	 console.log(cmd);
	 
	 code = execSync(cmd)
	 var img = fs.readFileSync(filename);
     var resvision = request('POST','https://api.projectoxford.ai/vision/v1.0/analyze?visualFeatures=Description',
						{ 
							headers:{
								'Ocp-Apim-Subscription-Key': '255ec2de41124b42a6ae6428f7f03b84',
								'Content-type': ' application/octet-stream'
								},
							body:img

						}).done(function(response) {
								console.log(response.getBody().toString('utf-8')); 
								bot.sendPhoto(idchat, filename, {caption: JSON.parse(response.getBody().toString('utf-8')).description.captions[0].text});
								res.send('send photo');
								res.end();
							}); 
							  
    });
 
							

app.get('/text/:idchat',function(req,res) {
 var idchat = req.params['idchat'];
 var txt = req.query.text ;
 bot.sendMessage(idchat,text);
 res.send('send msg');
});


var server = app.listen(8081, function () {

  var host = server.address().address
  var port = server.address().port

  console.log("Example app listening at http://%s:%s", host, port)

})
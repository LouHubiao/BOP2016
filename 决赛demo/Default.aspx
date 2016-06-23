<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="css/pagestyle.css" type="text/css" rel="stylesheet" />
    <link href="css/fullscreenstyle.css" type="text/css" rel="stylesheet" />
    <script src="js/speech.1.0.0.js" type="text/javascript"></script>
    <script type="text/javascript" src="js/jquery.min.js"></script>
    <script type="text/javascript" src="js/jquery.rotate.min.js"></script>
    <script type="text/javascript" src="js/jquery.fullscreenslides.js"></script>
    <script type="text/javascript" src="js/Action.js"></script>
    <script type="text/javascript">
        function getImgByIndex(index) {
            var images = document.getElementById("images");
            if (index < images.children.length) {
                var img = images.children[index].firstChild;
                return img;
            }
            return null;
        }
    </script>
    <script type="text/javascript">
        var maps;
        $(function () {
            $('.image img').fullscreenslides();
            var $container = $('#fullscreenSlideshowContainer');
            $container.bind("init", function () {
                $container
                .append('<div class="ui" id="fs-close">&times;</div>')
                .append('<div class="ui" id="fs-loader">Loading...</div>')
                .append('<div class="ui" id="fs-prev">&lt;</div>')
                .append('<div class="ui" id="fs-next">&gt;</div>')
                .append('<div class="ui" id="fs-caption"><span></span></div>');
                $('#fs-prev').click(function () {
                    $container.trigger("prevSlide");
                });

                $('#fs-next').click(function () {
                    $container.trigger("nextSlide");
                });

                $('#fs-close').click(function () {
                    $container.trigger("close");
                });

            })
            .bind("startLoading", function () {
                $('#fs-loader').show();
            })
            .bind("stopLoading", function () {
                $('#fs-loader').hide();
            })
            maps = JSON.parse('<%=maps%>');
            //$('#Capture2').click();
            start();
            run();
            
            //setInterval('run()', 1000);
            //console.log("hello"+t);
        });
    </script>
    <script type="text/javascript">
        var client;

        function useMic() {
            return true;
        }

        function getMode() {
            return Microsoft.ProjectOxford.SpeechRecognition.SpeechRecognitionMode.shortPhrase;
        }

        function getOxfordKey() {
            return "b00a2a4ef1ec42b899a2da177c7bf996";
        }

        function getLanguage() {
            return "zh-cn";
        }

        function clearText() {
            document.getElementById("output").value = "";
        }

        function setText(text) {
            document.getElementById("output").value += text;
        }

        function getLuisConfig() {
            return { appid: "ccea6475-7ca6-4fa9-be02-10b0e26533ec", subid: "cea17678fdcf482aa0e37e7362cf9706" };
        }

        function start() {
            var mode = getMode();
            var luisCfg = getLuisConfig();
            
            if (luisCfg) {
                client = Microsoft.ProjectOxford.SpeechRecognition.SpeechRecognitionServiceFactory.createMicrophoneClientWithIntent(
                    getLanguage(),
                    getOxfordKey(),
                    getOxfordKey(),
                    luisCfg.appid,
                    luisCfg.subid);
                init = 1;
            } else {
                client = Microsoft.ProjectOxford.SpeechRecognition.SpeechRecognitionServiceFactory.createMicrophoneClient(
                    mode,
                    getLanguage(),
                    getOxfordKey(),
                    getOxfordKey());
            }

            client.onPartialResponseReceived = function (response) {
                setText(response);
                console.log("onPartialResponseReceived...");
                isRunning = 0;
            }

            client.onFinalResponseReceived = function (response) {
                setText(JSON.stringify(response));
                console.log("onFinalResponseReceived...");
                isRunning = 0;
            }

            client.onIntentReceived = function (response) {
                setText(response);
                var responseJson = JSON.parse(String(response));
                var maxScore = responseJson.intents[0].score;
                var maxIndex = 0;
                window.filterImgs(responseJson.intents[maxIndex].intent, responseJson.text, responseJson.entities);
                console.log("onIntentReceived...");
            };
        }

        var isRunning = 0;
        function run() {
            console.log("running...");
            if (isRunning == 0) {
                console.log("start...");
                isRunning = 1;
                clearText();
                client.startMicAndRecognition();
                setTimeout(function () {
                    //client.endMicAndRecognition();
                    console.log("time out...");
                    isRunning = 0;
                }, 10000);
            }
        }
    </script>
</head>
<body>
    <div class="wrapper">
        <h1>BOP Test</h1>
        <div class="fav-list">
            <%=imgs %>
            <textarea id="output"></textarea>
        </div>
    </div>
</body>
</html>

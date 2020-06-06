var countDownTo = null;

$(function () {

    function fixed(num) {
        var txt = num.toString();
        if (txt.length >= 2) {
            return txt;
        }
        return '0' + txt;
    }

    function updateClock() {
        var dt = new Date();
        $('#clock').text(fixed(dt.getUTCHours()) + ':' + fixed(dt.getUTCMinutes()) + ':' + fixed(dt.getUTCSeconds()));

        if (countDownTo) {
            var delta = Math.ceil((countDownTo.getTime() - new Date().getTime()) / 1000);

            var sec = delta % 60;

            delta = (delta - sec) / 60;
            var min = delta % 60;

            delta = (delta - min) / 60;
            var hours = delta % 24;

            delta = (delta - hours) / 24;
            var days = delta;

            if (days > 0) {
                $('#countdown').text(days + ':' + fixed(hours) + ':' + fixed(min) + ':' + fixed(sec));
            }
            else {
                $('#countdown').text(fixed(hours) + ':' + fixed(min) + ':' + fixed(sec));
            }
        }
    }

    updateClock();

    setInterval(updateClock, 1000);
});
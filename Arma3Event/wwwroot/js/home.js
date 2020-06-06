$(function () {

    function fixed(num) {
        var txt = '00' + num.toString();
        return txt.substr(txt.length - 2);
    }

    function updateClock() {
        var dt = new Date();
        $('#clock').text(fixed(dt.getUTCHours()) + ':' + fixed(dt.getUTCMinutes()) + ':' + fixed(dt.getUTCSeconds()));
    }

    updateClock();

    setInterval(updateClock, 1000);
});
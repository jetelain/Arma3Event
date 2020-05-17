

function getFactionFlag(id) {

    var faction = factions.find(function (entry) { return entry.factionID == id });
    if (faction) {
        return faction.icon;
    }

    return "/img/flags/none.png";
}


$(function () {
    $('select.faction').each(function () {
        var img = $(this).parent().find('img');
        img.attr('src', getFactionFlag($(this).val()));
        $(this).on('change', function () { img.attr('src', getFactionFlag($(this).val())); });
    });
});

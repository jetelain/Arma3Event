function MGRS_CRS(factorx, factory, tileWidth) {
    return L.extend({}, L.CRS.Simple, {

        projection: L.Projection.LonLat,

        transformation: new L.Transformation(factorx, 0, -factory, tileWidth),

        scale: function (zoom) {
            return Math.pow(2, zoom);
        },

        zoom: function (scale) {
            return Math.log(scale) / Math.LN2;
        },

        distance: function (latlng1, latlng2) {
            var dx = latlng2.lng - latlng1.lng,
                dy = latlng2.lat - latlng1.lat;
            return Math.sqrt(dx * dx + dy * dy);
        },

        infinite: true
    });
}
function toCoord(num) {
    var numText = "00000" + num.toFixed(0);
    return numText.substr(numText.length - 5, 5);
}
function toGrid(latlng) {
    return toCoord(latlng.lng) + " - " + toCoord(latlng.lat);
}

function applySymbolSet() {
    var symbolset = $('#set').val();

    $('#size').empty();
    $.each(echelonMobilityTowedarray(symbolset), function (name, value) {
        $('#size').append($('<option></option>').attr({ value: value.code }).text(value.name));
    });

    var data = milstd.app6d[symbolset];

    var grps = {};
    $('#icon').empty();
    $.each(data['main icon'], function (name, value) {
        if (value['entity type']) {
            var grp = grps[value.entity];
            if (!grp) {
                grp = grps[value.entity] = $('<optgroup></optgroup>').attr({ label: value.entity });
                $('#icon').append(grp);
            }
            if (value['entity subtype']) {
                $(grp).append($('<option></option>').attr({ value: value.code }).text(value['entity type'] + " - " + value['entity subtype']));
            } else {
                $(grp).append($('<option></option>').attr({ value: value.code }).text(value['entity type']));
            }
        }
        else {
            $('#icon').append($('<option></option>').attr({ value: value.code }).text(value.entity));
        }
    });

    $('#mod1').empty();
    $.each(data['modifier 1'], function (name, value) {
        $('#mod1').append($('<option></option>').attr({ value: value.code }).text(value.modifier));
    });

    $('#mod2').empty();
    $.each(data['modifier 2'], function (name, value) {
        $('#mod2').append($('<option></option>').attr({ value: value.code }).text(value.modifier));
    });
}
function getSymbol() {
    var symbol = '10';
    symbol += $('#id1').val() || '0';
    symbol += $('#id2').val() || '0';
    symbol += $('#set').val() || '00';
    symbol += $('#status').val() || '0';
    symbol += $('#hq').val() || '0';
    symbol += $('#size').val() || '00';
    symbol += $('#icon').val() || '000000';
    symbol += $('#mod1').val() || '00';
    symbol += $('#mod2').val() || '00';
    return symbol;
}

function addDegreesToConfig(config, name) {
    var value = $('#' + name).val();
    if (value !== '') {
        config[name] = String(Number(value) * 360 / 6400);
    }
}
function addToConfig(config, name) {
    var value = $('#' + name).val();
    if (value !== '') {
        config[name] = value;
    }
}

function getSymbolConfig() {

    var config = {};
    addToConfig(config, 'uniqueDesignation');
    addDegreesToConfig(config, 'direction');
    addToConfig(config, 'location');
    addToConfig(config, 'quantity');
    addToConfig(config, 'staffComments');
    return config;
}

function setSymbol(symbol, config) {

    $('#id1').val(symbol.substr(2, 1));
    $('#id2').val(symbol.substr(3, 1));
    $('#set').val(symbol.substr(4, 2));
    applySymbolSet();

    $('#status').val(symbol.substr(6, 1));
    $('#hq').val(symbol.substr(7, 1));
    $('#size').val(symbol.substr(8, 2));
    $('#icon').val(symbol.substr(10, 6));
    $('#mod1').val(symbol.substr(16, 2));
    $('#mod2').val(symbol.substr(18, 2));

    $('#uniqueDesignation').val(config.uniqueDesignation || '');
    $('#location').val(config.location || '');
    $('#quantity').val(config.quantity || '');
    $('#staffComments').val(config.staffComments || '');
    $('#direction').val(config.direction !== undefined ? (Number(config.direction) * 6400 / 360) : '');

    applySymbol();
}

function applySymbol() {
    var symbol = getSymbol();

    $('#symbolNumber').text(symbol);

    var config = getSymbolConfig();

    config.size = 70;

    var sym = new ms.Symbol(symbol, config);

    $('#symbolPreview').empty();

    $('#symbolPreview').append($('<img></img>')
        .attr({
            src: sym.asCanvas(window.devicePixelRatio).toDataURL(),
            width: sym.getSize().width,
            height: sym.getSize().height

        })
    );

    return symbol;
}

function milsymbolMarkerTool(map) {

    var clickPosition = null;
    var clickMarker = null;
    var insertSymbolBtn;

    function insertSymbolHandler(e) {
        insertSymbolBtn.state('default');
        map.off('click', insertSymbolHandler);
        clickPosition = e.latlng;
        $('#milsymbol').modal('show');
        $('#milsymbol-delete').hide();
        $('#milsymbol-update').hide();
        $('#milsymbol-insert').show();
        $('#milsymbol-grid').text(toGrid(e.latlng));
    };
    function updateMakerHandler(e) {
        clickMarker = e.sourceTarget;
        setSymbol(clickMarker.options._symbol, clickMarker.options._symbolConfig);
        $('#milsymbol').modal('show');
        $('#milsymbol-delete').show();
        $('#milsymbol-update').show();
        $('#milsymbol-insert').hide();
        $('#milsymbol-grid').text(toGrid(e.latlng));
    };

    var sym = new ms.Symbol('10031000001211000000', { size: 8 });
    var btnImg = '<img src="' + sym.asCanvas(window.devicePixelRatio).toDataURL() + '" width="' + sym.getSize().width + '" height="' + sym.getSize().height + '">';
    insertSymbolBtn = L.easyButton({
        states: [{
            stateName: 'default',
            icon: btnImg,
            title: 'Insérer un symbole OTAN APP-6 D',
            onClick: function (btn, map) {
                btn.state('edit');
                map.on('click', insertSymbolHandler);
            }
        }, {
            stateName: 'edit',
            icon: 'fa-window-close',
            title: 'Annuler',
            onClick: function (btn, map) {
                btn.state('default');
                map.off('click', insertSymbolHandler);
            }
        }]
    }).addTo(map);

    $('#milsymbol-insert').on('click', function () {
        var symbol = getSymbol();
        var symbolConfig = getSymbolConfig();
        symbolConfig.size = 32;
        var sym = new ms.Symbol(symbol, symbolConfig);
        var myIcon = L.icon({
            iconUrl: sym.asCanvas(window.devicePixelRatio).toDataURL(),
            iconSize: [sym.getSize().width, sym.getSize().height],
            iconAnchor: [sym.getAnchor().x, sym.getAnchor().y]
        });
        L.marker(clickPosition, { icon: myIcon, draggable: true, _symbol: symbol, _symbolConfig: symbolConfig }).addTo(map).on('click', updateMakerHandler);
        $('#milsymbol').modal('hide');
    });

    $('#milsymbol-delete').on('click', function () {
        clickMarker.remove();
        $('#milsymbol').modal('hide');
    });

    $('#milsymbol-update').on('click', function () {
        var symbol = getSymbol();
        var symbolConfig = getSymbolConfig();
        symbolConfig.size = 32;
        var sym = new ms.Symbol(symbol, symbolConfig);

        var myIcon = L.icon({
            iconUrl: sym.asCanvas(window.devicePixelRatio).toDataURL(),
            iconSize: [sym.getSize().width, sym.getSize().height],
            iconAnchor: [sym.getAnchor().x, sym.getAnchor().y]
        });
        clickMarker.options._symbol = symbol;
        clickMarker.options._symbolConfig = symbolConfig;
        clickMarker.setIcon(myIcon);
        $('#milsymbol').modal('hide');
    });

    $.each(milstd.app6d, function (name, value) {
        $('#set').append($('<option></option>').attr({value: value.symbolset}).text(value.name));
    });

    $('#set').val("10");
    applySymbolSet();

    $('#set').change(applySymbolSet);
    applySymbol();

    $('select').change(applySymbol);
    $('input').change(applySymbol);
}

function basicSymbolMarkerTool(map) {


    var insertSymbolBtn;

    function insertSymbolHandler(e) {
        insertSymbolBtn.state('default');
        map.off('click', insertSymbolHandler);
    };

    insertSymbolBtn = L.easyButton({
        states: [{
            stateName: 'default',
            icon: 'fa-map-marker-alt',
            title: 'Insérer un symbole générique',
            onClick: function (btn, map) {
                btn.state('edit');
                map.on('click', insertSymbolHandler);
            }
        }, {
            stateName: 'edit',
            icon: 'fa-window-close',
            title: 'Annuler',
            onClick: function (btn, map) {
                btn.state('default');
                map.off('click', insertSymbolHandler);
            }
        }]
    }).addTo(map);
}

function InitMap(mapInfos) {

    var map = L.map('map', {
        minZoom: mapInfos.minZoom,
        maxZoom: mapInfos.maxZoom,
        crs: mapInfos.CRS
    });

    L.tileLayer(mapInfos.tilePattern, {
        attribution: mapInfos.attribution,
        tileSize: mapInfos.tileSize
    }).addTo(map);

    map.setView(mapInfos.center, mapInfos.defaultZoom);

    L.latlngGraticule().addTo(map);

    L.control.scale({ maxWidth: 200, imperial: false }).addTo(map);

    milsymbolMarkerTool(map);
    basicSymbolMarkerTool(map);
}


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
    return numText.substr(numText.length - 5, 4);
}
function toGrid(latlng) {
    return toCoord(latlng.lng) + " - " + toCoord(latlng.lat);
}

function applySymbolSet() {
    var id = '0003'; //+ $('#id1').val() + $('#id2').val();
    var symbolset = $('#set').val();

    $('#size').empty();
    $.each(echelonMobilityTowedarray(symbolset), function (name, value) {
        var sym = new ms.Symbol(id + symbolset + '00' + value.code+'0000000000', { size: 16 });
        var labelHtml = '<img class="mil-icon" src="' + sym.asCanvas(window.devicePixelRatio).toDataURL() + '" width="' + sym.getSize().width + '" height="' + sym.getSize().height + '"> ' + value.name;
        $('#size').append($('<option></option>').attr({ value: value.code, 'data-content': labelHtml }).text(value.name));
    });

    var data = milstd.app6d[symbolset];

    var grps = {};
    $('#icon').empty();
    $.each(data['main icon'], function (name, value) {

        var sym = new ms.Symbol(id + symbolset + '0000' + value.code + '0000', { size: 16 });
        var labelHtml = '<img class="mil-icon" src="' + sym.asCanvas(window.devicePixelRatio).toDataURL() + '" width="' + sym.getSize().width + '" height="' + sym.getSize().height + '"> ';
        var labelText = '';

        if (value['entity type']) {
            if (value['entity subtype']) {
                labelHtml += '<span class="font-weight-light text-muted">' + value.entity + ' - ' + value['entity type'] + '</span> - <strong>' + value['entity subtype'] + '</strong>';
                labelText = value.entity + ' - ' + value['entity type'] + ' - ' + value['entity subtype'];
            } else {
                labelHtml += '<span class="font-weight-light text-muted">' + value.entity + '</span> - <strong>' + value['entity type'] + '</strong>';
                labelText = value.entity + ' - ' + value['entity subtype'];
            }
        }
        else {
            labelHtml += '<strong>' + value.entity + '</strong>';
            labelText = value.entity;
        }
        $('#icon').append($('<option></option>').attr({ value: value.code, 'data-content': labelHtml }).text(labelText));
    });

    $('#mod1').empty();
    $.each(data['modifier 1'], function (name, value) {
        var sym = new ms.Symbol(id + symbolset + '0000000000' + value.code + '00', { size: 16 });
        var labelHtml = '<img class="mil-icon" src="' + sym.asCanvas(window.devicePixelRatio).toDataURL() + '" width="' + sym.getSize().width + '" height="' + sym.getSize().height + '"> ' + value.modifier;
        $('#mod1').append($('<option></option>').attr({ value: value.code, 'data-content': labelHtml  }).text(value.modifier));
    });

    $('#mod2').empty();
    $.each(data['modifier 2'], function (name, value) {
        var sym = new ms.Symbol(id + symbolset + '000000000000' + value.code, { size: 16 });
        var labelHtml = '<img class="mil-icon" src="' + sym.asCanvas(window.devicePixelRatio).toDataURL() + '" width="' + sym.getSize().width + '" height="' + sym.getSize().height + '"> ' + value.modifier;
        $('#mod2').append($('<option></option>').attr({ value: value.code, 'data-content': labelHtml }).text(value.modifier));
    });

    $('#size').selectpicker('refresh');
    $('#icon').selectpicker('refresh');
    $('#mod1').selectpicker('refresh');
    $('#mod2').selectpicker('refresh');
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

    $('select').selectpicker('render');
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

var modalMarkerId;
var modalMarkerData;
var clickPosition = null;

function updateMarkerHandler(e) {
    var marker = e.sourceTarget;
    modalMarkerId = marker.options.markerId;
    modalMarkerData = marker.options.markerData;

    if (modalMarkerData.type == 'mil') {
        setSymbol(modalMarkerData.symbol, modalMarkerData.config);
        $('#milsymbol').modal('show');
        $('#milsymbol-delete').show();
        $('#milsymbol-update').show();
        $('#milsymbol-insert').hide();
        $('#milsymbol-grid').text(toGrid(e.latlng));

    } else if (modalMarkerData.type == 'basic') {
        $('#basic-type').val(modalMarkerData.symbol);
        $('#basic-color').val(modalMarkerData.config.color);
        $('#basic-label').val(modalMarkerData.config.label);
        $('select').selectpicker('render');

        $('#basicsymbol').modal('show');
        $('#basicsymbol-delete').show();
        $('#basicsymbol-update').show();
        $('#basicsymbol-insert').hide();
        $('#basicsymbol-grid').text(toGrid(e.latlng));
    }
};
function insertMilSymbol(latlng) {
    clickPosition = latlng;
    $('#milsymbol').modal('show');
    $('#milsymbol-delete').hide();
    $('#milsymbol-update').hide();
    $('#milsymbol-insert').show();
    $('#milsymbol-grid').text(toGrid(latlng));
};
function insertBasicSymbol(latlng) {
    clickPosition = latlng;
    $('#basicsymbol').modal('show');
    $('#basicsymbol-delete').hide();
    $('#basicsymbol-update').hide();
    $('#basicsymbol-insert').show();
    $('#basicsymbol-grid').text(toGrid(latlng));
};

function milsymbolMarkerTool(connection) {

    $('#milsymbol-insert').on('click', function () {
        var symbol = getSymbol();
        var symbolConfig = getSymbolConfig();

        connection.invoke('AddMarker', mapHubInfos.matchID, mapHubInfos.roundSideID, {
            type: 'mil',
            symbol: symbol,
            config: symbolConfig,
            pos: [clickPosition.lat, clickPosition.lng]
        });

        $('#milsymbol').modal('hide');
    });

    $('#milsymbol-delete').on('click', function () {
        connection.invoke('RemoveMarker', modalMarkerId);
        $('#milsymbol').modal('hide');
    });

    $('#milsymbol-update').on('click', function () {
        modalMarkerData.symbol = getSymbol();
        modalMarkerData.config = getSymbolConfig();
        connection.invoke('UpdateMarker', modalMarkerId, modalMarkerData);
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

function basicsymbolMarkerTool(connection) {

    $('#basicsymbol-insert').on('click', function () {
        connection.invoke('AddMarker', mapHubInfos.matchID, mapHubInfos.roundSideID, {
            type: 'basic',
            symbol: $('#basic-type').val(),
            config: { color: $('#basic-color').val(), label: $('#basic-label').val() },
            pos: [clickPosition.lat, clickPosition.lng]
        });
        $('#basicsymbol').modal('hide');
    });

    $('#basicsymbol-delete').on('click', function () {
        connection.invoke('RemoveMarker', modalMarkerId);
        $('#basicsymbol').modal('hide');
    });

    $('#basicsymbol-update').on('click', function () {
        modalMarkerData.symbol = $('#basic-type').val();
        modalMarkerData.config = { color: $('#basic-color').val(), label: $('#basic-label').val() };
        connection.invoke('UpdateMarker', modalMarkerId, modalMarkerData);
        $('#basicsymbol').modal('hide');
    });
}

var basicColors = { "ColorBlack": "000000", "ColorGrey": "7F7F7F", "ColorRed": "E50000", "ColorBrown": "7F3F00", "ColorOrange": "D86600", "ColorYellow": "D8D800", "ColorKhaki": "7F9966", "ColorGreen": "00CC00", "ColorBlue": "0000FF", "ColorPink": "FF4C66", "ColorWhite": "FFFFFF", "ColorUNKNOWN": "B29900", "colorBLUFOR": "004C99", "colorOPFOR": "7F0000", "colorIndependent": "007F00", "colorCivilian": "66007F" };

function InitMap(mapInfos) {
    $(function () {





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


        map.on('click', function (e) {
            clickPosition = e.latlng;
            if ($('#tool-mil').prop('checked')) {
                insertMilSymbol(e.latlng);
            }
            else if ($('#tool-basic').prop('checked')) {
                insertBasicSymbol(e.latlng);
            }
        });

        $('.maptool').on('click', function () {
            if ($('#tool-hand').prop('checked')) {
                $('.leaflet-container').css('cursor', '');
            } else {
                $('.leaflet-container').css('cursor', 'crosshair');
            }
            if ($('#tool-point').prop('checked')) {
                map.dragging.disable();
            } else {
                map.dragging.enable();
            }
        });

        var isPointing = false;



        var markers = {};
        var pointing = {};

        var connection = new signalR.HubConnectionBuilder().withUrl("/MapHub").build();


        function markerMoveEnd(e) {
            var marker = e.target;
            var markerId = marker.options.markerId;
            var markerData = marker.options.markerData;
            markerData.pos = [marker.getLatLng().lat, marker.getLatLng().lng];
            connection.invoke('MoveMarker', markerId, markerData).catch(function (err) {
                return console.error(err.toString());
            });
        }

        connection.on("AddOrUpdateMarker", function (marker) {
            var icon = null;
            var markerId = marker.id;
            var markerData = marker.data;
            var existing = markers[markerId];

            if (markerData.type == 'mil') {
                var symbolConfig = $.extend({ size: 32 }, markerData.config);
                var sym = new ms.Symbol(markerData.symbol, symbolConfig);
                icon = L.icon({
                    iconUrl: sym.asCanvas(window.devicePixelRatio).toDataURL(),
                    iconSize: [sym.getSize().width, sym.getSize().height],
                    iconAnchor: [sym.getAnchor().x, sym.getAnchor().y]
                });
            }
            else if (markerData.type == 'basic') {
                var url = '/img/markers/' + markerData.config.color + '/' + markerData.symbol + '.png';

                if (markerData.config.label && markerData.config.label.length > 0) {
                    var iconHtml = $('<div></div>').append(
                            $('<div></div>')
                            .addClass('text-marker-content')
                            .css('color', '#' + basicColors[markerData.config.color])
                            .text(markerData.config.label)
                            .prepend('<img src="' + url + '" width="32" height="32" />'))
                        .html();

                    icon = new L.DivIcon({
                        className: 'text-marker',
                        html: iconHtml,
                        iconAnchor: [16, 16]
                    });
                }
                else {
                    icon = L.icon({iconUrl: url,iconSize: [32, 32],iconAnchor: [16, 16]});
                }
            }
            if (existing) {
                existing.setIcon(icon);
                existing.setLatLng(markerData.pos);
                existing.options.markerData = markerData;
            }
            else {
                markers[markerId] = L.marker(markerData.pos, { icon: icon, draggable: true, markerId: markerId, markerData: markerData })
                    .addTo(map)
                    .on('click', updateMarkerHandler)
                    .on('dragend', markerMoveEnd);
            }
        });

        connection.on("RemoveMarker", function (marker) {
            var existing = markers[marker.id];
            if (existing) {
                existing.remove();
            }
        });

        var pointingIcon = new L.icon({iconUrl: '/img/pointmap.png',iconSize: [16, 16],iconAnchor: [8, 8]});

        connection.on("PointMap", function (id, pos) {
            console.log("PointMap", id, pos);
            var existing = pointing[id];
            if (existing) {
                existing.setLatLng(pos);
            }
            else {
                pointing[id] = L.marker(pos, { icon: pointingIcon, draggable: false, interactive: false }).addTo(map);
            }
        });
        connection.on("EndPointMap", function (id) {
            var existing = pointing[id];
            if (existing) {
                existing.remove();
                delete pointing[id];
            }
        });

        connection.start().then(function () {
            connection.invoke("Hello", mapHubInfos.matchID, mapHubInfos.roundSideID);
        });

        milsymbolMarkerTool(connection);
        basicsymbolMarkerTool(connection);

        $('select').selectpicker();

        map.on('mousemove', function (e) {
            $('#coordinates').val(toGrid(e.latlng));
            if (isPointing) {
                connection.invoke("PointMap", mapHubInfos.matchID, mapHubInfos.roundSideID, [e.latlng.lat, e.latlng.lng]);
            }
        });
        map.on('mousedown', function (e) {
            if ($('#tool-point').prop('checked')) {
                isPointing = true;
                connection.invoke("PointMap", mapHubInfos.matchID, mapHubInfos.roundSideID, [e.latlng.lat, e.latlng.lng]);
            }
        });

        map.on('mouseup', function (e) {
            if (isPointing) {
                isPointing = false;
                connection.invoke("EndPointMap", mapHubInfos.matchID, mapHubInfos.roundSideID);
            }
        });
        map.on('mouseout', function (e) {
            if (isPointing) {
                isPointing = false;
                connection.invoke("EndPointMap", mapHubInfos.matchID, mapHubInfos.roundSideID);
            }
        });
    });
}


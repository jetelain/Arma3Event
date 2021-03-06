﻿function applySymbolSet() {
    var id = '0003';
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

            $('#icon').append($('<option></option>').attr({ 'data-divider': 'true' }));
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
    var symbol = '100';
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
    addToConfig(config, 'higherFormation');
    addToConfig(config, 'reinforcedReduced');
    return config;
}

function setSymbol(symbol, config) {

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
    $('#higherFormation').val(config.higherFormation || '');
    $('#reinforcedReduced').val(config.reinforcedReduced || '');
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
        $('#basic-dir').val(modalMarkerData.config.dir);
        $('#basic-label').val(modalMarkerData.config.label);
        $('select').selectpicker('render');

        $('#basicsymbol').modal('show');
        $('#basicsymbol-delete').show();
        $('#basicsymbol-update').show();
        $('#basicsymbol-insert').hide();
        $('#basicsymbol-grid').text(toGrid(e.latlng));

    } else if (modalMarkerData.type == 'line') {
        $('#line-color').val(modalMarkerData.config.color);
        $('select').selectpicker('render');
        $('#line').modal('show');
    }

};
function insertMilSymbol(latlng) {
    clickPosition = latlng;
    setSymbol(getSymbol(), {}); // Garde le même symbole, mais réinitialise les annotations
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

        connection.invoke('AddMarker', mapHubInfos.mapId, {
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
        connection.invoke('AddMarker', mapHubInfos.mapId, {
            type: 'basic',
            symbol: $('#basic-type').val(),
            config: { color: $('#basic-color').val(), label: $('#basic-label').val(), dir: $('#basic-dir').val() },
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
        modalMarkerData.config = { color: $('#basic-color').val(), label: $('#basic-label').val(), dir: $('#basic-dir').val() };
        connection.invoke('UpdateMarker', modalMarkerId, modalMarkerData);
        $('#basicsymbol').modal('hide');
    });
}

function lineMarkerTool(connection) {

    $('#line-delete').on('click', function () {
        connection.invoke('RemoveMarker', modalMarkerId);
        $('#line').modal('hide');
    });

    $('#line-update').on('click', function () {
        modalMarkerData.config = { color: $('#line-color').val() };
        connection.invoke('UpdateMarker', modalMarkerId, modalMarkerData);
        $('#line').modal('hide');
    });
}


var currentLine = null;
var lineFirstPoint = null;
var basicColors = { "ColorBlack": "000000", "ColorGrey": "7F7F7F", "ColorRed": "E50000", "ColorBrown": "7F3F00", "ColorOrange": "D86600", "ColorYellow": "D8D800", "ColorKhaki": "7F9966", "ColorGreen": "00CC00", "ColorBlue": "0000FF", "ColorPink": "FF4C66", "ColorWhite": "FFFFFF", "ColorUNKNOWN": "B29900", "colorBLUFOR": "004C99", "colorOPFOR": "7F0000", "colorIndependent": "007F00", "colorCivilian": "66007F" };

function insertLine(latlng, map, append, connection) {
    var point = latlng;

    if (!currentLine) {
        currentLine = L.polyline([point, point], { color: '#'+basicColors[$('#tool-color').val()], weight: 3, interactive: false }).addTo(map);
    } else if (append) {
        var data = currentLine.getLatLngs();
        data[data.length - 1] = point;
        data.push(point);
        currentLine.setLatLngs(data);
    } else {
        var data = currentLine.getLatLngs();
        data[data.length - 1] = point;
        currentLine.remove();
        currentLine = null;
        connection.invoke('AddMarker', mapHubInfos.mapId, {
            type: 'line',
            symbol: 'line',
            config: { color: $('#tool-color').val()},
            pos: data.map(function (e) { return [e.lat, e.lng]; }).flat()
        });
    }
}


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

        var isPointing = false;



        var markers = {};
        var pointing = {};

        var connection = new signalR.HubConnectionBuilder().withUrl("/MapHub").build();


        map.on('click', function (e) {
            clickPosition = e.latlng;
            if (e.originalEvent.target.localName == "div") {
                if ($('#tool-mil').prop('checked')) {
                    insertMilSymbol(e.latlng);
                }
                else if ($('#tool-basic').prop('checked')) {
                    insertBasicSymbol(e.latlng);
                }
                else if ($('#tool-line').prop('checked')) {
                    insertLine(e.latlng, map, e.originalEvent.ctrlKey, connection);
                }
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
            if ($('#tool-line').prop('checked')) {
                $('#tool-color').selectpicker('show');
            } else {
                $('#tool-color').selectpicker('hide');
            }
        });



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
            var canEdit = mapHubInfos.canEdit && marker.mapId.roundSideID == mapHubInfos.mapId.roundSideID;

            if (markerData.type == 'line') {

                var posList = [];
                for (var i = 0; i < markerData.pos.length; i += 2) {
                    posList.push([markerData.pos[i], markerData.pos[i + 1]]);
                }
                var color = '#' + (basicColors[markerData.config.color] || '000000');
                if (existing) {
                    existing.setLatLngs(posList);
                    existing.setStyle({ color: color });
                    existing.options.markerData = markerData;
                } else {
                    var mapMarker = L.polyline(posList, { color: color, weight: 3, interactive: canEdit, markerId: markerId, markerData: markerData }).addTo(map);
                    if (canEdit) {
                        mapMarker.on('click', updateMarkerHandler);
                    }
                    markers[markerId] = mapMarker;
                }

            } else {
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

                    if ((markerData.config.label && markerData.config.label.length > 0) || markerData.config.dir) {
                        var img = $('<img src="' + url + '" width="32" height="32" />');
                        if (markerData.config.dir) {
                            img.css('transform', 'rotate(' + (Number(markerData.config.dir) * 360 / 6400) + 'deg)')
                        }

                        var iconHtml = $('<div></div>').append(
                            $('<div></div>')
                                .addClass('text-marker-content')
                                .css('color', '#' + basicColors[markerData.config.color])
                                .text(markerData.config.label)
                                .prepend(img))
                            .html();

                        icon = new L.DivIcon({
                            className: 'text-marker',
                            html: iconHtml,
                            iconAnchor: [16, 16]
                        });
                    }
                    else {
                        icon = L.icon({ iconUrl: url, iconSize: [32, 32], iconAnchor: [16, 16] });
                    }
                }
                if (existing) {
                    existing.setIcon(icon);
                    existing.setLatLng(markerData.pos);
                    existing.options.markerData = markerData;
                }
                else {
                    var mapMarker =
                        L.marker(markerData.pos, { icon: icon, draggable: canEdit, interactive: canEdit, markerId: markerId, markerData: markerData })
                            .addTo(map);
                    if (canEdit) {
                        mapMarker.on('click', updateMarkerHandler)
                            .on('dragend', markerMoveEnd);
                    }
                    markers[markerId] = mapMarker;
                }
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

        function connectionLost(e) {
            if (e) {
                $('#connectionlost').show();
            }
        }


        connection.start().then(function () {
            connection.invoke("Hello", mapHubInfos.mapId);
        }).catch(connectionLost);

        connection.onclose(connectionLost);

        milsymbolMarkerTool(connection);
        basicsymbolMarkerTool(connection);
        lineMarkerTool(connection);

        $('select').selectpicker();
        $('#tool-color').selectpicker('hide');

        map.on('mousemove', function (e) {
            $('#coordinates').val(toGrid(e.latlng));
            if (isPointing) {
                connection.invoke("PointMap", mapHubInfos.mapId, [e.latlng.lat, e.latlng.lng]);
            }
            if (currentLine) {
                var data = currentLine.getLatLngs();
                data[data.length-1] = [e.latlng.lat, e.latlng.lng];
                currentLine.setLatLngs(data);
            }
        });
        map.on('mousedown', function (e) {
            if ($('#tool-point').prop('checked')) {
                isPointing = true;
                connection.invoke("PointMap", mapHubInfos.mapId, [e.latlng.lat, e.latlng.lng]);
            }
        });

        map.on('mouseup', function (e) {
            if (isPointing) {
                isPointing = false;
                connection.invoke("EndPointMap", mapHubInfos.mapId);
            }
        });
        map.on('mouseout', function (e) {
            if (isPointing) {
                isPointing = false;
                connection.invoke("EndPointMap", mapHubInfos.mapId);
            }
        });
    });
}



var basicColors = { "ColorBlack": "000000", "ColorGrey": "7F7F7F", "ColorRed": "E50000", "ColorBrown": "7F3F00", "ColorOrange": "D86600", "ColorYellow": "D8D800", "ColorKhaki": "7F9966", "ColorGreen": "00CC00", "ColorBlue": "0000FF", "ColorPink": "FF4C66", "ColorWhite": "FFFFFF", "ColorUNKNOWN": "B29900", "colorBLUFOR": "004C99", "colorOPFOR": "7F0000", "colorIndependent": "007F00", "colorCivilian": "66007F" };

function InitMap(mapInfos) {
    $(function () {

        var map = L.map('map', {
            minZoom: mapInfos.minZoom,
            maxZoom: mapInfos.maxZoom,
            crs: mapInfos.CRS,
            zoomControl: false
        });

        L.tileLayer(mapInfos.tilePattern, {
            attribution: mapInfos.attribution,
            tileSize: mapInfos.tileSize
        }).addTo(map);


        L.Control.Watermark =
            L.Control.extend({
            onAdd: function (map) {
                return $('#maplink')[0];
            },

            onRemove: function (map) {
                // Nothing to do here
            }
        });

        L.control.watermark = function (opts) {
            return new L.Control.Watermark(opts);
        }

        L.control.watermark({ position: 'topright' }).addTo(map);

        //map.setView(mapInfos.center, mapInfos.defaultZoom - 1);

        // On devrait également pouvoir faire :
        map.fitBounds([[0, 0],[mapInfos.center[0] * 2, mapInfos.center[1] * 2]]);

        //map.on('click', function () { window.location.href = $('#maplink').attr('href'); });

        var markers = {};

        var connection = new signalR.HubConnectionBuilder().withUrl("/MapHub").build();

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

        connection.start().then(function () {
            connection.invoke("Hello", mapHubInfos.mapId);
        });

    });
}


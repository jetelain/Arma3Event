

function InitMap(mapInfos, data) {
    $(function () {

        var map = L.map('map', {
            minZoom: mapInfos.minZoom,
            maxNativeZoom: mapInfos.maxZoom,
            maxZoom: mapInfos.maxZoom + 5,
            crs: mapInfos.CRS
        });

        L.tileLayer(mapInfos.tilePattern, {
            attribution: mapInfos.attribution,
            tileSize: mapInfos.tileSize,
            maxNativeZoom: mapInfos.maxZoom
        }).addTo(map);

        map.setView(mapInfos.center, mapInfos.defaultZoom);

        L.latlngGraticule().addTo(map);

        L.control.scale({ maxWidth: 200, imperial: false }).addTo(map);

        var playerIcon = L.icon({ iconUrl: '/img/markers/dotBlue.png', iconSize: [20, 20], iconAnchor: [10, 10] });
        var vehicleIcon = L.icon({ iconUrl: '/img/markers/rectBlue.png', iconSize: [30, 30], iconAnchor: [15, 15] });

        data.vehicles.forEach(vehicle => {
            L.marker([vehicle.position.y, vehicle.position.x], { icon: vehicleIcon }).addTo(map).bindPopup('<a href="#backup-vehicle-' + vehicle.vehicleId + '">' + vehicle.name + '</a>');
        });
        data.players.forEach(player => {
            L.marker([player.position.y, player.position.x], { icon: playerIcon }).addTo(map).bindPopup('<a href="#backup-personnel-' + player.steamId + '">' + player.name + '</a>');
        });
    });
}


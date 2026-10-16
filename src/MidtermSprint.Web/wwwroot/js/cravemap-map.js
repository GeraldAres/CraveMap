const maps = new WeakMap();

function createElement(tagName, className, text) {
    const element = document.createElement(tagName);
    element.className = className;
    element.textContent = text;
    return element;
}

export function createMap(element, options, dotnetReference) {
    if (!window.L) {
        throw new Error("Leaflet did not load. Check the local Leaflet script and stylesheet.");
    }

    const map = window.L.map(element, {
        center: [options.latitude, options.longitude],
        zoom: options.zoom,
        scrollWheelZoom: false
    });

    window.L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    const markerLayer = window.L.featureGroup().addTo(map);
    const originMarker = window.L.circleMarker([options.latitude, options.longitude], {
        radius: 8,
        color: "#fffaf3",
        weight: 3,
        fillColor: "#4c7c59",
        fillOpacity: 1
    }).bindTooltip("Your selected area").addTo(map);

    if (options.isPicker) {
        map.on("click", event => {
            originMarker.setLatLng(event.latlng);
            dotnetReference.invokeMethodAsync(
                "OnMapLocationPicked",
                event.latlng.lat,
                event.latlng.lng);
        });
    }

    const state = {
        map,
        markerLayer,
        originMarker,
        dotnetReference,
        selectedMarkerId: null
    };
    maps.set(element, state);
    updateMap(element, options);
    requestAnimationFrame(() => map.invalidateSize());
}

export function updateMap(element, options) {
    const state = maps.get(element);
    if (!state) {
        return;
    }

    state.originMarker.setLatLng([options.latitude, options.longitude]);
    state.markerLayer.clearLayers();
    state.selectedMarkerId = options.selectedMarkerId ?? null;

    for (const marker of options.markers ?? []) {
        const selected = marker.restaurantId === state.selectedMarkerId;
        const pin = window.L.circleMarker([marker.latitude, marker.longitude], {
            radius: selected ? 10 : 8,
            color: "#fffaf3",
            weight: selected ? 4 : 3,
            fillColor: selected ? "#bd3d2c" : "#df533d",
            fillOpacity: 1
        });
        const popup = createElement("div", "cravemap-map-popup", "");
        popup.append(
            createElement("strong", "cravemap-map-popup__title", marker.restaurantName),
            createElement("span", "cravemap-map-popup__item", marker.menuItemName),
            createElement("span", "cravemap-map-popup__price", `₱${Number(marker.price).toLocaleString()}`)
        );
        pin.bindPopup(popup);
        pin.on("click", () => state.dotnetReference.invokeMethodAsync(
            "OnLeafletMarkerSelected",
            marker.restaurantId));
        pin.addTo(state.markerLayer);
    }

    if (options.fitMarkers && state.markerLayer.getLayers().length > 0) {
        const bounds = state.markerLayer.getBounds().extend(state.originMarker.getLatLng());
        state.map.fitBounds(bounds, { padding: [30, 30], maxZoom: 15 });
    } else {
        state.map.setView([options.latitude, options.longitude], options.zoom);
    }

    requestAnimationFrame(() => state.map.invalidateSize());
}

export function getCurrentLocation() {
    return new Promise(resolve => {
        if (!navigator.geolocation) {
            resolve({ isSuccess: false, error: "This browser does not provide location services." });
            return;
        }

        navigator.geolocation.getCurrentPosition(
            position => resolve({
                isSuccess: true,
                latitude: position.coords.latitude,
                longitude: position.coords.longitude
            }),
            error => {
                const messages = {
                    1: "Location permission was denied. You can choose a demo area or set a location on the map instead.",
                    2: "Your location is unavailable right now. Choose a demo area or set a location on the map instead.",
                    3: "Getting your location took too long. Try again or set a location on the map."
                };
                resolve({
                    isSuccess: false,
                    error: messages[error.code] ?? "We couldn't get your location. Choose a demo area or use the map."
                });
            },
            { enableHighAccuracy: false, timeout: 10000, maximumAge: 60000 }
        );
    });
}

export function destroyMap(element) {
    const state = maps.get(element);
    if (!state) {
        return;
    }

    state.map.remove();
    maps.delete(element);
}

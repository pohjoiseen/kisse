// Index page, the one with big map
// Htmx doesn't help there much so the logic is all here

// create main map
const mapEl = document.getElementById('homepage-map');
const map = L.map(mapEl);
map.attributionControl.setPrefix(false);
L.control.scale({metric: true}).addTo(map);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: 'Map &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a>',
    minZoom: window.MIN_ZOOM,
    maxZoom: window.MAX_ZOOM,
    detectRetina: true
}).addTo(map);

// try to get stored map position, use only if not too old
const LS_POSITION_KEY = 'kisse.indexMap.position';
const MAX_POSITION_AGE = 15 * 60 * 1000;  // 15 min
let savedPosition = localStorage.getItem(LS_POSITION_KEY);
if (savedPosition) {
    try {
        savedPosition = JSON.parse(savedPosition);
        if (Date.now() - savedPosition.timestamp < MAX_POSITION_AGE) {
            map.setView([savedPosition.lat, savedPosition.lng], savedPosition.zoom);
        } else {
            savedPosition = null;
        }
    } catch (e) {
        // ignore errors
        savedPosition = null;
    }
}

// if no usable saved position found, center at default location (configurable in appsettings.json)
// and try to geolocate
if (!savedPosition) {
    map.setView([window.DEFAULT_LAT, window.DEFAULT_LNG], window.DEFAULT_ZOOM);
    map.locate({
        watch: false,
        setView: true,
        maximumAge: 30 * 1000,
        enableHighAccuracy: true
    });
}

// save position on every move
const onMapMove = () => {
    window.localStorage.setItem(LS_POSITION_KEY, JSON.stringify({ 
        lat: map.getCenter().lat, 
        lng: map.getCenter().lng,
        zoom: map.getZoom(),
        timestamp: Date.now()
    }));
};
map.on('moveend', onMapMove);
map.on('zoomend', onMapMove);

// measure emojis actual size
const catEmoji = '🐱';
const measuringDiv = document.createElement('div');
measuringDiv.style.visibility = 'hidden';
measuringDiv.style.width = 'fit-content';
measuringDiv.style.fontSize = '15px';
document.body.appendChild(measuringDiv);
measuringDiv.textContent = catEmoji;
const catEmojiWidth = measuringDiv.clientWidth, catEmojiHeight = measuringDiv.clientHeight;
const observationEmoji = '👀';
measuringDiv.textContent = observationEmoji;
const observationEmojiWidth = measuringDiv.clientWidth, observationEmojiHeight = measuringDiv.clientHeight;
const selfEmoji = '🔴';
measuringDiv.textContent = selfEmoji;
const selfEmojiWidth = measuringDiv.clientWidth, selfEmojiHeight = measuringDiv.clientHeight;
measuringDiv.remove();

// add observation markers
// just dump everything we have, should not ever be that many so don't bother with dynamically loading,
// and we cluster them with markercluser Leaflet plugin anyway
const clusteredMarkers = L.markerClusterGroup();
for (const [id, [lat, lng]] of Object.entries(window.CATS)) {
    const marker = L.marker(new L.LatLng(lat, lng), {
        icon: /* thumb
        ? L.icon({
            iconUrl: thumb,
            iconSize: [24, 24],
            iconAnchor: [12, 12],
            className: 'map-thumb-icon'
        })
        : */ L.divIcon({
            iconSize: [catEmojiWidth, catEmojiHeight],
            iconAnchor: [catEmojiWidth / 2, catEmojiHeight / 2],
            html: catEmoji,
            className: 'map-emoji-icon'
        })
    });
    marker.id = id;
    marker.bindPopup(`<div class="cat-popup-loading" data-id="${id}">Loading...</div>`, {maxWidth: 360});
    marker.getPopup().on('contentupdate', onCatPopup);
    clusteredMarkers.addLayer(marker);
}
for (const [id, [lat, lng]] of Object.entries(window.OBSERVATIONS)) {
    const marker = L.marker(new L.LatLng(lat, lng), {
        icon: /* thumb
        ? L.icon({
            iconUrl: thumb,
            iconSize: [24, 24],
            iconAnchor: [12, 12],
            className: 'map-thumb-icon'
        })
        : */ L.divIcon({
            iconSize: [observationEmojiWidth, observationEmojiHeight],
            iconAnchor: [observationEmojiWidth / 2, observationEmojiHeight / 2],
            html: observationEmoji,
            className: 'map-emoji-icon'
        })
    });
    marker.id = id;
    marker.bindPopup(`<div class="observation-popup-loading" data-id="${id}">Loading...</div>`, {maxWidth: 360});
    marker.getPopup().on('contentupdate', onObservationPopup);
    clusteredMarkers.addLayer(marker);
}
map.addLayer(clusteredMarkers);

// load more data when popup is opened
async function onCatPopup(e) {
    // look for placeholder "Loading..." div in the popup
    const loadingEl = e.target.getElement().querySelector('.cat-popup-loading');
    if (loadingEl) {
        const id = loadingEl.dataset.id;
        try {
            const response = await fetch(`/Cat/ViewPopup/${id}`, {
                credentials: 'include',
                headers: {'HX-Request': 'true'}  // emulate htmx request to ensure layout is not rendered
            });
            if (!response.ok) {
                throw new Error(`${response.status} ${response.statusText}`);
            }
            const html = await response.text();
            // replace placeholder "Loading..." with loaded HTML, don't forget to apply htmx to it
            loadingEl.outerHTML = html;
            htmx.process(e.target.getElement());
        } catch (e) {
            loadingEl.textContent = e.message;
            console.error(e);
        }
    }
}


async function onObservationPopup(e) {
    // look for placeholder "Loading..." div in the popup
    const loadingEl = e.target.getElement().querySelector('.observation-popup-loading');
    if (loadingEl) {
        const id = loadingEl.dataset.id;
        try {
            const response = await fetch(`/Observation/ViewPopup/${id}`, {
                credentials: 'include',
                headers: {'HX-Request': 'true'}  // emulate htmx request to ensure layout is not rendered
            });
            if (!response.ok) {
                throw new Error(`${response.status} ${response.statusText}`);
            }
            const html = await response.text();
            // replace placeholder "Loading..." with loaded HTML, don't forget to apply htmx to it
            loadingEl.outerHTML = html;
            htmx.process(e.target.getElement());
        } catch (e) {
            loadingEl.textContent = e.message;
            console.error(e);
        }
    }
}

// add a special marker for user's position, update on watchPosition()
const selfMarker = L.marker(new L.LatLng(0, 0), {
    icon: L.divIcon({
        iconSize: [selfEmojiWidth, selfEmojiHeight],
        iconAnchor: [selfEmojiWidth / 2, selfEmojiHeight / 2],
        html: selfEmoji,
        className: 'map-emoji-icon'
    })
});
selfMarker.bindPopup("You are here");
if ('geolocation' in window.navigator) {
    window.navigator.geolocation.watchPosition((position) => {
        selfMarker.setLatLng(L.latLng(position.coords.latitude, position.coords.longitude));
        if (!map.hasLayer(selfMarker)) {
            map.addLayer(selfMarker);
        }
    }, () => {
        if (map.hasLayer(selfMarker)) {
            map.removeLayer(selfMarker);
        }
    }, {
        maximumAge: 30 * 1000,
        enableHighAccuracy: true,
    });
}
// JS code that needs to run on all pages goes here

// Make sure that all textareas with appropriate class autosize to their content
htmx.onLoad((content) => content.querySelectorAll('.textarea-autosize').forEach(el => {
    el.style.height = '5px';
    el.style.height = el.scrollHeight + 15 + 'px';
}));

// Initialize Leaflet for location selector (_LocationSelector.cshtml)
// Only support one such per page
// Map element is not reloaded (hx-preserve) so this only needs to be done once 
const mapElem = document.getElementById('location-selector');
if (mapElem) {
    const latElem = document.getElementById('location-selector-lat');
    const lngElem = document.getElementById('location-selector-lng');
    const zoomElem = document.getElementById('location-selector-zoom');
    const map = L.map(mapElem, {
        gestureHandling: true  // make sure map is easy to operate with touch
    });
    map.attributionControl.setPrefix(false);
    L.control.scale({metric: true}).addTo(map);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: 'Map &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a>',
        minZoom: window.MIN_ZOOM,
        maxZoom: window.MAX_ZOOM,
        detectRetina: true
    }).addTo(map);
    map.setView([parseFloat(latElem.value), parseFloat(lngElem.value)], parseInt(zoomElem.value, 10));
    // store reference in window
    window.locationSelectorMap = map;

    // handle map move/zoom by updating values in hidden inputs
    const onMapMove = () => {
        // inputs can be reloaded by htmx so must look them up again
        const latElem = document.getElementById('location-selector-lat');
        const lngElem = document.getElementById('location-selector-lng');
        const zoomElem = document.getElementById('location-selector-zoom');
        latElem.value = map.getCenter().lat;
        lngElem.value = map.getCenter().lng;
        zoomElem.value = map.getZoom();
    }
    map.on('moveend', onMapMove);
    map.on('zoomend', onMapMove);
}
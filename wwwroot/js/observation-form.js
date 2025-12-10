// if opened with location set to default coordinates, start by using browser geolocation
if (Math.abs(parseFloat(document.getElementById('location-selector-lat').value) - window.DEFAULT_LAT) < 0.00001 &&
    Math.abs(parseFloat(document.getElementById('location-selector-lng').value) - window.DEFAULT_LNG) < 0.00001 &&
    'geolocation' in window.navigator) {
    window.navigator.geolocation.getCurrentPosition((position) => {
        window.locationSelectorMap.panTo(L.latLng(position.coords.latitude, position.coords.longitude));
    });
}

// one a photo or photos have been uploaded, set location to the last photo with non-zero coords
document.addEventListener('htmx:afterSwap', (e) => {
    if (e.detail.target.id === 'photos') {
        const thumbnails = [...e.detail.target.querySelectorAll('.photo-thumbnail')].toReversed();        
        for (const thumbnail of thumbnails) {
            if (thumbnail.dataset.lat && thumbnail.dataset.lng &&
                thumbnail.dataset.lat !== '0' && thumbnail.dataset.lng !== '0') {
                window.locationSelectorMap.panTo(L.latLng(parseFloat(thumbnail.dataset.lat), parseFloat(thumbnail.dataset.lng)));
                break;
            }
        }
    }
});

// allow uploading files by pasting
window.addEventListener('paste', e => {
    if (e.clipboardData?.files?.length > 0) {
        document.getElementById('upload-input').files = e.clipboardData.files;
        htmx.trigger('#upload-input', 'input', {value: 'pasted'});
    }
});

let socket;

function connect() {
    socket = new WebSocket('ws://localhost:27654');

    socket.onopen = () => console.log('[YTMusic] Połączono z WinUI!');
    socket.onerror = () => console.log('[YTMusic] Brak połączenia z WinUI, próba za 5s...');
    socket.onclose = () => {
        console.log('[YTMusic] Połączenie zerwane, próba za 5s...');
        setTimeout(connect, 5000); 
    };
}

connect();

setInterval(() => {
    if (!socket || socket.readyState !== WebSocket.OPEN) return;

    const titleElem = document.querySelector('.ytmusic-player-bar .title');
    const artistElem = document.querySelector('.ytmusic-player-bar .byline');
    const video = document.querySelector('video');
    const progressBar = document.querySelector('#progress-bar');
    
    // NAPRAWA OKŁADKI: Wyciągamy obrazek konkretnie z miniaturki piosenki (thumbnail), a nie avatara wykonawcy
    const imgElem = document.querySelector('ytmusic-player-bar .thumbnail-image-wrapper img');

    if (titleElem && artistElem && video && progressBar) {
        const title = titleElem.innerText;
        const artist = artistElem.innerText;
        const coverUrl = imgElem ? imgElem.src : "";
        
        let videoId = null;

        const player = document.getElementById('movie_player');
        if (player && typeof player.getVideoData === 'function') {
            const data = player.getVideoData();
            if (data && data.video_id) {
                videoId = data.video_id;
            }
        }

        const cleanTrackUrl = videoId ? `https://music.youtube.com/watch?v=${videoId}` : "";

        const currentSec = parseInt(progressBar.getAttribute('aria-valuenow')) || 0;
        const totalSec = parseInt(progressBar.getAttribute('aria-valuemax')) || 0;
        const isPaused = video.paused; 

        if (title && totalSec > 0) {
            const data = {
                title: title,
                artist: artist,
                currentSec: currentSec,
                totalSec: totalSec,
                coverUrl: coverUrl,
                trackUrl: cleanTrackUrl,
                isPaused: isPaused
            };

            socket.send(JSON.stringify(data));
        }
    }
}, 1000);
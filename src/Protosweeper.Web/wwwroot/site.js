let url;
let difficulty;
let websocket;
let gameStarted = false;

function connect(url) {
    websocket = new WebSocket(url);
    
    websocket.onmessage = event => {
        if (!event.data) {
            console.log("No data received");
            return;
        }
        
        const data = JSON.parse(event.data);
        console.log(data);
        
        if (data.error) {
            console.log("Got error");
            console.log(data.error);
            return;
        }
        
        if (data.type === "cell") {
            const { x, y, count } = data;
            const id = `cell-${x}-${y}`;
            const cell = document.getElementById(id);
            if (cell) {
                cell.innerText = count > 0 ? `${count}` : " ";
                cell.className = `cell clicked count-${count}`;
            } else {
                console.log(`Couldn't find cell ${data}`);
            }
        } else if (data.type === "flag") {
            const {x, y} = data;
            const id = `cell-${x}-${y}`;
            const cell = document.getElementById(id);
            if (cell) {
                cell.innerText = "🚩";
                cell.className = "cell flag";
            }
        } else if (data.type === "unflag") {
            const { x, y } = data;
            const id = `cell-${x}-${y}`;
            const cell = document.getElementById(id);
            if (cell) {
                cell.innerText = " ";
                cell.className = "cell";
            }
        } else if (data.type === "mine") {
            const { x, y } = data;
            const id = `cell-${x}-${y}`;
            const cell = document.getElementById(id);
            if (cell) {
                cell.innerText = "💥";
                cell.className = "cell clicked mine";
            }
        }
    }

    websocket.onerror = (event) => {
        console.log("Connection error");
    }

    websocket.onclose = (event) => {
        console.log("Reconnecting...");
        setTimeout(() => {
            connect(url);
        }, 10000);
    }
}

const cellPattern = /cell-(\d+)-(\d+)/;

function clickCell(cellId) {
    const cell = document.getElementById(cellId);
    return (event) => {
        console.log(event);
        console.log(cellId);
        
        const groups = cellPattern.exec(cellId);
        const x = parseInt(groups[1]);
        const y = parseInt(groups[2]);
        
        if (!gameStarted) {
            const wsUrl = `${url}?difficulty=${difficulty}&x=${x}&y=${y}`;
            console.log(wsUrl);
            connect(wsUrl);
            gameStarted = true;
            return;
        }
        
        let button = "left";
        if (event.button === 2) {
            button = "right";
        } else if (event.button === 1) {
            button = "middle";
        }
        
        const message = {
            type: "click",
            button,
            x,
            y,
        };
        websocket.send(JSON.stringify(message));
    }
    
}

function initialise(_url, _difficulty) {
    difficulty = _difficulty;
    url = _url;
    
    const cells = document.getElementsByClassName("cell");
    for (const cell of cells) {
        cell.addEventListener("contextmenu", e => e.preventDefault())
        cell.onmouseup = clickCell(cell.id);
    }
}

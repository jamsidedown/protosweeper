let restUrl;
let wsUrl;
let difficulty;
let websocket;
let gameStarted = false;
let ended = false;

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
        } else if (data.type === "progress") {
            const { flagged } = data;
            const progress = document.getElementById("progress");
            progress.innerText = flagged;
            progress.hidden = false;
        } else if (data.type === "win") {
            const result = document.getElementById("result");
            result.innerText = "You win!";
            result.hidden = false;
            ended = true;
        } else if (data.type === "lose") {
            const result = document.getElementById("result");
            result.innerText = "You lose!";
            result.hidden = false;
            ended = true;
        }
    }

    websocket.onerror = (event) => {
        console.log("Connection error");
    }

    websocket.onclose = (event) => {
        if (ended) {
            return;
        }
        console.log("Reconnecting...");
        setTimeout(() => {
            connect(url);
        }, 10000);
    }
}

const cellPattern = /cell-(\d+)-(\d+)/;

function clickCell(cellId) {
    return async (event) => {
        console.log(event);
        console.log(cellId);
        
        if (ended) {
            return;
        }
        
        const groups = cellPattern.exec(cellId);
        const x = parseInt(groups[1]);
        const y = parseInt(groups[2]);
        
        if (!gameStarted) {
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            
            const postUrl = `${restUrl}?difficulty=${difficulty}&x=${x}&y=${y}`;
            console.log(`POST url: ${postUrl}`);
            const response = await fetch(postUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token,
                },
                body: "{}",
            })

            if (!response.ok) {
                throw new Error(`HTTP error: ${response.status}`);
            }

            const data = await response.json();
            console.log(data);
            
            const websocketUrl = `${wsUrl}/${data.id}`;
            console.log(websocketUrl);
            connect(websocketUrl);
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

function initialise(_restUrl, _wsUrl, _difficulty) {
    difficulty = _difficulty;
    restUrl = _restUrl;
    wsUrl = _wsUrl;
    
    const cells = document.getElementsByClassName("cell");
    for (const cell of cells) {
        cell.addEventListener("contextmenu", e => e.preventDefault())
        cell.onmouseup = clickCell(cell.id);
    }
}

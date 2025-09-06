let websocket;

function connect(url) {
    websocket = new WebSocket(url);
    
    websocket.onmessage = event => {
        if (!event.data) {
            console.log("No data received");
            return;
        }
        
        const data = JSON.parse(event.data);
        
        if (data.error) {
            console.log("Got error");
            console.log(data.error);
            return;
        }
        
        if (data.cell) {
            const x = data.cell.x;
            const y = data.cell.y;
            const value = data.cell.value;
            const id = `cell-${x}-${y}`;
            const cellClass = data.cell.class;
            const cell = document.getElementById(id);
            if (cell) {
                cell.innerText = value;
                cell.className = cellClass;
            } else {
                console.log(`Couldn't find cell ${data.cell}`);
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
        
        let button = "left";
        if (event.button === 2) {
            button = "right";
        } else if (event.button === 1) {
            button = "middle";
        }
        
        const message = {
            button,
            x,
            y,
        };
        websocket.send(JSON.stringify(message));
        cell.classList.add("clicked");
    }
    
}

function initialiseCells() {
    const cells = document.getElementsByClassName("cell");
    for (const cell of cells) {
        cell.addEventListener("contextmenu", e => e.preventDefault())
        cell.onmouseup = clickCell(cell.id);
    }
}

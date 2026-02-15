const Minesweeper = (function () {
    let rows = 9, cols = 9, mines = 10;
    let gameOver = false;

    const container = () => document.getElementById("boardContainer");
    const statusMsg = () => document.getElementById("statusMsg");
    const minesLeftEl = () => document.getElementById("minesLeft");
    const timerEl = () => document.getElementById("timer");
    const serverTimeEl = () => document.getElementById("serverTime");

    let timerId = null;
    let startTime = null;

    function startTimer() {
        startTime = Date.now();
        stopTimer();
        timerId = setInterval(() => {
            const seconds = Math.floor((Date.now() - startTime) / 1000);
            timerEl().textContent = seconds;
        }, 250);
    }

    function stopTimer() {
        if (timerId) clearInterval(timerId);
        timerId = null;
    }

    function buildEmptyBoardDom(rCount, cCount) {
        const cont = container();
        cont.innerHTML = "";
        for (let r = 0; r < rCount; r++) {
            const rowDiv = document.createElement("div");
            rowDiv.className = "ms-row";
            for (let c = 0; c < cCount; c++) {
                const cell = document.createElement("div");
                cell.id = `cell-${r}-${c}`;
                cell.className = "ms-cell";
                cell.dataset.r = r;
                cell.dataset.c = c;

                cell.addEventListener("click", onCellClick);
                cell.addEventListener("contextmenu", onCellRightClick);

                rowDiv.appendChild(cell);
            }
            cont.appendChild(rowDiv);
        }
    }

    function applyUpdate(u) {
        const el = document.getElementById(u.id);
        if (!el) return;

        // reset base class
        el.className = "ms-cell";

        if (u.revealed) el.classList.add("revealed");
        if (u.flagged) el.classList.add("flagged");
        if (u.revealed && u.mine) el.classList.add("mine");

        el.textContent = u.text || "";
    }

    function applyResponse(resp) {
        statusMsg().textContent = resp.statusMsg;
        minesLeftEl().textContent = resp.flagsLeft;

        if (Array.isArray(resp.updates)) {
            resp.updates.forEach(applyUpdate);
        }

        if (resp.isGameOver || resp.isWin) {
            gameOver = true;
            stopTimer();
        }
    }

    function postForm(url, dataObj) {
        const body = new URLSearchParams();
        Object.keys(dataObj).forEach(k => body.append(k, dataObj[k]));
        return fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: body.toString()
        }).then(r => {
            if (!r.ok) throw new Error("Request failed");
            return r.json();
        });
    }

    function onCellClick(e) {
        if (gameOver) return;

        const r = parseInt(this.dataset.r, 10);
        const c = parseInt(this.dataset.c, 10);

        postForm("/User/Reveal", { r, c })
            .then(resp => {
                // start timer on first successful reveal (server places mines on first reveal)
                if (!timerId) startTimer();
                applyResponse(resp);
            })
            .catch(err => console.error(err));
    }

    function onCellRightClick(e) {
        e.preventDefault();
        if (gameOver) return;

        const r = parseInt(this.dataset.r, 10);
        const c = parseInt(this.dataset.c, 10);

        postForm("/User/ToggleFlag", { r, c })
            .then(applyResponse)
            .catch(err => console.error(err));
    }

    function updateTimestamp() {
        fetch("/User/Timestamp")
            .then(r => r.text())
            .then(t => {
                if (serverTimeEl()) serverTimeEl().textContent = t;
            })
            .catch(() => { });
    }

    function init(r, c, m) {
        rows = r; cols = c; mines = m;

        gameOver = false;
        statusMsg().textContent = "Game in progress";
        timerEl().textContent = "0";
        stopTimer();

        // create server game + render empty board immediately
        postForm("/User/NewGame", { rows, cols, mines })
            .then(resp => {
                buildEmptyBoardDom(resp.rows, resp.cols);
                minesLeftEl().textContent = resp.flagsLeft;
                statusMsg().textContent = resp.statusMsg;
            })
            .catch(err => console.error(err));

        updateTimestamp();
        setInterval(updateTimestamp, 1000);
    }

    function reset() {
        init(rows, cols, mines);
    }

    return { init, reset };
})();

window.Minesweeper = Minesweeper;
const Minesweeper = (function () {
    let rows = 9, cols = 9, mines = 10;
    let gameOver = false;

    const container = () => document.getElementById("boardContainer");
    const statusMsg = () => document.getElementById("statusMsg");
    const minesLeftEl = () => document.getElementById("minesLeft");
    const timerEl = () => document.getElementById("timer");
    const serverTimeEl = () => document.getElementById("serverTime");
    const savedGamesStatusEl = () => document.getElementById("savedGamesStatus");
    const savedGamesTableEl = () => document.getElementById("savedGamesTable");

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

    function toUpdateFromCell(cell) {
        let text = "";
        if (cell.Revealed) {
            if (cell.Mine) text = "💣";
            else if (cell.NeighborMines > 0) text = String(cell.NeighborMines);
        } else if (cell.Flagged) {
            text = "🚩";
        }

        return {
            id: `cell-${cell.R}-${cell.C}`,
            revealed: cell.Revealed,
            flagged: cell.Flagged,
            mine: cell.Mine,
            text: text
        };
    }

    function renderFromGame(game) {
        rows = game.Rows;
        cols = game.Cols;
        mines = game.Mines;
        gameOver = game.IsGameOver || game.IsWin;

        document.getElementById("rowsInput").value = rows;
        document.getElementById("colsInput").value = cols;
        document.getElementById("minesInput").value = mines;

        buildEmptyBoardDom(rows, cols);
        for (let r = 0; r < game.Board.length; r++) {
            for (let c = 0; c < game.Board[r].length; c++) {
                applyUpdate(toUpdateFromCell(game.Board[r][c]));
            }
        }

        statusMsg().textContent = game.StatusMsg;
        minesLeftEl().textContent = game.FlagsLeft;
        timerEl().textContent = "0";
        stopTimer();
        if (!game.FirstClick && !gameOver) startTimer();
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

    function loadSavedGames() {
        const statusEl = savedGamesStatusEl();
        const tableEl = savedGamesTableEl();
        if (statusEl) statusEl.textContent = "Loading saved games...";

        fetch("/api/showSavedGames")
            .then(r => {
                if (!r.ok) throw new Error("Failed to load saved games");
                return r.json();
            })
            .then(list => renderSavedGames(list))
            .catch(err => {
                if (statusEl) statusEl.textContent = err.message;
                if (tableEl) tableEl.classList.add("d-none");
            });
    }

    function renderSavedGames(list) {
        const statusEl = savedGamesStatusEl();
        const tableEl = savedGamesTableEl();
        const tbody = tableEl ? tableEl.querySelector("tbody") : null;

        if (!tableEl || !tbody) return;
        tbody.innerHTML = "";

        if (!Array.isArray(list) || list.length === 0) {
            if (statusEl) statusEl.textContent = "No saved games found.";
            tableEl.classList.add("d-none");
            return;
        }

        if (statusEl) statusEl.textContent = `Saved games: ${list.length}`;
        tableEl.classList.remove("d-none");

        list.forEach(g => {
            const tr = document.createElement("tr");
            const dateText = g.DateSaved ? new Date(g.DateSaved).toLocaleString() : "--";

            tr.innerHTML = `
                <td>${g.Id}</td>
                <td>${g.UserId}</td>
                <td>${dateText}</td>
                <td>
                    <button class="btn btn-sm btn-outline-primary me-2" data-action="load" data-id="${g.Id}">Load</button>
                    <button class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${g.Id}">Delete</button>
                </td>
            `;

            tbody.appendChild(tr);
        });

        tbody.querySelectorAll("button[data-action='load']").forEach(btn => {
            btn.addEventListener("click", function () {
                const id = parseInt(this.dataset.id, 10);
                loadSavedGame(id);
            });
        });

        tbody.querySelectorAll("button[data-action='delete']").forEach(btn => {
            btn.addEventListener("click", function () {
                const id = parseInt(this.dataset.id, 10);
                deleteSavedGame(id);
            });
        });
    }

    function loadSavedGame(id) {
        postForm("/User/LoadSavedGame", { id })
            .then(resp => {
                if (!resp.game) throw new Error("Invalid saved game response");
                renderFromGame(resp.game);
            })
            .catch(err => {
                if (savedGamesStatusEl()) savedGamesStatusEl().textContent = err.message;
            });
    }

    function deleteSavedGame(id) {
        fetch(`/api/deleteOneGame/${id}`, { method: "DELETE" })
            .then(r => {
                if (!r.ok) throw new Error("Failed to delete saved game");
                return r.json();
            })
            .then(() => loadSavedGames())
            .catch(err => {
                if (savedGamesStatusEl()) savedGamesStatusEl().textContent = err.message;
            });
    }

    function saveGame() {
        postForm("/User/SaveGame", {})
            .then(resp => {
                statusMsg().textContent = `Game saved (Id ${resp.id}).`;
                loadSavedGames();
            })
            .catch(err => {
                statusMsg().textContent = "Failed to save game.";
                console.error(err);
            });
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

    return { init, reset, saveGame, loadSavedGames };
})();

window.Minesweeper = Minesweeper;

/* Minimal Minesweeper game logic for MVC view integration
   Provides: Minesweeper.init(rows, cols, mines), reset()
   Renders board into #boardContainer and updates #statusMsg, #minesLeft, #timer
*/

const Minesweeper = (function () {
    let rows = 9, cols = 9, mines = 10;
    let board = []; // 2D array of cells
    let revealedCount = 0;
    let flagsLeft = 0;
    let timerId = null;
    let startTime = null;
    let firstClick = true;
    const container = document.getElementById('boardContainer');

    function Cell(r, c) {
        this.r = r; this.c = c;
        this.mine = false;
        this.revealed = false;
        this.flagged = false;
        this.neighborMines = 0;
    }

    function createEmptyBoard() {
        board = [];
        for (let r = 0; r < rows; r++) {
            const row = [];
            for (let c = 0; c < cols; c++) row.push(new Cell(r, c));
            board.push(row);
        }
        revealedCount = 0;
    }

    function placeMinesAvoiding(firstR, firstC) {
        let toPlace = mines;
        const total = rows * cols;
        const forbidden = new Set();
        forbidden.add(firstR + ',' + firstC);
        // also forbid neighbors so first click never dies
        for (let dr = -1; dr <= 1; dr++) for (let dc = -1; dc <= 1; dc++) {
            const nr = firstR + dr, nc = firstC + dc;
            if (nr >= 0 && nr < rows && nc >= 0 && nc < cols) forbidden.add(nr + ',' + nc);
        }

        while (toPlace > 0) {
            const idx = Math.floor(Math.random() * total);
            const r = Math.floor(idx / cols);
            const c = idx % cols;
            const key = r + ',' + c;
            if (forbidden.has(key)) continue;
            const cell = board[r][c];
            if (!cell.mine) {
                cell.mine = true; toPlace--;
            }
        }
    }

    function computeNeighbors() {
        for (let r = 0; r < rows; r++) for (let c = 0; c < cols; c++) {
            const cell = board[r][c];
            let count = 0;
            for (let dr = -1; dr <= 1; dr++) for (let dc = -1; dc <= 1; dc++) {
                if (dr === 0 && dc === 0) continue;
                const nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols) if (board[nr][nc].mine) count++;
            }
            cell.neighborMines = count;
        }
    }

    function renderBoard() {
        container.innerHTML = '';
        for (let r = 0; r < rows; r++) {
            const rowDiv = document.createElement('div');
            rowDiv.className = 'ms-row';
            for (let c = 0; c < cols; c++) {
                const cell = board[r][c];
                const btn = document.createElement('div');
                btn.className = 'ms-cell';
                btn.dataset.r = r; btn.dataset.c = c;
                if (cell.revealed) {
                    btn.classList.add('revealed');
                    if (cell.mine) {
                        btn.classList.add('mine');
                        btn.textContent = '💣';
                    } else if (cell.neighborMines > 0) {
                        btn.textContent = cell.neighborMines;
                    }
                } else if (cell.flagged) {
                    btn.classList.add('flagged');
                    btn.textContent = '🚩';
                }

                btn.addEventListener('click', onCellClick);
                btn.addEventListener('contextmenu', onCellRightClick);
                rowDiv.appendChild(btn);
            }
            container.appendChild(rowDiv);
        }
        document.getElementById('minesLeft').textContent = flagsLeft;
    }

    function onCellClick(e) {
        const r = parseInt(this.dataset.r, 10);
        const c = parseInt(this.dataset.c, 10);
        if (gameOver()) return;
        if (board[r][c].flagged || board[r][c].revealed) return;

        if (firstClick) {
            placeMinesAvoiding(r, c);
            computeNeighbors();
            startTimer();
            firstClick = false;
        }

        revealCell(r, c);
        checkWin();
        renderBoard();
    }

    function onCellRightClick(e) {
        e.preventDefault();
        const r = parseInt(this.dataset.r, 10);
        const c = parseInt(this.dataset.c, 10);
        if (gameOver()) return;
        const cell = board[r][c];
        if (cell.revealed) return;
        cell.flagged = !cell.flagged;
        flagsLeft += cell.flagged ? -1 : 1;
        document.getElementById('minesLeft').textContent = flagsLeft;
        renderBoard();
    }

    function revealCell(r, c) {
        const cell = board[r][c];
        if (cell.revealed || cell.flagged) return;
        cell.revealed = true;
        revealedCount++;
        if (cell.mine) {
            // reveal all mines and end game
            for (let i = 0; i < rows; i++) for (let j = 0; j < cols; j++) if (board[i][j].mine) board[i][j].revealed = true;
            stopTimer();
            document.getElementById('statusMsg').textContent = 'Game Over! You hit a mine.';
            return;
        }
        if (cell.neighborMines === 0) {
            // flood fill
            for (let dr = -1; dr <= 1; dr++) for (let dc = -1; dc <= 1; dc++) {
                const nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols) {
                    if (!board[nr][nc].revealed) revealCell(nr, nc);
                }
            }
        }
    }

    function checkWin() {
        const totalSafe = rows * cols - mines;
        if (revealedCount >= totalSafe) {
            stopTimer();
            document.getElementById('statusMsg').textContent = 'You Win!';
            // reveal all mines as flagged
            for (let r = 0; r < rows; r++) for (let c = 0; c < cols; c++) if (board[r][c].mine) board[r][c].flagged = true;
            renderBoard();
        }
    }

    function startTimer() {
        startTime = Date.now();
        timerId = setInterval(() => {
            const seconds = Math.floor((Date.now() - startTime) / 1000);
            document.getElementById('timer').textContent = seconds;
        }, 250);
    }

    function stopTimer() {
        if (timerId) clearInterval(timerId);
        timerId = null;
    }

    function gameOver() {
        // game over when timerId is null and not firstClick? But if user hasn't started, timerId null. Use status text.
        const st = document.getElementById('statusMsg').textContent;
        return st.startsWith('Game Over') || st.startsWith('You Win');
    }

    function init(r, c, m) {
        rows = Math.max(5, Math.min(24, parseInt(r, 10) || 9));
        cols = Math.max(5, Math.min(30, parseInt(c, 10) || 9));
        mines = Math.max(1, Math.min(rows * cols - 1, parseInt(m, 10) || 10));
        flagsLeft = mines;
        firstClick = true;
        document.getElementById('statusMsg').textContent = 'Game in progress';
        document.getElementById('minesLeft').textContent = flagsLeft;
        document.getElementById('timer').textContent = '0';
        stopTimer();
        createEmptyBoard();
        renderBoard();
    }

    function reset() {
        stopTimer();
        init(rows, cols, mines);
    }

    return { init, reset };
})();

// expose for debugging
window.Minesweeper = Minesweeper;

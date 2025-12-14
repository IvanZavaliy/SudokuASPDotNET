import { SudokuEngine } from './game-engine.js';

export class SudokuUI {
    constructor() {
        this.boardElement = document.getElementById('sudoku-board');
        this.timerElement = document.getElementById('game-time');
        this.resultModal = document.getElementById('result-screen');

        this.engine = null;
        this.timer = null;
        this.seconds = 0;
        this.selectedRow = -1;
        this.selectedCol = -1;
        this.isPaused = false;
    }

    async init(level = 'Easy') {
        try {
            this.boardElement.innerHTML = '<div class="loader">Loading...</div>';
            const response = await fetch(`/api/game/new/${level}`);
            if (!response.ok) throw new Error('Failed to load game');
            const gameDto = await response.json();

            this.engine = new SudokuEngine(gameDto);
            this.renderBoard();
            this.startTimer();
            this.setupInputs();
            this.selectCell(4, 4);
        } catch (error) {
            console.error(error);
            this.boardElement.innerHTML = '<div class="error">Error. Try reloading.</div>';
        }
    }

    renderBoard() {
        this.boardElement.innerHTML = '';
        this.boardElement.removeAttribute('style');

        for (let r = 0; r < 9; r++) {
            for (let c = 0; c < 9; c++) {
                const cell = document.createElement('div');
                cell.classList.add('main-grid-cell');
                cell.dataset.row = r;
                cell.dataset.col = c;

                const val = this.engine.getValue(r, c);
                if (val !== 0) {
                    cell.innerText = val;
                    if (this.engine.isImmutable(r, c)) {
                        cell.classList.add('filled');
                    } else {
                        cell.setAttribute('data-value', val);
                    }
                }

                cell.addEventListener('click', () => this.selectCell(r, c));
                this.boardElement.appendChild(cell);
            }
        }
    }

    selectCell(row, col) {
        this.selectedRow = row;
        this.selectedCol = col;

        document.querySelectorAll('.main-grid-cell').forEach(c => {
            c.classList.remove('selected', 'hover-highlight', 'same-number');
        });

        const activeCell = this.getCellDiv(row, col);
        if(activeCell) activeCell.classList.add('selected');

        this.highlightGroup(row, col);

        const val = this.engine.getValue(row, col);
        if (val !== 0) {
            this.highlightSameNumbers(val);
        }
    }

    highlightGroup(row, col) {
        const startRow = row - (row % 3);
        const startCol = col - (col % 3);

        const cells = document.querySelectorAll('.main-grid-cell');
        cells.forEach(cell => {
            const r = parseInt(cell.dataset.row);
            const c = parseInt(cell.dataset.col);

            if (r === row || c === col ||
                (r >= startRow && r < startRow + 3 && c >= startCol && c < startCol + 3)) {

                if (r !== row || c !== col) {
                    cell.classList.add('hover-highlight');
                }
            }
        });
    }

    highlightSameNumbers(value) {
        const cells = document.querySelectorAll('.main-grid-cell');
        cells.forEach(cell => {
            const r = parseInt(cell.dataset.row);
            const c = parseInt(cell.dataset.col);
            if (this.engine.getValue(r, c) === value) {
                cell.classList.add('same-number');
            }
        });
    }

    setupInputs() {
        document.addEventListener('keydown', (e) => {
            if (this.isPaused) return;
            if (e.key === 'ArrowUp') this.moveSelection(-1, 0);
            else if (e.key === 'ArrowDown') this.moveSelection(1, 0);
            else if (e.key === 'ArrowLeft') this.moveSelection(0, -1);
            else if (e.key === 'ArrowRight') this.moveSelection(0, 1);
            else if (e.key >= '1' && e.key <= '9') this.makeMove(parseInt(e.key));
            else if (e.key === 'Backspace' || e.key === 'Delete') this.makeMove(0);
            else if ((e.ctrlKey || e.metaKey) && e.key === 'z') {
                e.preventDefault();
                this.undoMove();
            }
        });

        document.querySelectorAll('.number').forEach((btn, idx) => {
            btn.addEventListener('click', () => this.makeMove(idx + 1));
        });

        const btnDelete = document.getElementById('btn-delete');
        if (btnDelete) {
            btnDelete.onclick = () => {
                this.makeMove(0);
            };
        }

        document.getElementById('btn-undo')?.addEventListener('click', () => this.undoMove());
    }

    moveSelection(dRow, dCol) {
        let newRow = this.selectedRow + dRow;
        let newCol = this.selectedCol + dCol;
        if (newRow >= 0 && newRow < 9 && newCol >= 0 && newCol < 9) {
            this.selectCell(newRow, newCol);
        }
    }

    makeMove(value) {
        const result = this.engine.makeMove(this.selectedRow, this.selectedCol, value);

        if (!result.success) {
            if (result.reason === 'immutable') {
                const cell = this.getCellDiv(this.selectedRow, this.selectedCol);
                cell.classList.add('shake');
                setTimeout(() => cell.classList.remove('shake'), 300);
            }
            return;
        }

        this.updateCellUI(this.selectedRow, this.selectedCol, value);

        this.selectCell(this.selectedRow, this.selectedCol);

        this.clearAllErrors();

        if (result.isError) {
            this.getCellDiv(this.selectedRow, this.selectedCol).classList.add('cell-err');

            result.conflicts.forEach(conf => {
                const cell = this.getCellDiv(conf.row, conf.col);
                if (cell) cell.classList.add('cell-err');
            });
        }

        if (this.engine.checkWin()) {
            this.handleWin();
        }
    }

    undoMove() {
        const action = this.engine.undo();
        if (action) {
            this.updateCellUI(action.row, action.col, action.oldValue);
            this.selectCell(action.row, action.col);
            this.clearAllErrors();
        }
    }

    updateCellUI(row, col, value) {
        const cell = this.getCellDiv(row, col);
        if (!cell) return;

        cell.innerText = value === 0 ? '' : value;
        cell.setAttribute('data-value', value);
        cell.classList.add('zoom-in');
        setTimeout(() => cell.classList.remove('zoom-in'), 200);
    }

    clearAllErrors() {
        document.querySelectorAll('.cell-err').forEach(c => c.classList.remove('cell-err'));
    }

    getCellDiv(row, col) {
        return document.querySelector(`.main-grid-cell[data-row="${row}"][data-col="${col}"]`);
    }

    startTimer() {
        if (this.timer) clearInterval(this.timer);
        this.seconds = 0;
        this.timer = setInterval(() => {
            if (!this.isPaused) {
                this.seconds++;
                this.timerElement.innerText = new Date(this.seconds * 1000).toISOString().substr(11, 8);
            }
        }, 1000);
    }

    handleWin() {
        clearInterval(this.timer);
        document.querySelector('#result-time').innerText = this.timerElement.innerText;
        this.resultModal.classList.add('active');
    }
}
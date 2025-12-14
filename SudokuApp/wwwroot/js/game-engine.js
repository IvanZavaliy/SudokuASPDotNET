export class SudokuEngine {
    constructor(puzzleDto) {
        this.originalGrid = JSON.parse(JSON.stringify(puzzleDto.puzzleGrid));
        this.currentGrid = JSON.parse(JSON.stringify(puzzleDto.puzzleGrid));
        this.solution = puzzleDto.solutionGrid;

        this.history = [];
        this.historyStep = -1;
    }

    makeMove(row, col, value) {
        if (this.isImmutable(row, col)) {
            return { success: false, reason: 'immutable' };
        }

        const oldValue = this.currentGrid[row][col];
        if (oldValue === value) return { success: false, reason: 'same_value' };

        if (this.historyStep < this.history.length - 1) {
            this.history = this.history.slice(0, this.historyStep + 1);
        }

        this.history.push({ row, col, oldValue, newValue: value });
        this.historyStep++;

        this.currentGrid[row][col] = value;

        const conflicts = this.validateMove(row, col, value);

        return {
            success: true,
            isError: conflicts.length > 0,
            conflicts: conflicts
        };
    }

    undo() {
        if (this.historyStep < 0) return null;

        const action = this.history[this.historyStep];
        this.currentGrid[action.row][action.col] = action.oldValue;
        this.historyStep--;

        return action;
    }

    redo() {
        if (this.historyStep >= this.history.length - 1) return null;

        this.historyStep++;
        const action = this.history[this.historyStep];
        this.currentGrid[action.row][action.col] = action.newValue;

        return action;
    }

    isImmutable(row, col) {
        return this.originalGrid[row][col] !== 0;
    }

    checkWin() {
        for (let r = 0; r < 9; r++) {
            for (let c = 0; c < 9; c++) {
                if (this.currentGrid[r][c] !== this.solution[r][c]) {
                    return false;
                }
            }
        }
        return true;
    }

    validateMove(row, col, value) {
        if (value === 0) return [];

        let conflicts = [];

        for (let c = 0; c < 9; c++) {
            if (c !== col && this.currentGrid[row][c] === value) {
                conflicts.push({ type: 'row', row: row, col: c });
            }
        }

        for (let r = 0; r < 9; r++) {
            if (r !== row && this.currentGrid[r][col] === value) {
                conflicts.push({ type: 'col', row: r, col: col });
            }
        }

        const startRow = row - (row % 3);
        const startCol = col - (col % 3);

        for (let r = 0; r < 3; r++) {
            for (let c = 0; c < 3; c++) {
                const globalRow = startRow + r;
                const globalCol = startCol + c;
                if ((globalRow !== row || globalCol !== col) &&
                    this.currentGrid[globalRow][globalCol] === value) {
                    conflicts.push({ type: 'box', row: globalRow, col: globalCol });
                }
            }
        }

        return conflicts;
    }

    getValue(row, col) {
        return this.currentGrid[row][col];
    }
}
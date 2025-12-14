export class SudokuEngine {
    constructor(puzzleDto) {
        this.originalGrid = JSON.parse(JSON.stringify(puzzleDto.puzzleGrid));
        this.currentGrid = JSON.parse(JSON.stringify(puzzleDto.puzzleGrid));
        this.solution = puzzleDto.solutionGrid;

        this.history = [];
        this.historyStep = -1;
    }

    /**
     * Основний метод ходу
     * @param {number} row - Рядок (0-8)
     * @param {number} col - Стовпець (0-8)
     * @param {number} value - Значення (0 для очищення, 1-9 для цифри)
     * @returns {object} Результат: { success, isError, conflicts }
     */
    makeMove(row, col, value) {
        // 1. Перевірка: чи можна змінювати цю клітинку (Start Numbers)
        if (this.isImmutable(row, col)) {
            return { success: false, reason: 'immutable' };
        }

        const oldValue = this.currentGrid[row][col];
        if (oldValue === value) return { success: false, reason: 'same_value' };

        // 2. Обрізаємо історію, якщо зробили хід після кількох Undo
        if (this.historyStep < this.history.length - 1) {
            this.history = this.history.slice(0, this.historyStep + 1);
        }

        // 3. Записуємо хід в історію
        this.history.push({ row, col, oldValue, newValue: value });
        this.historyStep++;

        // 4. Оновлюємо стан
        this.currentGrid[row][col] = value;

        // 5. Перевіряємо на конфлікти (Game Rules)
        const conflicts = this.validateMove(row, col, value);

        return {
            success: true,
            isError: conflicts.length > 0,
            conflicts: conflicts // Масив об'єктів {row, col} де є конфлікт
        };
    }

    /**
     * Повертає стан на один крок назад
     */
    undo() {
        if (this.historyStep < 0) return null; // Немає куди відкочуватись

        const action = this.history[this.historyStep];
        this.currentGrid[action.row][action.col] = action.oldValue;
        this.historyStep--;

        return action; // Повертаємо, щоб UI знав, яку клітинку перемалювати
    }

    /**
     * Повертає скасований хід (Redo)
     */
    redo() {
        if (this.historyStep >= this.history.length - 1) return null;

        this.historyStep++;
        const action = this.history[this.historyStep];
        this.currentGrid[action.row][action.col] = action.newValue;

        return action;
    }

    /**
     * Перевіряє, чи є число стартовим (незмінним)
     */
    isImmutable(row, col) {
        return this.originalGrid[row][col] !== 0;
    }

    /**
     * Перевірка на перемогу (порівняння з SolutionGrid)
     */
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

    /**
     * Валідація правил Судоку (пошук дублікатів)
     * Повертає масив координат конфліктуючих клітинок
     */
    validateMove(row, col, value) {
        if (value === 0) return []; // Очищення клітинки не викликає конфліктів

        let conflicts = [];

        // Перевірка рядка
        for (let c = 0; c < 9; c++) {
            if (c !== col && this.currentGrid[row][c] === value) {
                conflicts.push({ type: 'row', row: row, col: c });
            }
        }

        // Перевірка стовпця
        for (let r = 0; r < 9; r++) {
            if (r !== row && this.currentGrid[r][col] === value) {
                conflicts.push({ type: 'col', row: r, col: col });
            }
        }

        // Перевірка квадрату 3х3
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

    /**
     * Отримати поточне значення клітинки
     */
    getValue(row, col) {
        return this.currentGrid[row][col];
    }
}
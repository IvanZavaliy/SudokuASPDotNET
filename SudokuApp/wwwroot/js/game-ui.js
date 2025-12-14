import { SudokuEngine } from './game-engine.js';

export class SudokuUI {
    constructor() {
        // DOM Elements
        this.boardElement = document.getElementById('sudoku-board');
        this.timerElement = document.getElementById('game-time');
        this.resultModal = document.getElementById('result-screen');

        // State
        this.engine = null;
        this.timer = null;
        this.seconds = 0;
        this.selectedRow = 0;
        this.selectedCol = 0;
        this.isPaused = false;
    }

    /**
     * Точка входу: завантаження гри з API
     * Реалізує сценарій [P4]: Швидке отримання гри з пулу
     */
    async init(level = 'Easy') {
        try {
            // Показуємо лоадер, поки йде запит
            this.boardElement.innerHTML = '<div class="loader">Loading...</div>';

            const response = await fetch(`/api/game/new/${level}`);
            if (!response.ok) throw new Error('Failed to load game');

            const gameDto = await response.json();

            // Ініціалізуємо "двигун" (Logic Layer) [M-T3]
            this.engine = new SudokuEngine(gameDto);

            // Будуємо інтерфейс
            this.renderBoard();
            this.startTimer();
            this.setupInputs();

            // Фокус на центральну клітинку [U-Ef1]
            this.selectCell(4, 4);

        } catch (error) {
            console.error(error);
            this.boardElement.innerHTML = '<div class="error">Error loading game. Please try again.</div>';
        }
    }

    /**
     * Генерація HTML сітки (Dynamic Rendering)
     */
    renderBoard() {
        this.boardElement.innerHTML = '';

        for (let r = 0; r < 9; r++) {
            for (let c = 0; c < 9; c++) {
                const cell = document.createElement('div');
                cell.classList.add('main-grid-cell');
                cell.dataset.row = r;
                cell.dataset.col = c;

                // Перевіряємо, чи це стартове число
                const val = this.engine.getValue(r, c);
                if (val !== 0) {
                    cell.innerText = val;
                    cell.classList.add('filled'); // Стиль для незмінних цифр
                }

                // Додаємо обробник кліку мишею
                cell.addEventListener('click', () => this.selectCell(r, c));

                this.boardElement.appendChild(cell);
            }
        }
    }

    /**
     * Логіка виділення клітинки та підсвітки (Smart Highlighting [U-Fb1])
     */
    selectCell(row, col) {
        this.selectedRow = row;
        this.selectedCol = col;

        // 1. Очищення старих стилів
        document.querySelectorAll('.main-grid-cell').forEach(c => {
            c.classList.remove('selected', 'hover-highlight', 'same-number');
        });

        // 2. Виділення активної клітинки
        const activeCell = this.getCellDiv(row, col);
        activeCell.classList.add('selected');

        // 3. Підсвітка рядка, колонки та квадрату (Smart Highlight)
        this.highlightGroup(row, col);

        // 4. Підсвітка однакових цифр
        const val = this.engine.getValue(row, col);
        if (val !== 0) {
            this.highlightSameNumbers(val);
        }
    }

    highlightGroup(row, col) {
        // Визначаємо межі квадрата 3х3
        const startRow = row - (row % 3);
        const startCol = col - (col % 3);

        const cells = document.querySelectorAll('.main-grid-cell');
        cells.forEach(cell => {
            const r = parseInt(cell.dataset.row);
            const c = parseInt(cell.dataset.col);

            // Якщо це той самий рядок, колонка або квадрат
            if (r === row || c === col ||
                (r >= startRow && r < startRow + 3 && c >= startCol && c < startCol + 3)) {
                if (r !== row || c !== col) { // Не підсвічуємо саму вибрану клітинку цим класом
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

    /**
     * Обробка введення (Клавіатура + Миша)
     * [U-Ef1] Keyboard Mastery
     */
    setupInputs() {
        // Клавіатура
        document.addEventListener('keydown', (e) => {
            if (this.isPaused) return;

            // Навігація стрілками
            if (e.key === 'ArrowUp') this.moveSelection(-1, 0);
            else if (e.key === 'ArrowDown') this.moveSelection(1, 0);
            else if (e.key === 'ArrowLeft') this.moveSelection(0, -1);
            else if (e.key === 'ArrowRight') this.moveSelection(0, 1);

            // Введення цифр (1-9)
            else if (e.key >= '1' && e.key <= '9') {
                this.makeMove(parseInt(e.key));
            }

            // Видалення (Backspace / Delete)
            else if (e.key === 'Backspace' || e.key === 'Delete') {
                this.makeMove(0);
            }

            // Undo (Ctrl+Z) [U-Ef3]
            else if ((e.ctrlKey || e.metaKey) && e.key === 'z') {
                e.preventDefault();
                this.undoMove();
            }
        });

        // Віртуальна цифрова клавіатура (для мобільних/миші)
        document.querySelectorAll('.number').forEach((btn, idx) => {
            btn.addEventListener('click', () => this.makeMove(idx + 1));
        });

        // Кнопки управління
        document.getElementById('btn-undo')?.addEventListener('click', () => this.undoMove());
        document.getElementById('btn-new-game')?.addEventListener('click', () => location.reload()); // Або перезапуск init()
    }

    moveSelection(dRow, dCol) {
        let newRow = this.selectedRow + dRow;
        let newCol = this.selectedCol + dCol;

        // Clamp (не виходити за межі) або Wrap (зациклити)
        if (newRow >= 0 && newRow < 9 && newCol >= 0 && newCol < 9) {
            this.selectCell(newRow, newCol);
        }
    }

    /**
     * Виконання ходу
     * [P-L1] Optimistic Update - миттєво оновлюємо UI
     */
    makeMove(value) {
        const result = this.engine.makeMove(this.selectedRow, this.selectedCol, value);

        if (!result.success) {
            // Анімація "трусіння" якщо не можна змінити (immutable)
            if (result.reason === 'immutable') {
                const cell = this.getCellDiv(this.selectedRow, this.selectedCol);
                cell.classList.add('shake');
                setTimeout(() => cell.classList.remove('shake'), 300);
            }
            return;
        }

        // Оновлюємо UI
        this.updateCellUI(this.selectedRow, this.selectedCol, value);

        // Оновлюємо підсвітку цифр
        this.selectCell(this.selectedRow, this.selectedCol);

        // Обробка помилок (Conflict Highlighting)
        this.clearErrors();
        if (result.isError) {
            result.conflicts.forEach(conf => {
                const cell = this.getCellDiv(conf.row, conf.col);
                cell.classList.add('cell-err');
            });
            // Саму клітинку теж підсвітити як помилкову
            this.getCellDiv(this.selectedRow, this.selectedCol).classList.add('cell-err');
        }

        // Перевірка перемоги
        if (this.engine.checkWin()) {
            this.handleWin();
        }
    }

    undoMove() {
        const action = this.engine.undo();
        if (action) {
            this.updateCellUI(action.row, action.col, action.oldValue);
            this.selectCell(action.row, action.col); // Перемістити фокус туди, де була зміна
            this.clearErrors(); // Скидаємо помилки, бо стан змінився

            // Повторна валідація, щоб показати актуальні помилки (якщо залишилися)
            // (В повній версії можна перевіряти всю дошку, тут спрощено)
        }
    }

    updateCellUI(row, col, value) {
        const cell = this.getCellDiv(row, col);
        cell.innerText = value === 0 ? '' : value;
        cell.setAttribute('data-value', value);

        // Анімація введення
        cell.classList.add('zoom-in');
        setTimeout(() => cell.classList.remove('zoom-in'), 200);
    }

    clearErrors() {
        document.querySelectorAll('.cell-err').forEach(c => c.classList.remove('cell-err'));
    }

    getCellDiv(row, col) {
        // Шукаємо div за дата-атрибутами (швидше, ніж по ID для 81 елемента)
        return document.querySelector(`.main-grid-cell[data-row="${row}"][data-col="${col}"]`);
    }

    // --- Timer Logic ---
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
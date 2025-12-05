const cells = document.querySelectorAll('.main-grid-cell');
const number_inputs = document.querySelectorAll('.number');
const game_time = document.querySelector('#game-time');
const result_screen = document.querySelector('#result-screen');
const result_time = document.querySelector('#result-time');
const pause_screen = document.querySelector('#pause-screen');

let timer = null;
let pause = false;
let seconds = 0;
let selected_cell = -1;

// Форматуємо час
const showTime = (seconds) => new Date(seconds * 1000).toISOString().substr(11, 8);

const startTimer = () => {
    // Очищаємо старий таймер, щоб не було прискорень
    if (timer) clearInterval(timer);

    timer = setInterval(() => {
        if (!pause) {
            seconds++;
            game_time.innerHTML = showTime(seconds);
        }
    }, 1000);
};

// --- ВІЗУАЛ (Підсвітка) ---
const resetBg = () => cells.forEach(e => e.classList.remove('hover'));

const hoverBg = (index) => {
    let row = Math.floor(index / 9);
    let col = index % 9;

    let box_start_row = row - row % 3;
    let box_start_col = col - col % 3;

    // Підсвічуємо квадрат 3х3
    for (let i = 0; i < 3; i++) {
        for (let j = 0; j < 3; j++) {
            cells[9 * (box_start_row + i) + (box_start_col + j)].classList.add('hover');
        }
    }
    // Підсвічуємо рядок і колонку
    for (let i = 0; i < 9; i++) {
        cells[row * 9 + i].classList.add('hover');
        cells[i * 9 + col].classList.add('hover');
    }
};

// --- ЛОГІКА ПЕРЕВІРКИ (ВИПРАВЛЕНО) ---
const checkErr = (value) => {
    let errorFound = false; // Прапорець: чи знайшли ми хоч одну помилку?

    const addErr = (cell) => {
        if (parseInt(cell.getAttribute('data-value')) === value) {
            // Знайшли дублікат!
            cell.classList.add('err', 'cell-err');
            setTimeout(() => cell.classList.remove('cell-err'), 500);
            errorFound = true; // Запам'ятовуємо, що є помилка
        }
    };

    let index = selected_cell;
    let row = Math.floor(index / 9);
    let col = index % 9;

    let box_start_row = row - row % 3;
    let box_start_col = col - col % 3;

    // 1. Перевірка квадрату 3х3
    for (let i = 0; i < 3; i++) {
        for (let j = 0; j < 3; j++) {
            let cellIndex = 9 * (box_start_row + i) + (box_start_col + j);
            if (cellIndex !== index) addErr(cells[cellIndex]);
        }
    }

    // 2. Перевірка рядка
    for (let step = 0; step < 9; step++) {
        let cellIndex = 9 * row + step;
        if (cellIndex !== index) addErr(cells[cellIndex]);
    }

    // 3. Перевірка колонки
    for (let step = 0; step < 9; step++) {
        let cellIndex = step * 9 + col;
        if (cellIndex !== index) addErr(cells[cellIndex]);
    }

    // 4. Фінальний штрих: якщо знайшли помилку у сусідів, 
    // то і сама наша клітинка теж має бути червоною!
    if (errorFound) {
        cells[index].classList.add('err', 'cell-err');
        setTimeout(() => cells[index].classList.remove('cell-err'), 500);
    }
};

const removeErr = () => cells.forEach(e => e.classList.remove('err'));

const checkWin = () => {
    // Перевіряємо, чи співпадає все поле з SOLUTION (який прийшов з C#)
    for (let i = 0; i < 81; i++) {
        let row = Math.floor(i / 9);
        let col = i % 9;
        let val = parseInt(cells[i].getAttribute('data-value')) || 0;

        // Якщо клітинка порожня або не співпадає з відповіддю
        if (val !== SOLUTION[row][col]) return false;
    }
    return true;
};

// --- ПОДІЇ (Кліки) ---

// Клік по клітинці дошки
cells.forEach((cell, index) => {
    cell.addEventListener('click', () => {
        if (!cell.classList.contains('filled')) {
            cells.forEach(c => c.classList.remove('selected'));
            selected_cell = index;
            cell.classList.remove('err');
            cell.classList.add('selected');
            resetBg();
            hoverBg(index);
        }
    });
});

// Клік по цифрах (Введення)
number_inputs.forEach((btn, idx) => {
    btn.addEventListener('click', () => {
        if (selected_cell >= 0 && !cells[selected_cell].classList.contains('filled')) {
            let val = idx + 1;
            let cell = cells[selected_cell];

            // Записуємо значення
            cell.innerHTML = val;
            cell.setAttribute('data-value', val);

            // Спочатку прибираємо старі помилки
            removeErr();

            // Потім шукаємо нові
            checkErr(val);

            // Ефект збільшення
            cell.classList.add('zoom-in');
            setTimeout(() => cell.classList.remove('zoom-in'), 500);

            // Перевірка перемоги
            if (checkWin()) {
                clearInterval(timer);
                result_time.innerHTML = showTime(seconds);
                result_screen.classList.add('active');
            }
        }
    });
});

// Кнопка видалення
document.querySelector('#btn-delete').addEventListener('click', () => {
    if (selected_cell >= 0 && !cells[selected_cell].classList.contains('filled')) {
        cells[selected_cell].innerHTML = '';
        cells[selected_cell].setAttribute('data-value', 0);
        removeErr(); // Якщо видалили число, помилка зникає
    }
});

// Пауза
document.querySelector('#btn-pause').addEventListener('click', () => {
    pause = true;
    pause_screen.classList.add('active');
});
document.querySelector('#btn-resume').addEventListener('click', () => {
    pause = false;
    pause_screen.classList.remove('active');
});

document.querySelector('#btn-new-game').addEventListener('click', () => {
    window.location.href = "/"; // Повернення на головну
});

// Старт
startTimer();
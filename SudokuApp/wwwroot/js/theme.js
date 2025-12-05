// wwwroot/js/theme.js
const toggle = document.querySelector('#dark-mode-toggle');
if(toggle) {
    toggle.addEventListener('click', () => {
        document.body.classList.toggle('dark');
        localStorage.setItem('darkmode', document.body.classList.contains('dark'));
    });
}
// Init
if (localStorage.getItem('darkmode') === 'true') {
    document.body.classList.add('dark');
}
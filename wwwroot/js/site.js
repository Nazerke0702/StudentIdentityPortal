(() => {
  const body = document.body;
  const themeButton = document.getElementById('themeToggle');
  const savedTheme = localStorage.getItem('student-portal-theme');
  if (savedTheme === 'dark') body.classList.add('dark');
  themeButton?.addEventListener('click', () => {
    body.classList.toggle('dark');
    localStorage.setItem('student-portal-theme', body.classList.contains('dark') ? 'dark' : 'light');
  });
  document.querySelectorAll('.password-toggle').forEach(button => {
    button.addEventListener('click', () => {
      const input = button.parentElement.querySelector('input');
      const reveal = input.type === 'password';
      input.type = reveal ? 'text' : 'password';
      button.textContent = reveal ? 'Hide' : 'Show';
      button.setAttribute('aria-label', reveal ? 'Hide password' : 'Show password');
    });
  });
})();

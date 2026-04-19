/**
 * Theme toggle with View Transitions API (circular reveal effect).
 * Adapted from https://github.com/rudrodip/theme-toggle-effect
 *
 * Usage: include this script, then call initThemeToggle(buttonSelector).
 * The button should contain two child icons with id="icon-moon" and id="icon-sun".
 */

(function () {
  'use strict';

  // Inject view-transition CSS once
  var style = document.createElement('style');
  style.textContent = [
    '::view-transition-group(root) {',
    '  animation-timing-function: linear(',
    '    0 0%, 0.1684 2.66%, 0.3165 5.49%, 0.446 8.52%, 0.5581 11.78%,',
    '    0.6535 15.29%, 0.7341 19.11%, 0.8011 23.3%, 0.8557 27.93%,',
    '    0.8962 32.68%, 0.9283 38.01%, 0.9529 44.08%, 0.9711 51.14%,',
    '    0.9833 59.06%, 0.9915 68.74%, 1 100%',
    '  );',
    '}',
    '::view-transition-new(root) {',
    '  mask: url(\'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 40 40"><defs><filter id="blur"><feGaussianBlur stdDeviation="2"/></filter></defs><circle cx="20" cy="20" r="18" fill="white" filter="url(%23blur)"/></svg>\') center / 0 no-repeat;',
    '  animation: __vt-scale 0.8s;',
    '  animation-fill-mode: both;',
    '}',
    '::view-transition-old(root),',
    '[data-theme="dark"]::view-transition-old(root) {',
    '  animation: none;',
    '  animation-fill-mode: both;',
    '  z-index: -1;',
    '}',
    '[data-theme="dark"]::view-transition-new(root) {',
    '  animation: __vt-scale 0.8s;',
    '  animation-fill-mode: both;',
    '}',
    '@keyframes __vt-scale {',
    '  to { mask-size: 200vmax; }',
    '}',
  ].join('\n');
  document.head.appendChild(style);

  function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);

    var moon = document.getElementById('icon-moon');
    var sun = document.getElementById('icon-sun');
    if (moon) moon.style.display = theme === 'dark' ? 'none' : '';
    if (sun) sun.style.display = theme === 'dark' ? '' : 'none';
  }

  // Expose globally so templates can call it from onclick
  window.toggleTheme = function () {
    var current = document.documentElement.getAttribute('data-theme') || 'light';
    var next = current === 'dark' ? 'light' : 'dark';

    if (!document.startViewTransition) {
      applyTheme(next);
      return;
    }

    document.startViewTransition(function () {
      applyTheme(next);
    });
  };

  // Initialise icon state on load
  var saved = localStorage.getItem('theme') || 'light';
  applyTheme(saved);
})();

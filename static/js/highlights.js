/**
 * Highlights Engine — Text highlighting with persistence, Apple Pencil support,
 * and Obsidian-compatible Markdown export.
 *
 * Usage: include this script on chapter pages. Call HL.init(bookSlug, chapterSlug)
 * after DOM ready.
 */
var HL = (function() {
  'use strict';

  var STORAGE_PREFIX = 'highlights_';
  var bookSlug = '';
  var chapterSlug = '';
  var container = null;
  var toolbar = null;
  var currentRange = null;
  var highlights = [];
  var nextId = 1;
  var isPenMode = false;
  var penStartRange = null;

  // ── Initialization ──────────────────────────────────────

  function init(book, chapter) {
    bookSlug = book;
    chapterSlug = chapter;
    container = document.querySelector('.chapter-content');
    if (!container) return;

    loadHighlights();
    applyAllHighlights();
    createToolbar();
    bindEvents();
  }

  // ── Toolbar ─────────────────────────────────────────────

  function createToolbar() {
    toolbar = document.createElement('div');
    toolbar.className = 'highlight-toolbar';
    toolbar.innerHTML =
      '<button class="hl-btn hl-yellow" data-color="yellow" title="Yellow">&#9632;</button>' +
      '<button class="hl-btn hl-green" data-color="green" title="Green">&#9632;</button>' +
      '<button class="hl-btn hl-blue" data-color="blue" title="Blue">&#9632;</button>' +
      '<button class="hl-btn hl-pink" data-color="pink" title="Pink">&#9632;</button>' +
      '<button class="hl-btn hl-remove" data-action="remove" title="Remove highlight">&#10005;</button>';
    toolbar.style.display = 'none';
    document.body.appendChild(toolbar);

    toolbar.addEventListener('mousedown', function(e) {
      e.preventDefault();
      e.stopPropagation();
    });

    toolbar.addEventListener('click', function(e) {
      var btn = e.target.closest('.hl-btn');
      if (!btn) return;
      var action = btn.dataset.action;
      if (action === 'remove') {
        removeHighlightAtSelection();
      } else {
        var color = btn.dataset.color || 'yellow';
        highlightSelection(color);
      }
      hideToolbar();
    });
  }

  function showToolbar(x, y) {
    toolbar.style.display = 'flex';
    var tw = toolbar.offsetWidth;
    var th = toolbar.offsetHeight;
    var left = Math.max(8, Math.min(x - tw / 2, window.innerWidth - tw - 8));
    var top = y - th - 10;
    if (top < 0) top = y + 20;
    toolbar.style.left = left + 'px';
    toolbar.style.top = top + window.scrollY + 'px';
  }

  function hideToolbar() {
    if (toolbar) toolbar.style.display = 'none';
  }

  // ── Pen Preview Overlay ──────────────────────────────────

  var penOverlay = null;

  function createPenOverlay() {
    penOverlay = document.createElement('div');
    penOverlay.className = 'pen-preview-overlay';
    penOverlay.style.display = 'none';
    document.body.appendChild(penOverlay);
  }

  function updatePenPreview() {
    if (!penOverlay) return;
    var sel = window.getSelection();
    if (!sel || sel.isCollapsed) {
      penOverlay.style.display = 'none';
      return;
    }
    try {
      var range = sel.getRangeAt(0);
      var rects = range.getClientRects();
      if (rects.length === 0) { penOverlay.style.display = 'none'; return; }

      // Build highlight rectangles
      var html = '';
      for (var i = 0; i < rects.length; i++) {
        var r = rects[i];
        html += '<div style="position:absolute;left:' + (r.left + window.scrollX) + 'px;top:' + (r.top + window.scrollY) + 'px;width:' + r.width + 'px;height:' + r.height + 'px;background:rgba(255,235,59,0.4);border-radius:2px;pointer-events:none;"></div>';
      }
      penOverlay.innerHTML = html;
      penOverlay.style.display = 'block';
    } catch(e) {
      penOverlay.style.display = 'none';
    }
  }

  function hidePenPreview() {
    if (penOverlay) {
      penOverlay.style.display = 'none';
      penOverlay.innerHTML = '';
    }
  }

  // ── Selection Events ────────────────────────────────────

  function bindEvents() {
    createPenOverlay();

    // Apple Pencil: activate preview mode on pen down
    container.addEventListener('pointerdown', function(e) {
      if (e.pointerType === 'pen') {
        isPenMode = true;
      }
    });

    // Apple Pencil: update live preview overlay as user drags
    container.addEventListener('pointermove', function(e) {
      if (!isPenMode || e.pointerType !== 'pen') return;
      updatePenPreview();
    });

    // Also update on selectionchange for smoother preview
    document.addEventListener('selectionchange', function() {
      if (isPenMode) updatePenPreview();
    });

    // Pointer up — handle pen auto-highlight or show toolbar for mouse/touch
    container.addEventListener('pointerup', function(e) {
      if (e.pointerType === 'pen') {
        isPenMode = false;
        hidePenPreview();
        setTimeout(function() {
          var sel = window.getSelection();
          if (sel && !sel.isCollapsed && container.contains(sel.anchorNode)) {
            highlightSelection('yellow');
            sel.removeAllRanges();
          }
        }, 10);
        return;
      }

      setTimeout(function() {
        var sel = window.getSelection();
        if (!sel || sel.isCollapsed) { hideToolbar(); return; }
        if (!container.contains(sel.anchorNode)) { hideToolbar(); return; }

        var range = sel.getRangeAt(0);
        currentRange = range;
        var rect = range.getBoundingClientRect();
        showToolbar(rect.left + rect.width / 2, rect.top);
      }, 10);
    });

    // Cancel pen mode if pointer is cancelled
    container.addEventListener('pointercancel', function(e) {
      if (e.pointerType === 'pen') {
        isPenMode = false;
        hidePenPreview();
      }
    });

    // Hide toolbar on click outside
    document.addEventListener('pointerdown', function(e) {
      if (toolbar && !toolbar.contains(e.target) && !e.target.closest('.hl-delete-inline')) {
        hideToolbar();
      }
    });

    // Keyboard shortcut: Ctrl/Cmd+Shift+H to highlight selection
    document.addEventListener('keydown', function(e) {
      if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === 'H') {
        e.preventDefault();
        var sel = window.getSelection();
        if (sel && !sel.isCollapsed && container.contains(sel.anchorNode)) {
          highlightSelection('yellow');
        }
      }
    });
  }

  // ── Highlight Application ───────────────────────────────

  function highlightSelection(color) {
    var sel = window.getSelection();
    if (!sel || sel.isCollapsed) return;
    var range = sel.getRangeAt(0);
    if (!container.contains(range.commonAncestorContainer)) return;

    var hlId = 'hl-' + nextId++;
    var text = range.toString();
    if (!text.trim()) return;

    var anchor = serializeRange(range);
    if (!anchor) return;

    wrapRange(range, hlId, color);
    sel.removeAllRanges();

    var entry = {
      id: hlId,
      color: color,
      text: text,
      anchor: anchor,
      timestamp: Date.now()
    };
    highlights.push(entry);
    saveHighlights();
  }

  function wrapRange(range, hlId, color) {
    var textNodes = getTextNodesInRange(range);
    if (textNodes.length === 0) return;

    var lastMark = null;
    for (var i = 0; i < textNodes.length; i++) {
      var node = textNodes[i];
      var nodeRange = document.createRange();

      if (node === range.startContainer && node === range.endContainer) {
        nodeRange.setStart(node, range.startOffset);
        nodeRange.setEnd(node, range.endOffset);
      } else if (node === range.startContainer) {
        nodeRange.setStart(node, range.startOffset);
        nodeRange.setEnd(node, node.textContent.length);
      } else if (node === range.endContainer) {
        nodeRange.setStart(node, 0);
        nodeRange.setEnd(node, range.endOffset);
      } else {
        nodeRange.selectNodeContents(node);
      }

      if (nodeRange.toString().length === 0) continue;

      var mark = document.createElement('mark');
      mark.className = 'user-highlight hl-' + color;
      mark.dataset.hlId = hlId;
      nodeRange.surroundContents(mark);
      lastMark = mark;
    }

    // Add persistent delete button after the last mark of this highlight
    if (lastMark) {
      addDeleteButton(lastMark, hlId);
    }
  }

  function addDeleteButton(afterEl, hlId) {
    var delBtn = document.createElement('button');
    delBtn.className = 'hl-delete-inline';
    delBtn.dataset.hlId = hlId;
    delBtn.innerHTML = '&#10005;';
    delBtn.title = 'Delete highlight';
    delBtn.addEventListener('click', function(ev) {
      ev.preventDefault();
      ev.stopPropagation();
      removeHighlight(hlId);
    });
    afterEl.parentNode.insertBefore(delBtn, afterEl.nextSibling);
  }

  function getTextNodesInRange(range) {
    var nodes = [];
    var walker = document.createTreeWalker(
      range.commonAncestorContainer.nodeType === Node.TEXT_NODE
        ? range.commonAncestorContainer.parentNode
        : range.commonAncestorContainer,
      NodeFilter.SHOW_TEXT,
      null
    );

    var started = false;
    while (walker.nextNode()) {
      var node = walker.currentNode;
      if (node === range.startContainer) started = true;
      if (started && range.intersectsNode(node)) {
        // Skip nodes already inside a highlight mark
        if (!node.parentElement.closest('.user-highlight')) {
          nodes.push(node);
        }
      }
      if (node === range.endContainer) break;
    }
    return nodes;
  }

  // ── Highlight Removal ───────────────────────────────────

  function removeHighlightAtSelection() {
    var hlId = toolbar.dataset.activeHlId;
    if (hlId) {
      removeHighlight(hlId);
      delete toolbar.dataset.activeHlId;
      return;
    }
    // Fallback: remove highlight under current selection
    var sel = window.getSelection();
    if (!sel || sel.isCollapsed) return;
    var node = sel.anchorNode;
    var mark = node.nodeType === Node.TEXT_NODE ? node.parentElement : node;
    var hl = mark.closest('.user-highlight');
    if (hl) removeHighlight(hl.dataset.hlId);
  }

  function removeHighlight(hlId) {
    // Remove delete button(s) for this highlight
    container.querySelectorAll('.hl-delete-inline[data-hl-id="' + hlId + '"]').forEach(function(b) { b.remove(); });
    // Unwrap mark elements
    var marks = container.querySelectorAll('.user-highlight[data-hl-id="' + hlId + '"]');
    marks.forEach(function(mark) {
      var parent = mark.parentNode;
      while (mark.firstChild) {
        parent.insertBefore(mark.firstChild, mark);
      }
      parent.removeChild(mark);
      parent.normalize();
    });
    highlights = highlights.filter(function(h) { return h.id !== hlId; });
    saveHighlights();
  }

  // ── Range Serialization (XPath-based) ───────────────────

  function serializeRange(range) {
    try {
      return {
        startPath: getNodePath(range.startContainer),
        startOffset: range.startOffset,
        endPath: getNodePath(range.endContainer),
        endOffset: range.endOffset,
        text: range.toString()
      };
    } catch (e) {
      return null;
    }
  }

  function deserializeRange(anchor) {
    try {
      var startNode = resolveNodePath(anchor.startPath);
      var endNode = resolveNodePath(anchor.endPath);
      if (!startNode || !endNode) return null;

      var range = document.createRange();
      range.setStart(startNode, Math.min(anchor.startOffset, startNode.textContent.length));
      range.setEnd(endNode, Math.min(anchor.endOffset, endNode.textContent.length));
      return range;
    } catch (e) {
      return null;
    }
  }

  function getNodePath(node) {
    var path = [];
    var current = node.nodeType === Node.TEXT_NODE ? node : node.firstChild || node;
    var textNode = node.nodeType === Node.TEXT_NODE ? node : null;

    var el = textNode ? textNode.parentElement : node;

    while (el && el !== container) {
      var parent = el.parentElement;
      if (!parent) break;
      var children = Array.from(parent.children);
      var idx = children.indexOf(el);
      path.unshift(el.tagName.toLowerCase() + '[' + idx + ']');
      el = parent;
    }

    // Add text node index within parent
    if (textNode) {
      var textIdx = 0;
      var child = textNode.parentElement.firstChild;
      while (child && child !== textNode) {
        if (child.nodeType === Node.TEXT_NODE) textIdx++;
        child = child.nextSibling;
      }
      path.push('#text[' + textIdx + ']');
    }

    return path.join('/');
  }

  function resolveNodePath(pathStr) {
    var parts = pathStr.split('/');
    var current = container;

    for (var i = 0; i < parts.length; i++) {
      var part = parts[i];
      var textMatch = part.match(/^#text\[(\d+)\]$/);
      if (textMatch) {
        var textIdx = parseInt(textMatch[1], 10);
        var count = 0;
        var child = current.firstChild;
        while (child) {
          if (child.nodeType === Node.TEXT_NODE) {
            if (count === textIdx) return child;
            count++;
          }
          child = child.nextSibling;
        }
        return current.firstChild;
      }

      var match = part.match(/^(\w+)\[(\d+)\]$/);
      if (!match) return null;
      var tag = match[1];
      var idx = parseInt(match[2], 10);
      var children = Array.from(current.children);
      if (idx >= children.length) return null;
      current = children[idx];
    }
    return current;
  }

  // ── Persistence ─────────────────────────────────────────

  function storageKey() {
    return STORAGE_PREFIX + bookSlug + '_' + chapterSlug;
  }

  function saveHighlights() {
    try {
      localStorage.setItem(storageKey(), JSON.stringify({
        highlights: highlights,
        nextId: nextId
      }));
    } catch (e) {
      console.warn('Highlights: localStorage save failed', e);
    }
  }

  function loadHighlights() {
    try {
      var raw = localStorage.getItem(storageKey());
      if (raw) {
        var data = JSON.parse(raw);
        highlights = data.highlights || [];
        nextId = data.nextId || 1;
      }
    } catch (e) {
      highlights = [];
      nextId = 1;
    }
  }

  function applyAllHighlights() {
    var applied = [];
    for (var i = 0; i < highlights.length; i++) {
      var h = highlights[i];
      var range = deserializeRange(h.anchor);
      if (range && range.toString().length > 0) {
        wrapRange(range, h.id, h.color);
        applied.push(h);
      } else {
        // Try text-based fallback search
        var found = findTextInContainer(h.anchor.text || h.text);
        if (found) {
          wrapRange(found, h.id, h.color);
          applied.push(h);
        }
      }
    }
    highlights = applied;
    if (applied.length !== highlights.length) saveHighlights();
  }

  function findTextInContainer(text) {
    if (!text || text.length < 3) return null;
    var walker = document.createTreeWalker(container, NodeFilter.SHOW_TEXT, null);
    var searchText = text.substring(0, 100);
    while (walker.nextNode()) {
      var node = walker.currentNode;
      var idx = node.textContent.indexOf(searchText);
      if (idx >= 0) {
        var range = document.createRange();
        range.setStart(node, idx);
        range.setEnd(node, Math.min(idx + text.length, node.textContent.length));
        return range;
      }
    }
    return null;
  }

  // ── Get All Highlights (for summary page) ───────────────

  function getAllHighlights() {
    var all = [];
    for (var i = 0; i < localStorage.length; i++) {
      var key = localStorage.key(i);
      if (key.indexOf(STORAGE_PREFIX) !== 0) continue;
      try {
        var data = JSON.parse(localStorage.getItem(key));
        var parts = key.replace(STORAGE_PREFIX, '').split('_');
        var book = parts[0];
        var chapter = parts.slice(1).join('_');
        (data.highlights || []).forEach(function(h) {
          all.push({
            id: h.id,
            color: h.color,
            text: h.text || (h.anchor && h.anchor.text) || '',
            bookSlug: book,
            chapterSlug: chapter,
            timestamp: h.timestamp || 0
          });
        });
      } catch (e) {}
    }
    all.sort(function(a, b) { return b.timestamp - a.timestamp; });
    return all;
  }

  // ── Delete highlight from summary page ──────────────────

  function deleteHighlightById(bookS, chapterS, hlId) {
    var key = STORAGE_PREFIX + bookS + '_' + chapterS;
    try {
      var raw = localStorage.getItem(key);
      if (!raw) return;
      var data = JSON.parse(raw);
      data.highlights = (data.highlights || []).filter(function(h) { return h.id !== hlId; });
      if (data.highlights.length === 0) {
        localStorage.removeItem(key);
      } else {
        localStorage.setItem(key, JSON.stringify(data));
      }
    } catch (e) {}
  }

  function clearAllHighlights() {
    var keys = [];
    for (var i = 0; i < localStorage.length; i++) {
      var key = localStorage.key(i);
      if (key.indexOf(STORAGE_PREFIX) === 0) keys.push(key);
    }
    keys.forEach(function(k) { localStorage.removeItem(k); });
  }

  // ── Markdown Export ─────────────────────────────────────

  function exportAllMarkdown() {
    var all = getAllHighlights();
    if (all.length === 0) return '';

    var grouped = {};
    all.forEach(function(h) {
      var key = h.bookSlug;
      if (!grouped[key]) grouped[key] = {};
      if (!grouped[key][h.chapterSlug]) grouped[key][h.chapterSlug] = [];
      grouped[key][h.chapterSlug].push(h);
    });

    var md = '# OneStream Study Highlights\n\n';
    md += '_Exported on ' + new Date().toLocaleDateString() + '_\n\n';

    Object.keys(grouped).forEach(function(book) {
      md += '## ' + formatSlug(book) + '\n\n';
      Object.keys(grouped[book]).forEach(function(chapter) {
        md += '### [[' + formatSlug(chapter) + ']]\n\n';
        grouped[book][chapter].forEach(function(h) {
          md += '> ' + h.text.replace(/\n/g, '\n> ') + '\n\n';
        });

        // Include user notes if available
        var notesKey = 'notes_' + book + '_' + chapter;
        var notes = null;
        try { notes = localStorage.getItem(notesKey); } catch(e) {}
        if (notes && notes.trim()) {
          md += '#### My Notes\n\n' + notes.trim() + '\n\n';
        }
      });
    });

    return md;
  }

  function exportChapterMarkdown(book, chapter) {
    var key = STORAGE_PREFIX + book + '_' + chapter;
    var md = '# ' + formatSlug(chapter) + '\n\n';
    md += '_From: ' + formatSlug(book) + '_\n\n';
    md += '## Highlights\n\n';

    try {
      var raw = localStorage.getItem(key);
      if (raw) {
        var data = JSON.parse(raw);
        (data.highlights || []).forEach(function(h) {
          var text = h.text || (h.anchor && h.anchor.text) || '';
          md += '> ' + text.replace(/\n/g, '\n> ') + '\n\n';
        });
      }
    } catch (e) {}

    // Include notes
    var notesKey = 'notes_' + book + '_' + chapter;
    try {
      var notes = localStorage.getItem(notesKey);
      if (notes && notes.trim()) {
        md += '## My Notes\n\n' + notes.trim() + '\n\n';
      }
    } catch(e) {}

    return md;
  }

  function downloadMarkdown(filename, content) {
    var blob = new Blob([content], { type: 'text/markdown;charset=utf-8' });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  function downloadAllHighlights() {
    var md = exportAllMarkdown();
    if (!md) { alert('No highlights to export.'); return; }
    var date = new Date().toISOString().slice(0, 10);
    downloadMarkdown('onestream-highlights-' + date + '.md', md);
  }

  function downloadChapterHighlights(book, chapter) {
    var md = exportChapterMarkdown(book, chapter);
    downloadMarkdown(formatSlug(chapter) + '-highlights.md', md);
  }

  // ── Obsidian URI ────────────────────────────────────────

  function openInObsidian(book, chapter) {
    var vault = localStorage.getItem('obsidian_vault');
    if (!vault) {
      vault = prompt('Enter your Obsidian vault name:');
      if (!vault) return;
      localStorage.setItem('obsidian_vault', vault);
    }

    var md = chapter ? exportChapterMarkdown(book, chapter) : exportAllMarkdown();
    if (!md) { alert('No highlights to export.'); return; }

    var name = chapter
      ? 'OneStream/' + formatSlug(chapter)
      : 'OneStream/All Highlights';

    // URI has ~2KB limit; fall back to download for large content
    if (md.length > 1800) {
      if (confirm('Content is too large for Obsidian URI. Download as .md file instead?')) {
        var filename = chapter
          ? formatSlug(chapter) + '-highlights.md'
          : 'onestream-highlights-' + new Date().toISOString().slice(0, 10) + '.md';
        downloadMarkdown(filename, md);
      }
      return;
    }

    var uri = 'obsidian://new?vault=' + encodeURIComponent(vault) +
      '&name=' + encodeURIComponent(name) +
      '&content=' + encodeURIComponent(md);
    window.location.href = uri;
  }

  // ── Utilities ───────────────────────────────────────────

  function formatSlug(slug) {
    return slug.replace(/-/g, ' ').replace(/\b\w/g, function(c) { return c.toUpperCase(); });
  }

  // ── Public API ──────────────────────────────────────────

  return {
    init: init,
    getAllHighlights: getAllHighlights,
    deleteHighlightById: deleteHighlightById,
    clearAllHighlights: clearAllHighlights,
    exportAllMarkdown: exportAllMarkdown,
    exportChapterMarkdown: exportChapterMarkdown,
    downloadAllHighlights: downloadAllHighlights,
    downloadChapterHighlights: downloadChapterHighlights,
    openInObsidian: openInObsidian,
    formatSlug: formatSlug
  };
})();

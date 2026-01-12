/* ============================================
   GESTION STOCK & VENTE — UI Helpers
   Toast, Modal, Scanner focus, Form validation
   ============================================ */

// === Toast Notifications ===
const Toast = {
  container: null,

  init() {
    if (!this.container) {
      this.container = document.createElement('div');
      this.container.className = 'toast-container';
      document.body.appendChild(this.container);
    }
  },

  show(message, type = 'info', duration = 5000) {
    this.init();

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;

    const icons = {
      success: '✓',
      error: '✕',
      warning: '!',
      info: 'i'
    };

    toast.innerHTML = `
      <div class="toast-icon">${icons[type] || icons.info}</div>
      <div class="toast-content">
        <div class="toast-title">${this.getTitleForType(type)}</div>
        <div class="toast-message">${message}</div>
      </div>
      <button class="toast-close" aria-label="Fermer">×</button>
    `;

    const closeBtn = toast.querySelector('.toast-close');
    closeBtn.addEventListener('click', () => this.hide(toast));

    this.container.appendChild(toast);

    if (duration > 0) {
      setTimeout(() => this.hide(toast), duration);
    }

    return toast;
  },

  getTitleForType(type) {
    const titles = {
      success: 'Succès',
      error: 'Erreur',
      warning: 'Attention',
      info: 'Information'
    };
    return titles[type] || titles.info;
  },

  hide(toast) {
    toast.style.animation = 'fadeOut 0.2s ease-out';
    setTimeout(() => toast.remove(), 200);
  },

  success(message, duration) {
    return this.show(message, 'success', duration);
  },

  error(message, duration) {
    return this.show(message, 'error', duration);
  },

  warning(message, duration) {
    return this.show(message, 'warning', duration);
  },

  info(message, duration) {
    return this.show(message, 'info', duration);
  }
};

// Expose globally
window.Toast = Toast;

// === Modal Helper ===
const Modal = {
  confirm(options) {
    return new Promise((resolve) => {
      const { title, message, confirmText = 'Confirmer', cancelText = 'Annuler', danger = false } = options;

      const backdrop = document.createElement('div');
      backdrop.className = 'modal-backdrop';

      const container = document.createElement('div');
      container.className = 'modal-container';

      const modal = document.createElement('div');
      modal.className = 'modal-content';
      modal.innerHTML = `
        <div class="modal-header">
          <h3 class="modal-title">${title}</h3>
        </div>
        <div class="modal-body">
          ${message}
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" data-action="cancel">${cancelText}</button>
          <button class="btn ${danger ? 'btn-danger' : 'btn-primary'}" data-action="confirm">${confirmText}</button>
        </div>
      `;

      container.appendChild(modal);
      document.body.appendChild(backdrop);
      document.body.appendChild(container);

      const cleanup = () => {
        backdrop.remove();
        container.remove();
      };

      modal.querySelector('[data-action="cancel"]').addEventListener('click', () => {
        cleanup();
        resolve(false);
      });

      modal.querySelector('[data-action="confirm"]').addEventListener('click', () => {
        cleanup();
        resolve(true);
      });

      backdrop.addEventListener('click', () => {
        cleanup();
        resolve(false);
      });

      // Focus confirm button
      setTimeout(() => {
        modal.querySelector('[data-action="confirm"]').focus();
      }, 100);
    });
  }
};

window.Modal = Modal;

// === Scanner Focus Management ===
const ScannerFocus = {
  inputId: null,

  init(inputId) {
    this.inputId = inputId;
    const input = document.getElementById(inputId);

    if (!input) return;

    // Focus on page load
    input.focus();

    // Refocus on click anywhere (except buttons/links)
    document.addEventListener('click', (e) => {
      if (!e.target.matches('button, a, input, select, textarea')) {
        setTimeout(() => input.focus(), 10);
      }
    });

    // Refocus after any action
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' && e.target === input) {
        setTimeout(() => input.focus(), 100);
      }
    });
  }
};

window.ScannerFocus = ScannerFocus;

// === Form Validation Helper ===
const FormValidation = {
  validateRequired(input) {
    const value = input.value.trim();
    if (!value) {
      this.showError(input, 'Ce champ est requis.');
      return false;
    }
    this.clearError(input);
    return true;
  },

  validateEmail(input) {
    const value = input.value.trim();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (value && !emailRegex.test(value)) {
      this.showError(input, 'Adresse email invalide.');
      return false;
    }
    this.clearError(input);
    return true;
  },

  validateNumber(input, min = null, max = null) {
    const value = parseFloat(input.value);
    if (isNaN(value)) {
      this.showError(input, 'Veuillez entrer un nombre valide.');
      return false;
    }
    if (min !== null && value < min) {
      this.showError(input, `La valeur doit être au moins ${min}.`);
      return false;
    }
    if (max !== null && value > max) {
      this.showError(input, `La valeur ne peut pas dépasser ${max}.`);
      return false;
    }
    this.clearError(input);
    return true;
  },

  showError(input, message) {
    this.clearError(input);
    input.classList.add('is-invalid');
    const error = document.createElement('span');
    error.className = 'form-error';
    error.textContent = message;
    input.parentNode.appendChild(error);
  },

  clearError(input) {
    input.classList.remove('is-invalid');
    const existing = input.parentNode.querySelector('.form-error');
    if (existing) existing.remove();
  }
};

window.FormValidation = FormValidation;

// === Loading State Helper ===
const LoadingState = {
  show(button) {
    if (!button) return;
    button.disabled = true;
    button.dataset.originalText = button.textContent;
    button.innerHTML = '<span class="loader loader-sm"></span> Chargement...';
  },

  hide(button) {
    if (!button) return;
    button.disabled = false;
    button.textContent = button.dataset.originalText || button.textContent;
  }
};

window.LoadingState = LoadingState;

// === Initialize on DOM ready ===
document.addEventListener('DOMContentLoaded', () => {
  // Auto-focus scanner input if it exists
  const scannerInput = document.querySelector('[data-scanner-focus]');
  if (scannerInput) {
    ScannerFocus.init(scannerInput.id);
  }

  // Replace native confirm dialogs
  document.querySelectorAll('[data-confirm]').forEach(element => {
    element.addEventListener('click', async (e) => {
      e.preventDefault();
      const message = element.dataset.confirm || 'Êtes-vous sûr ?';
      const danger = element.classList.contains('btn-danger');
      const confirmed = await Modal.confirm({
        title: 'Confirmation',
        message,
        danger
      });
      if (confirmed) {
        // If it's a form button, submit the form
        if (element.form) {
          element.form.submit();
        }
        // If it's a link, follow the href
        else if (element.href) {
          window.location.href = element.href;
        }
      }
    });
  });

  // Auto-dismiss alerts after 5 seconds
  document.querySelectorAll('.alert-dismissible').forEach(alert => {
    setTimeout(() => {
      alert.style.animation = 'fadeOut 0.3s ease-out';
      setTimeout(() => alert.remove(), 300);
    }, 5000);
  });
});

// === Global fetch wrapper with error handling ===
window.fetchJSON = async (url, options = {}) => {
  try {
    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options.headers
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    Toast.error(`Erreur: ${error.message}`);
    throw error;
  }
};


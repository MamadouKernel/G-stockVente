/* ============================================
   GESTION STOCK & VENTE — Notifications SignalR
   Connexion temps réel et gestion des notifications
   ============================================ */

(function() {
    'use strict';

    let connection = null;
    let notificationCount = 0;

    // Initialiser la connexion SignalR
    function initSignalR() {
        if (typeof signalR === 'undefined') {
            console.warn('SignalR library not loaded');
            return;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        // Écouter les nouvelles notifications
        connection.on("ReceiveNotification", function (notification) {
            updateNotificationCount(notification.count || (notificationCount + 1));
            loadNotifications();
            showNotificationToast(notification);
        });

        // Mettre à jour le compteur
        connection.on("UpdateNotificationCount", function (count) {
            updateNotificationCount(count);
            loadNotifications();
        });

        // Démarrer la connexion
        connection.start()
            .then(function () {
                console.log("SignalR Connected");
                loadNotifications();
                loadNotificationCount();
            })
            .catch(function (err) {
                console.error("SignalR Connection Error: ", err.toString());
            });

        // Gérer la reconnexion
        connection.onreconnecting(function () {
            console.log("SignalR Reconnecting...");
        });

        connection.onreconnected(function () {
            console.log("SignalR Reconnected");
            loadNotifications();
            loadNotificationCount();
        });
    }

    // Charger le nombre de notifications non lues
    async function loadNotificationCount() {
        try {
            const response = await fetch('/Notifications/GetNonLues');
            if (response.ok) {
                const data = await response.json();
                updateNotificationCount(data.count || 0);
            }
        } catch (error) {
            console.error('Error loading notification count:', error);
        }
    }

    // Charger les notifications pour le dropdown
    async function loadNotifications() {
        const notificationList = document.getElementById('notificationList');
        if (!notificationList) return;

        try {
            const response = await fetch('/Notifications/GetNonLues');
            if (!response.ok) return;

            const data = await response.json();
            const notifications = data.notifications || [];

            if (notifications.length === 0) {
                notificationList.innerHTML = `
                    <div class="text-center text-muted py-4">
                        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" style="opacity: 0.4; margin-bottom: 8px;"><circle cx="12" cy="12" r="10"></circle><path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3"></path><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>
                        <div style="font-size: 13px;">Aucune notification</div>
                    </div>
                `;
                return;
            }

            let html = '';
            notifications.slice(0, 5).forEach(function(notif) {
                const typeClass = getTypeClass(notif.type);
                html += `
                    <a href="${notif.lienAction || '/Notifications'}" class="dropdown-item p-3" style="border-bottom: 1px solid var(--color-border); text-decoration: none;">
                        <div class="d-flex align-items-start">
                            <div class="flex-grow-1">
                                <div class="d-flex align-items-center mb-1">
                                    <span class="badge ${typeClass} me-2" style="font-size: 10px;">${notif.type}</span>
                                    <small class="text-muted" style="font-size: 11px;">${notif.dateCreation}</small>
                                </div>
                                <div style="font-weight: 600; font-size: 13px; color: var(--color-text-strong); margin-bottom: 4px;">${notif.titre}</div>
                                <div style="font-size: 12px; color: var(--color-text); line-height: 1.4;">${notif.message}</div>
                            </div>
                        </div>
                    </a>
                `;
            });

            if (notifications.length > 5) {
                html += `
                    <div class="dropdown-divider"></div>
                    <a href="/Notifications" class="dropdown-item text-center" style="font-size: 12px; font-weight: 600;">
                        Voir toutes les notifications (${notifications.length})
                    </a>
                `;
            }

            notificationList.innerHTML = html;
        } catch (error) {
            console.error('Error loading notifications:', error);
            notificationList.innerHTML = `
                <div class="text-center text-danger py-4" style="font-size: 13px;">
                    Erreur de chargement
                </div>
            `;
        }
    }

    // Mettre à jour le badge de compteur
    function updateNotificationCount(count) {
        notificationCount = count || 0;
        const badge = document.getElementById('notificationCountBadge');
        if (badge) {
            if (count > 0) {
                badge.style.display = '';
                badge.innerHTML = count > 99 ? '99+' : count.toString();
                badge.parentElement.classList.add('position-relative');
                if (!badge.parentElement.querySelector('.position-absolute')) {
                    badge.className = 'position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger';
                    badge.style.fontSize = '10px';
                    badge.style.padding = '2px 6px';
                    badge.style.minWidth = '18px';
                }
            } else {
                badge.style.display = 'none';
            }
        }
    }

    // Obtenir la classe CSS selon le type de notification
    function getTypeClass(type) {
        const classes = {
            'StockBas': 'bg-warning',
            'RuptureStock': 'bg-danger',
            'EcartInventaire': 'bg-warning',
            'AnnulationVente': 'bg-secondary',
            'TransfertEnAttente': 'bg-info',
            'TransfertRecu': 'bg-success',
            'RapportDisponible': 'bg-primary',
            'Autre': 'bg-secondary'
        };
        return classes[type] || 'bg-secondary';
    }

    // Afficher un toast pour une nouvelle notification
    function showNotificationToast(notification) {
        if (typeof Toast !== 'undefined' && Toast.show) {
            const type = notification.type?.includes('Rupture') ? 'error' :
                        notification.type?.includes('Stock') ? 'warning' :
                        notification.type?.includes('Transfert') ? 'info' : 'info';
            Toast.show(notification.message || notification.titre, type, 7000);
        }
    }

    // Charger les notifications quand le dropdown s'ouvre
    document.addEventListener('DOMContentLoaded', function() {
        const notificationDropdown = document.getElementById('notificationDropdown');
        if (notificationDropdown) {
            const dropdown = new bootstrap.Dropdown(notificationDropdown);
            notificationDropdown.addEventListener('shown.bs.dropdown', function() {
                loadNotifications();
            });
        }

        // Initialiser SignalR après le chargement de la page
        if (typeof signalR !== 'undefined') {
            initSignalR();
        } else {
            // Attendre que SignalR soit chargé
            window.addEventListener('load', function() {
                if (typeof signalR !== 'undefined') {
                    initSignalR();
                }
            });
        }
    });
})();


# 📘 PRD – Application de Gestion de Stock et de Vente – Version 2 (V2)

**Nom du produit** : Gestion Stock & Vente  
**Version** : V2  
**Technologie** : ASP.NET Core MVC (.NET 10) – C# – PostgreSQL – Entity Framework Core  
**Cible** : Réseau de points de vente **multi-boutiques** et **multi-dépôts**  
**Langue** : 100 % Français (métier, interface, base de données, documentation)

---

## 1. Contexte et acquis de la V1

La **V1** a livré un socle robuste couvrant :

* la gestion **multi-boutiques** (boutique active),
* la **caisse** avec scan (USB + caméra),
* la gestion des **stocks**, **achats** et **inventaires**,
* le **reporting** par boutique et consolidé.

La **V2** vise à **industrialiser et accélérer** l'exploitation : automatisation, temps réel, interopérabilité et contrôle renforcé.

---

## 2. Vision V2

🎯 **Vision**
Faire évoluer la solution vers une **plateforme de gestion commerciale avancée**, capable d'opérer à grande échelle (réseau de boutiques), d'anticiper les incidents (ruptures), et d'ouvrir le système aux paiements et intégrations modernes.

> La V2 ajoute **de l'intelligence opérationnelle** (alertes, transferts, planification) sans alourdir l'expérience utilisateur.

---

## 3. Objectifs V2

### Objectifs métiers

* Fluidifier la gestion **inter-dépôts** et **inter-boutiques**
* Réduire les ruptures via des alertes **proactives**
* Améliorer le pilotage réseau (comparaisons, tendances)
* Sécuriser davantage les opérations sensibles

### Objectifs utilisateurs

* Gagner du temps (moins d'actions manuelles)
* Être alerté avant les problèmes
* Accéder aux indicateurs clés en temps réel

---

## 4. Périmètre fonctionnel V2

### A. Multi-dépôts avancé & transferts

* Transferts de stock **entre dépôts** (même boutique) et **entre boutiques** (si autorisé)
* Workflow de transfert : création → validation → réception
* Traçabilité complète (sortie source / entrée destination)
* Historique et états des transferts

### B. Notifications & temps réel

* Alertes stock bas / rupture
* Alertes écarts d'inventaire significatifs
* Alertes annulation/avoir de vente
* Rafraîchissement temps réel des KPI (SignalR)
* Centre de notifications (lu / non lu)

### C. Rapports programmés

* Planification des rapports (journalier / hebdomadaire / mensuel)
* Envoi automatique par e-mail
* Rapports par boutique et **consolidés réseau**

### D. Paiements intégrés

* Intégration **Mobile Money**
* Intégration **Carte bancaire**
* Rapprochement des paiements
* Historique détaillé par boutique

### E. Sécurité & gouvernance avancées

* Journal détaillé des connexions
* Verrouillage temporaire après tentatives échouées
* Délégation temporaire de droits
* Traçabilité renforcée des actions critiques

---

## 5. Fonctionnalités détaillées

### 5.1 Transferts inter-dépôts

* Création d'un transfert (dépôt source → dépôt destination)
* Validation par rôle autorisé
* Mise à jour automatique des stocks
* Génération de mouvements de stock tracés

### 5.2 Notifications

* Tableau de notifications centralisé
* Notifications visibles à la connexion
* Paramétrage des seuils d'alerte par boutique

### 5.3 Reporting avancé

* Comparaison des performances entre boutiques
* Évolution du chiffre d'affaires (tendances)
* Analyse des marges (si prix d'achat disponibles)

### 5.4 Sécurité renforcée

* Historique détaillé des connexions (date, heure)
* Audit étendu des actions sensibles

---

## 6. Exigences non fonctionnelles V2

* Temps réel : latence < 2 secondes pour notifications
* Scalabilité : support de dizaines de boutiques
* Disponibilité élevée
* Sécurité renforcée (audits, contrôles d'accès)
* Journalisation complète

---

## 7. Architecture technique V2

* ASP.NET Core MVC (.NET 10)
* C# (full)
* PostgreSQL (indexation avancée)
* Entity Framework Core
* **SignalR** pour temps réel
* Services applicatifs dédiés (notifications, reporting)
* Architecture **Domaine / Infrastructure / Web** (en français)

---

## 8. Indicateurs de succès (KPI) V2

* Diminution des ruptures de stock
* Réduction du temps de traitement des opérations
* Taux d'utilisation des alertes
* Adoption des paiements intégrés

---

## 9. Compatibilité & migration

* Migration contrôlée V1 → V2
* Données existantes conservées
* Déploiement progressif par boutique

---

## 10. Roadmap post-V2

### Version 3 (préparation)

* API publique pour intégrations
* Application mobile dédiée (MAUI) avec mode hors ligne
* Analyses prédictives simples

---

## 11. Critères de validation V2

* Transferts inter-dépôts opérationnels et tracés
* Notifications temps réel fonctionnelles
* Rapports programmés reçus automatiquement
* Paiements intégrés opérationnels

---

✍️ **Conclusion**
La **V2** consolide l'application en une **solution de gestion commerciale mature**, prête pour l'expansion, l'automatisation et les intégrations modernes.


# 🔐 Permissions et Fonctionnalités - Manager Boutique

## 📋 Vue d'ensemble

Un **ManagerBoutique** peut gérer **uniquement sa boutique assignée**. Il a des droits étendus sur les opérations de sa boutique, mais ne peut pas :
- Créer de nouvelles boutiques
- Gérer les utilisateurs
- Accéder aux données d'autres boutiques

---

## ✅ FONCTIONNALITÉS AUTORISÉES

### 🏪 **Gestion de la Boutique**

#### ✅ **Consultation**
- **Voir la liste des boutiques** : Uniquement sa boutique (filtrage automatique)
- **Voir les détails de sa boutique** : Informations, dépôts associés
- **Changer de boutique** : Via le menu (si plusieurs boutiques assignées)

#### ✅ **Modification**
- **Modifier les paramètres de sa boutique** :
  - Nom, Adresse, Téléphone, Email
  - Logo
  - TVA par défaut
  - Statut actif/inactif

#### ❌ **Restrictions**
- **Ne peut PAS créer** de nouvelles boutiques (réservé à AdminReseau)
- **Ne peut PAS supprimer** de boutiques (soft delete réservé à AdminReseau)

---

### 📦 **Gestion des Produits**

#### ✅ **Consultation**
- **Voir tous les produits** : Uniquement ceux paramétrés pour sa boutique
- **Voir les détails d'un produit** : Informations complètes (SKU, code-barres, prix, seuils)

#### ✅ **Création**
- **Créer de nouveaux produits** :
  - Produit global (nom, description, catégorie)
  - Paramétrage boutique (SKU, code-barres, prix d'achat, prix de vente, seuil stock bas)
  - Automatiquement lié à sa boutique active

#### ✅ **Modification**
- **Modifier les produits** de sa boutique :
  - Informations globales (nom, description, catégorie)
  - Paramètres boutique (SKU, code-barres, prix, seuils)
  - Statut actif/inactif

#### ✅ **Recherche**
- Recherche par nom, SKU ou code-barres
- Prêt pour scanner USB/webcam

---

### 📂 **Gestion des Catégories**

#### ✅ **Toutes les opérations CRUD**
- **Créer** de nouvelles catégories
- **Voir** toutes les catégories
- **Modifier** les catégories existantes
- **Supprimer** les catégories (soft delete)

**Note** : Les catégories sont globales (partagées entre toutes les boutiques)

---

### 📊 **Gestion des Stocks**

#### ✅ **Consultation**
- **Voir les stocks** de tous les dépôts de sa boutique
- **Voir l'historique des mouvements** de stock
- **Alertes stock bas** : Produits en dessous du seuil

#### ✅ **Actions**
- **Créer des mouvements de stock** :
  - Entrées
  - Sorties
  - Ajustements
  - Pertes
  - Retours

#### ✅ **Mouvements**
- **Voir tous les mouvements** de sa boutique
- **Traçabilité complète** : Utilisateur, date, raison, dépôt

---

### 🏬 **Gestion des Dépôts**

#### ✅ **Toutes les opérations CRUD**
- **Créer** de nouveaux dépôts (uniquement pour sa boutique)
- **Voir** les dépôts de sa boutique
- **Modifier** les dépôts de sa boutique
- **Voir les détails** d'un dépôt
- **Supprimer** les dépôts (soft delete)

**Note** : Lors de la création, la boutique est automatiquement assignée (pas de choix pour ManagerBoutique)

---

### 🛒 **Gestion des Achats**

#### ✅ **Toutes les opérations**
- **Créer** de nouvelles commandes fournisseurs
- **Voir** toutes les commandes de sa boutique
- **Voir les détails** d'une commande
- **Réceptionner** les commandes (mise à jour automatique des stocks)
- **Modifier** les commandes (avant réception)

#### ✅ **Statuts**
- Gérer les statuts : En attente, En cours, Réceptionnée, Annulée

---

### 📋 **Gestion des Inventaires**

#### ✅ **Toutes les opérations**
- **Créer** de nouveaux inventaires
- **Voir** tous les inventaires de sa boutique
- **Voir les détails** d'un inventaire
- **Effectuer les comptages** (saisie des quantités)
- **Finaliser** les inventaires (ajustements automatiques des stocks)

#### ✅ **Statuts**
- Gérer les statuts : En cours, Finalisé, Annulé

---

### 🔄 **Gestion des Transferts Inter-Dépôts (V2)**

#### ✅ **Consultation**
- **Voir tous les transferts** de sa boutique (en tant que source ou destination)
- **Voir les détails** d'un transfert

#### ✅ **Création**
- **Créer des transferts** depuis les dépôts de sa boutique vers :
  - D'autres dépôts de sa boutique
  - D'autres boutiques (si autorisé)

#### ✅ **Validation**
- **Valider les transferts** sortants de sa boutique
- **Réceptionner les transferts** entrants dans sa boutique

#### ✅ **Statuts**
- Gérer les statuts : En attente, Validé, En transit, Reçu, Annulé

---

### 💰 **Point de Vente (Caisse)**

#### ✅ **Utilisation**
- **Utiliser la caisse** pour enregistrer des ventes
- **Gérer le panier** : Ajouter, modifier, supprimer des produits
- **Appliquer des remises**
- **Choisir le mode de paiement** : Espèces, Mobile Money, Carte
- **Finaliser les ventes**

#### ✅ **Consultation**
- **Voir l'historique des ventes** de sa boutique
- **Voir les détails** d'une vente
- **Rechercher** des ventes

#### ✅ **Annulation**
- **Annuler des ventes** (avec raison obligatoire)
- **Restauration automatique** des stocks lors de l'annulation

---

### 📈 **Rapports**

#### ✅ **Consultation**
- **Rapports de ventes** : De sa boutique uniquement
- **Rapports de stocks** : De sa boutique uniquement
- **Rapports d'achats** : De sa boutique uniquement
- **Exporter** les rapports (formats disponibles)

#### ✅ **Rapports Programmés (V2)**
- **Créer** des rapports programmés pour sa boutique
- **Voir** ses rapports programmés
- **Modifier** ses rapports programmés
- **Activer/Désactiver** les rapports programmés

---

### 💳 **Paiements Intégrés (V2)**

#### ✅ **Consultation**
- **Voir les paiements** de sa boutique
- **Voir les détails** d'un paiement
- **Historique** des transactions

#### ✅ **Gestion**
- **Configurer** les paramètres de paiement pour sa boutique
- **Gérer** les méthodes de paiement (Mobile Money, Carte)

---

### 🔔 **Notifications (V2)**

#### ✅ **Consultation**
- **Voir toutes les notifications** de sa boutique
- **Marquer comme lues**
- **Filtrer** par type de notification

#### ✅ **Types de notifications**
- Alertes stock bas
- Transferts reçus
- Achats en attente
- Inventaires en cours
- Rapports générés

---

### 📊 **Tableau de Bord**

#### ✅ **Vue d'ensemble**
- **KPIs de sa boutique** :
  - Ventes aujourd'hui
  - CA aujourd'hui
  - CA ce mois
  - Stocks bas
- **Graphiques** : Évolution des ventes (7 derniers jours)
- **Top produits** : Produits les plus vendus
- **Alertes** : Stocks bas, achats en attente, inventaires en cours

---

### ❓ **Aide**

#### ✅ **Accès**
- **Voir la page d'aide** avec guide spécifique ManagerBoutique
- **Accéder à la documentation** complète
- **Voir les workflows** et processus

---

## ❌ FONCTIONNALITÉS INTERDITES

### 🚫 **Gestion du Réseau**
- ❌ Créer de nouvelles boutiques
- ❌ Supprimer des boutiques
- ❌ Voir les données d'autres boutiques (isolation stricte)

### 🚫 **Gestion des Utilisateurs**
- ❌ Créer des utilisateurs
- ❌ Modifier des utilisateurs
- ❌ Assigner des rôles
- ❌ Voir la liste des utilisateurs

### 🚫 **Administration**
- ❌ Accéder au journal des connexions
- ❌ Gérer les paramètres système globaux
- ❌ Voir les rapports consolidés du réseau

---

## 🔒 **Isolation des Données**

### **Principe**
Toutes les données sont **automatiquement filtrées** par la boutique active du ManagerBoutique :

- ✅ **Produits** : Uniquement ceux paramétrés pour sa boutique
- ✅ **Stocks** : Uniquement les stocks de ses dépôts
- ✅ **Ventes** : Uniquement les ventes de sa boutique
- ✅ **Achats** : Uniquement les achats de sa boutique
- ✅ **Inventaires** : Uniquement les inventaires de sa boutique
- ✅ **Transferts** : Uniquement ceux où sa boutique est source ou destination
- ✅ **Rapports** : Uniquement les rapports de sa boutique

### **Sécurité**
- Les tentatives d'accès à des données d'autres boutiques sont **automatiquement bloquées**
- Les formulaires de création **forcent automatiquement** la boutique active
- Les listes sont **filtrées automatiquement** par la boutique active

---

## 📱 **Interface Utilisateur**

### **Sidebar (Menu latéral)**
Le ManagerBoutique voit :
- ✅ Tableau de bord
- ✅ Caisse
- ✅ Catalogue (Produits, Catégories)
- ✅ Stock (Stocks, Mouvements, Transferts, Inventaires, Dépôts)
- ✅ Achats (Fournisseurs, Commandes)
- ✅ Finances (Paiements intégrés)
- ✅ Ventes (Historique)
- ✅ Rapports (Ventes, Stocks, Achats, Rapports programmés)
- ✅ Boutiques (uniquement sa boutique)
- ✅ Notifications
- ✅ Aide

### **Carte d'accès rapide (Home)**
Le ManagerBoutique voit :
- ✅ Produits
- ✅ Ventes
- ✅ Stocks
- ❌ Boutiques (masquée, réservée à AdminReseau)

---

## 🎯 **Résumé des Permissions par Module**

| Module | Créer | Lire | Modifier | Supprimer | Notes |
|--------|-------|------|----------|-----------|-------|
| **Boutiques** | ❌ | ✅ (sa boutique) | ✅ (sa boutique) | ❌ | Ne peut pas créer |
| **Produits** | ✅ | ✅ (sa boutique) | ✅ (sa boutique) | ✅ | Soft delete |
| **Catégories** | ✅ | ✅ | ✅ | ✅ | Globales |
| **Dépôts** | ✅ | ✅ (sa boutique) | ✅ (sa boutique) | ✅ | Auto-assignés |
| **Stocks** | ✅ (mouvements) | ✅ (sa boutique) | ✅ (mouvements) | ❌ | Via mouvements |
| **Achats** | ✅ | ✅ (sa boutique) | ✅ (sa boutique) | ❌ | Avant réception |
| **Inventaires** | ✅ | ✅ (sa boutique) | ✅ (sa boutique) | ❌ | Via finalisation |
| **Transferts** | ✅ | ✅ (sa boutique) | ✅ (validation) | ❌ | Source/destination |
| **Ventes** | ✅ (caisse) | ✅ (sa boutique) | ❌ | ✅ (annulation) | Avec raison |
| **Fournisseurs** | ✅ | ✅ | ✅ | ✅ | Soft delete |
| **Rapports** | ✅ (programmés) | ✅ (sa boutique) | ✅ (programmés) | ✅ (programmés) | Export disponible |
| **Paiements** | ❌ | ✅ (sa boutique) | ✅ (config) | ❌ | Intégrés |
| **Utilisateurs** | ❌ | ❌ | ❌ | ❌ | Réservé AdminReseau |

---

## 📝 **Notes Importantes**

1. **Boutique Active** : Le ManagerBoutique doit avoir une boutique active assignée pour utiliser l'application
2. **Isolation Stricte** : Impossible d'accéder aux données d'autres boutiques
3. **Soft Delete** : Les suppressions sont logiques (pas de suppression physique en base)
4. **Traçabilité** : Toutes les actions sont tracées (utilisateur, date, boutique)
5. **Validation** : Certaines actions nécessitent une validation (transferts, inventaires)

---

**Dernière mise à jour** : Basé sur l'analyse du code source actuel (V1 + V2)


# 📖 Guide d'Utilisation - Gestion Stock & Vente

## Table des matières
1. [Présentation](#1-présentation)
2. [Première connexion](#2-première-connexion)
3. [Navigation dans l'application](#3-navigation-dans-lapplication)
4. [Profils utilisateurs et permissions](#4-profils-utilisateurs-et-permissions)
5. [Mode d'utilisation par profil](#5-mode-dutilisation-par-profil)
6. [Scénarios d'utilisation courants](#6-scénarios-dutilisation-courants)
7. [Fonctionnalités détaillées](#7-fonctionnalités-détaillées)
8. [FAQ et dépannage](#8-faq-et-dépannage)

---

## 1. Présentation

**Gestion Stock & Vente** est une application web professionnelle de gestion de stock et de vente multi-boutiques, conçue pour un usage quotidien intensif en boutique.

### Caractéristiques principales
- ✅ Multi-boutiques avec isolation des données
- ✅ Point de vente (POS) optimisé pour scanner
- ✅ Gestion complète des stocks par dépôt
- ✅ Suivi des achats et réceptions
- ✅ Inventaires avec ajustements automatiques
- ✅ Rapports et tableaux de bord
- ✅ Interface moderne et intuitive

### Navigateurs supportés
- Chrome/Edge (recommandé)
- Firefox
- Safari

---

## 2. Première connexion

### 2.1 Accès à l'application

1. Ouvrez votre navigateur
2. Accédez à l'URL de l'application (fournie par votre administrateur)
3. Vous arrivez sur la page de connexion

### 2.2 Connexion initiale

```
┌─────────────────────────────────────┐
│  Page de connexion                 │
│  ───────────────────────────────    │
│  Email : [votre@email.com]         │
│  Mot de passe : [mot de passe]      │
│  [Se connecter]                     │
└─────────────────────────────────────┘
```

**Étapes :**
1. Saisissez votre **email** (fourni par l'administrateur)
2. Saisissez votre **mot de passe temporaire** (fourni par l'administrateur)
3. Cliquez sur **"Se connecter"**

### 2.3 Changement de mot de passe obligatoire

Si c'est votre première connexion, vous serez automatiquement redirigé vers la page de changement de mot de passe.

```
┌─────────────────────────────────────┐
│  ⚠️ Changement de mot de passe      │
│  ───────────────────────────────    │
│  Mot de passe actuel : [••••••]     │
│  Nouveau mot de passe : [••••••]    │
│  Confirmer : [••••••]                │
│  [Enregistrer]                      │
└─────────────────────────────────────┘
```

**Règles du mot de passe :**
- Minimum 6 caractères
- Les deux champs doivent correspondre

**Après le changement :**
- Vous êtes automatiquement redirigé vers le tableau de bord
- Vous ne serez plus demandé de changer votre mot de passe

### 2.4 Sélection de la boutique active

Si vous n'avez pas de boutique active configurée, vous serez redirigé vers la page de sélection de boutique.

```
┌─────────────────────────────────────┐
│  Sélectionner une boutique          │
│  ───────────────────────────────    │
│  [Carte Boutique 1]                 │
│  [Carte Boutique 2]                 │
│  [Carte Boutique 3]                 │
└─────────────────────────────────────┘
```

**Étapes :**
1. Cliquez sur la carte de la boutique où vous travaillez
2. La boutique devient votre boutique active
3. Vous êtes redirigé vers le tableau de bord

**Note :** Les administrateurs réseau peuvent voir toutes les boutiques et changer de boutique à tout moment.

---

## 3. Navigation dans l'application

### 3.1 Structure de l'interface

```
┌─────────────────────────────────────────────────────────────┐
│ [Menu]  [Boutique Active]  [👤 Utilisateur]  [🔔 Notif]     │ ← Topbar
├─────────┬───────────────────────────────────────────────────┤
│         │                                                   │
│ Sidebar │              Contenu principal                    │
│         │                                                   │
│ 📊 Dash │                                                   │
│ 💰 Caisse│                                                   │
│ 📦 Produits│                                                 │
│ 📊 Stock │                                                   │
│ 🛒 Achats│                                                   │
│ 📋 Inventaires│                                               │
│ 📈 Rapports│                                                 │
│ 🏪 Boutiques│                                                │
│ ⚙️ Paramètres│                                               │
│         │                                                   │
└─────────┴───────────────────────────────────────────────────┘
```

### 3.2 Barre latérale (Sidebar)

La sidebar contient les modules principaux :

| Icône | Module | Description |
|-------|--------|-------------|
| 📊 | **Tableau de bord** | Vue d'ensemble et KPIs |
| 💰 | **Caisse** | Point de vente (POS) |
| 📦 | **Produits** | Catalogue des produits |
| 📊 | **Stock** | Gestion des stocks |
| 🛒 | **Achats** | Commandes et réceptions |
| 📋 | **Inventaires** | Comptages et ajustements |
| 📈 | **Rapports** | Analyses et exports |
| 🏪 | **Boutiques** | Gestion des boutiques (Admin) |
| ⚙️ | **Paramètres** | Configuration |

**État actif :** Le module actuellement ouvert est surligné en bleu.

### 3.3 Barre supérieure (Topbar)

- **Menu** (mobile) : Affiche/masque la sidebar sur mobile
- **Boutique active** : Affiche le nom de la boutique active (cliquable pour changer)
- **Utilisateur** : Menu déroulant avec :
  - Votre nom
  - "Changer de boutique"
  - "Déconnexion"
- **Notifications** : Alertes et notifications (à venir)

### 3.4 Changement de boutique

**Méthode 1 : Via le topbar**
1. Cliquez sur le nom de la boutique active (chip bleu)
2. Sélectionnez une autre boutique dans la liste
3. Confirmez

**Méthode 2 : Via le menu utilisateur**
1. Cliquez sur votre nom (topbar droite)
2. Cliquez sur "Changer de boutique"
3. Sélectionnez la boutique souhaitée

**Note :** Seuls les **Admin Réseau** peuvent changer de boutique. Les autres utilisateurs ont une boutique fixe.

---

## 4. Profils utilisateurs et permissions

### 4.1 Admin Réseau

**Accès :**
- ✅ Toutes les boutiques
- ✅ Tous les modules
- ✅ Gestion des utilisateurs
- ✅ Gestion des boutiques
- ✅ Rapports consolidés

**Restrictions :**
- Aucune restriction

### 4.2 Manager Boutique

**Accès :**
- ✅ Sa boutique uniquement
- ✅ Produits (création/modification)
- ✅ Ventes (consultation/annulation)
- ✅ Stocks (consultation)
- ✅ Rapports de sa boutique

**Restrictions :**
- ❌ Ne peut pas créer de produits pour d'autres boutiques
- ❌ Ne peut pas modifier les produits d'autres boutiques
- ❌ Ne peut pas voir les données d'autres boutiques

### 4.3 Caissier

**Accès :**
- ✅ Caisse (ventes uniquement)
- ✅ Consultation des ventes
- ✅ Consultation des produits

**Restrictions :**
- ❌ Pas d'accès à la création/modification de produits
- ❌ Pas d'accès aux stocks
- ❌ Pas d'accès aux achats
- ❌ Pas d'accès aux inventaires

### 4.4 Gestionnaire Stock

**Accès :**
- ✅ Produits (création/modification)
- ✅ Stocks (consultation/ajustements)
- ✅ Achats (création/réception)
- ✅ Inventaires (création/comptage)

**Restrictions :**
- ❌ Pas d'accès aux ventes
- ❌ Sa boutique uniquement

---

## 5. Mode d'utilisation par profil

### 5.1 Scénario : Caissier - Vente en caisse

**Objectif :** Enregistrer une vente rapidement avec un scanner

**Étapes détaillées :**

1. **Connexion**
   - Email + Mot de passe
   - Sélection boutique (si nécessaire)

2. **Accès à la caisse**
   - Cliquez sur **"💰 Caisse"** dans la sidebar
   - Le champ scanner est automatiquement en focus

3. **Scanner un produit**
   ```
   ┌─────────────────────────────────────┐
   │  Scanner code-barres : [________]    │ ← Focus automatique
   └─────────────────────────────────────┘
   ```
   - Scannez le code-barres OU
   - Saisissez le code-barres manuellement
   - Appuyez sur **Entrée**

4. **Produit trouvé**
   - Le produit s'ajoute automatiquement au panier
   - Quantité par défaut : 1
   - Le champ scanner reprend le focus automatiquement

5. **Modifier la quantité**
   - Dans le panier, cliquez sur le champ quantité
   - Modifiez la valeur
   - Appuyez sur **Entrée** ou cliquez ailleurs

6. **Ajouter d'autres produits**
   - Répétez l'étape 3 pour chaque produit

7. **Voir le total**
   ```
   ┌─────────────────────────────────────┐
   │  Total HT : 150,00 €                │
   │  TVA : 30,00 €                      │
   │  Total TTC : 180,00 €               │
   └─────────────────────────────────────┘
   ```

8. **Encaisser**
   - Cliquez sur **"Encaisser"**
   - Modal de confirmation :
     ```
     ┌─────────────────────────────────────┐
     │  Confirmer la vente ?              │
     │  Montant : 180,00 €                │
     │  Mode de paiement :                │
     │  ○ Espèces                         │
     │  ○ MobileMoney                     │
     │  ○ Carte                           │
     │  [Annuler]  [Valider]              │
     └─────────────────────────────────────┘
     ```
   - Sélectionnez le mode de paiement
   - Cliquez sur **"Valider"**

9. **Vente enregistrée**
   - Message de succès affiché
   - Panier vidé automatiquement
   - Focus retourne au champ scanner
   - Prêt pour la vente suivante

**Astuces :**
- Le scanner fonctionne en continu : scannez → Entrée → scannez → Entrée
- Pour supprimer un article du panier, cliquez sur l'icône poubelle
- Pour vider tout le panier, cliquez sur "Vider le panier"

### 5.2 Scénario : Manager Boutique - Créer un produit

**Objectif :** Ajouter un nouveau produit au catalogue de la boutique

**Étapes détaillées :**

1. **Accès au module Produits**
   - Cliquez sur **"📦 Produits"** dans la sidebar
   - Vous voyez la liste des produits de votre boutique

2. **Créer un nouveau produit**
   - Cliquez sur **"Nouveau produit"** (bouton en haut à droite)

3. **Remplir le formulaire**

   **Section 1 : Informations produit global**
   ```
   Nom * : [Produit ABC]
   Description : [Description du produit]
   Catégorie * : [Sélectionner catégorie ▼]
   ```

   **Section 2 : Paramètres boutique**
   ```
   Boutique * : [Votre boutique] (pré-sélectionnée)
   SKU : [SKU-001]
   Code-barres : [1234567890123]
   Prix achat * : [10,00]
   Prix vente * : [15,00]
   Seuil stock bas : [5]
   Actif : [✓]
   ```

4. **Valider**
   - Cliquez sur **"Enregistrer"**
   - Message de succès
   - Redirection vers la liste des produits

**Notes importantes :**
- Les champs marqués * sont obligatoires
- Le code-barres doit être unique pour votre boutique
- Le prix de vente doit être supérieur au prix d'achat (recommandé)
- Le seuil stock bas déclenche une alerte quand le stock atteint ce niveau

### 5.3 Scénario : Gestionnaire Stock - Réceptionner un achat

**Objectif :** Réceptionner une commande d'achat et mettre à jour les stocks

**Étapes détaillées :**

1. **Accès au module Achats**
   - Cliquez sur **"🛒 Achats"** dans la sidebar
   - Vous voyez la liste des achats

2. **Créer une commande d'achat** (si nécessaire)
   - Cliquez sur **"Nouvel achat"**
   - Remplissez :
     ```
     Fournisseur * : [Sélectionner fournisseur ▼]
     Date commande : [Date]
     Date livraison prévue : [Date]
     Notes : [Notes optionnelles]
     ```
   - Ajoutez les lignes de produits :
     ```
     Produit | Quantité | Prix unitaire | Total
     ────────────────────────────────────────
     [Sélectionner] | [10] | [5,00] | 50,00 €
     ```
   - Cliquez sur **"Enregistrer"**

3. **Réceptionner l'achat**
   - Dans la liste, cliquez sur **"Actions"** → **"Réceptionner"**
   - Pour chaque ligne, indiquez :
     ```
     Produit : Produit ABC
     Quantité commandée : 10
     Quantité reçue * : [10]
     Dépôt de réception * : [Sélectionner dépôt ▼]
     Prix achat : [5,00] (modifiable)
     ```
   - Cliquez sur **"Valider la réception"**

4. **Résultat**
   - Statut de l'achat passe à "Réceptionné"
   - Les stocks sont automatiquement mis à jour
   - Les mouvements de stock sont enregistrés
   - Message de succès affiché

**Automatismes :**
- ✅ Mise à jour automatique des stocks
- ✅ Création automatique des mouvements de stock (Type: Entrée)
- ✅ Mise à jour du prix d'achat du produit (si différent)

### 5.4 Scénario : Gestionnaire Stock - Effectuer un inventaire

**Objectif :** Compter les stocks réels et ajuster les écarts

**Étapes détaillées :**

1. **Démarrer un inventaire**
   - Cliquez sur **"📋 Inventaires"** dans la sidebar
   - Cliquez sur **"Démarrer un inventaire"**

2. **Paramétrer l'inventaire**
   ```
   Dépôt * : [Sélectionner dépôt ▼]
   Date inventaire : [Date]
   Notes : [Notes optionnelles]
   ```
   - Cliquez sur **"Démarrer"**

3. **Compter les produits**
   - Pour chaque produit :
     - **Méthode 1 : Scanner**
       - Scannez le code-barres
       - Saisissez la quantité réelle
     - **Méthode 2 : Saisie manuelle**
       - Recherchez le produit
       - Saisissez la quantité réelle
   
   ```
   Produit : Produit ABC
   Quantité théorique : 10
   Quantité réelle * : [8]
   Écart : -2 (automatique)
   ```

4. **Finaliser l'inventaire**
   - Une fois tous les produits comptés, cliquez sur **"Finaliser"**
   - Vérifiez le résumé des écarts :
     ```
     ┌─────────────────────────────────────┐
     │  Résumé des écarts                  │
     │  ───────────────────────────────    │
     │  Produit ABC : -2                   │
     │  Produit XYZ : +1                   │
     │  Total écarts : -1                  │
     │  [Annuler]  [Confirmer]             │
     └─────────────────────────────────────┘
     ```

5. **Confirmer**
   - Cliquez sur **"Confirmer"**
   - Les stocks sont automatiquement ajustés
   - Les mouvements de stock sont créés (Type: Ajustement)
   - L'inventaire est clôturé

**Notes :**
- Vous pouvez reprendre un inventaire en cours
- Les écarts sont calculés automatiquement
- Les ajustements sont tracés dans l'historique

### 5.5 Scénario : Manager Boutique - Consulter les rapports

**Objectif :** Analyser les performances de la boutique

**Étapes détaillées :**

1. **Accès aux rapports**
   - Cliquez sur **"📈 Rapports"** dans la sidebar
   - Vous voyez les différents types de rapports disponibles

2. **Rapport des ventes**
   - Cliquez sur **"Rapport des ventes"**
   - Définissez les filtres :
     ```
     Date début : [01/01/2024]
     Date fin : [31/01/2024]
     Boutique : [Votre boutique] (pré-sélectionnée)
     ```
   - Cliquez sur **"Appliquer"**
   - Résultats affichés :
     ```
     ┌─────────────────────────────────────┐
     │  Statistiques                       │
     │  ───────────────────────────────    │
     │  Nombre de ventes : 150             │
     │  Total HT : 15 000,00 €            │
     │  Total TVA : 3 000,00 €            │
     │  Total TTC : 18 000,00 €           │
     └─────────────────────────────────────┘
     
     Liste des ventes :
     [Tableau détaillé]
     ```

3. **Export (si disponible)**
   - Cliquez sur **"Exporter en CSV"**
   - Le fichier est téléchargé

---

## 6. Scénarios d'utilisation courants

### 6.1 Vente avec produit introuvable

**Situation :** Le code-barres scanné n'existe pas dans le système

**Solution :**
1. Un message d'erreur s'affiche : "Produit non trouvé"
2. Options :
   - **Option 1 :** Créer le produit rapidement (si vous avez les droits)
   - **Option 2 :** Saisir manuellement le nom du produit
   - **Option 3 :** Ignorer et continuer avec d'autres produits

### 6.2 Ajustement de stock manuel

**Situation :** Vous constatez un écart de stock (perte, casse, retour)

**Solution :**
1. Allez dans **"📊 Stock"** → **"Actions"** → **"Ajuster"**
2. Sélectionnez le type de mouvement :
   - **Ajustement** : Correction d'erreur
   - **Perte** : Produit perdu/cassé
   - **Retour** : Retour fournisseur
3. Saisissez la quantité (négative pour diminuer)
4. Ajoutez une raison
5. Validez

**Résultat :**
- Stock mis à jour automatiquement
- Mouvement enregistré dans l'historique

### 6.3 Annulation d'une vente

**Situation :** Un client demande l'annulation d'une vente

**Solution :**
1. Allez dans **"💰 Ventes"** (liste des ventes)
2. Trouvez la vente à annuler
3. Cliquez sur **"Actions"** → **"Annuler"**
4. Confirmez dans la modal
5. La vente est annulée et les stocks sont restaurés automatiquement

**Conditions :**
- La vente doit être au statut "Enregistrée"
- Les stocks sont automatiquement restaurés
- Un mouvement de stock (Type: Retour) est créé

### 6.4 Consultation des stocks bas

**Situation :** Vous voulez voir les produits en rupture de stock

**Solution :**
1. Allez dans **"📊 Stock"**
2. Activez le filtre **"Stock bas uniquement"**
3. La liste affiche uniquement les produits dont le stock ≤ seuil
4. Les produits sont surlignés en rouge/orange

**Astuce :** Le tableau de bord affiche aussi une alerte "Stocks bas" avec le nombre de produits concernés.

### 6.5 Recherche d'un produit

**Situation :** Vous cherchez un produit spécifique

**Méthode 1 : Dans la liste des produits**
1. Allez dans **"📦 Produits"**
2. Utilisez le champ de recherche :
   - Recherche par **nom**
   - Recherche par **SKU**
   - Recherche par **code-barres**
3. Les résultats se filtrent automatiquement

**Méthode 2 : Dans la caisse**
- Le scanner recherche automatiquement par code-barres
- Si le produit existe, il est ajouté au panier

---

## 7. Fonctionnalités détaillées

### 7.1 Point de vente (Caisse)

**Caractéristiques :**
- ✅ Focus automatique sur le champ scanner
- ✅ Recherche instantanée par code-barres
- ✅ Panier en temps réel avec totaux
- ✅ Modification de quantité inline
- ✅ Calcul automatique HT/TVA/TTC
- ✅ Support multi-modes de paiement
- ✅ Interface optimisée pour la vitesse

**Raccourcis clavier :**
- **Entrée** : Valider le code-barres scanné
- **Tab** : Navigation entre les champs
- **Escape** : Annuler/Retour

### 7.2 Gestion des produits

**Structure :**
- **Produit global** : Nom, description, catégorie (partagé entre toutes les boutiques)
- **Paramètres boutique** : SKU, code-barres, prix, seuil (spécifique à chaque boutique)

**Avantages :**
- Un même produit peut avoir des prix différents par boutique
- Des codes-barres différents par boutique
- Gestion centralisée du catalogue

### 7.3 Gestion des stocks

**Niveaux de stock :**
- **Stock par dépôt** : Chaque produit peut avoir un stock dans chaque dépôt
- **Stock total** : Somme des stocks de tous les dépôts de la boutique

**Mouvements automatiques :**
- ✅ **Sortie** : Lors d'une vente
- ✅ **Entrée** : Lors d'une réception d'achat
- ✅ **Ajustement** : Lors d'un inventaire
- ✅ **Retour** : Lors d'une annulation de vente
- ✅ **Perte** : Lors d'un ajustement manuel de type "Perte"

### 7.4 Rapports

**Types de rapports disponibles :**

1. **Rapport des ventes**
   - Filtres : Date, boutique
   - Statistiques : Nombre, totaux HT/TVA/TTC
   - Détails : Liste des ventes

2. **Rapport des stocks**
   - Filtres : Boutique, dépôt, stock bas
   - Statistiques : Valeur totale, nombre de lignes
   - Détails : Liste des stocks

3. **Rapport des achats**
   - Filtres : Date, boutique, fournisseur, statut
   - Statistiques : Nombre, totaux
   - Détails : Liste des achats

**Exports :**
- Format CSV (à venir)
- Impression (à venir)

---

## 8. FAQ et dépannage

### 8.1 Problèmes de connexion

**Q : Je ne peux pas me connecter**
- Vérifiez votre email et mot de passe
- Contactez votre administrateur pour réinitialiser votre mot de passe

**Q : Je suis bloqué sur la page de changement de mot de passe**
- Assurez-vous que les deux champs de mot de passe correspondent
- Le mot de passe doit faire au moins 6 caractères

### 8.2 Problèmes de boutique

**Q : Je ne vois pas de boutique active**
- Contactez votre administrateur pour vous assigner une boutique
- Si vous êtes Admin Réseau, sélectionnez une boutique dans le menu

**Q : Je veux changer de boutique**
- Seuls les Admin Réseau peuvent changer de boutique
- Utilisez le menu utilisateur → "Changer de boutique"

### 8.3 Problèmes de scanner

**Q : Le scanner ne fonctionne pas**
- Vérifiez que le champ scanner a le focus (cliquez dedans)
- Le scanner doit envoyer un "Entrée" après le code-barres
- Testez avec une saisie manuelle du code-barres

**Q : Le produit n'est pas trouvé**
- Vérifiez que le code-barres existe dans le système
- Vérifiez que le produit est actif
- Vérifiez que le produit appartient à votre boutique

### 8.4 Problèmes de permissions

**Q : Je ne peux pas créer de produit**
- Vérifiez que vous avez le rôle "ManagerBoutique" ou "GestionnaireStock"
- Contactez votre administrateur

**Q : Je ne vois que ma boutique**
- C'est normal ! L'isolation des données est une fonctionnalité de sécurité
- Seuls les Admin Réseau voient toutes les boutiques

### 8.5 Problèmes de performance

**Q : L'application est lente**
- Vérifiez votre connexion internet
- Fermez les onglets inutiles
- Contactez le support technique

**Q : Les données ne se chargent pas**
- Actualisez la page (F5)
- Vérifiez votre connexion internet
- Contactez le support technique

---

## 9. Bonnes pratiques

### 9.1 Pour les caissiers

- ✅ Scannez systématiquement les codes-barres (plus rapide)
- ✅ Vérifiez les totaux avant d'encaisser
- ✅ Utilisez le bon mode de paiement
- ✅ En cas d'erreur, annulez la vente et recommencez

### 9.2 Pour les gestionnaires de stock

- ✅ Effectuez des inventaires réguliers
- ✅ Réceptionnez les achats dès l'arrivée
- ✅ Documentez les ajustements (raison obligatoire)
- ✅ Surveillez les alertes de stock bas

### 9.3 Pour les managers

- ✅ Vérifiez les rapports quotidiennement
- ✅ Créez les produits avec tous les détails (code-barres, prix)
- ✅ Configurez les seuils de stock bas correctement
- ✅ Formez les utilisateurs sur les bonnes pratiques

---

## 10. Support et contact

### 10.1 Assistance technique

- **Email support :** support@geststockvente.com
- **Téléphone :** (à définir)
- **Horaires :** (à définir)

### 10.2 Documentation

- **Guide utilisateur :** Ce document
- **Workflow complet :** Voir `WORKFLOW_COMPLET.md`
- **Documentation technique :** (à venir)

---

**Version du guide :** 1.0  
**Dernière mise à jour :** 2024  
**Application :** Gestion Stock & Vente V1.0


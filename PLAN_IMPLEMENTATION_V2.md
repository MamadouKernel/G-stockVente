# 📋 Plan d'Implémentation V2 - Gestion Stock & Vente

## Vue d'ensemble

Ce document détaille le plan d'implémentation des fonctionnalités V2, organisé par priorité et dépendances.

---

## 🎯 Priorités d'implémentation

### Phase 1 : Fondations (Priorité Haute)
1. **Modèles de données** pour les nouvelles entités
2. **Migrations de base de données**
3. **Services de base** (notifications, transferts)

### Phase 2 : Transferts (Priorité Haute)
1. **Transferts inter-dépôts**
2. **Transferts inter-boutiques**
3. **Workflow de validation**

### Phase 3 : Notifications (Priorité Haute)
1. **Système de notifications**
2. **SignalR pour temps réel**
3. **Centre de notifications**

### Phase 4 : Rapports programmés (Priorité Moyenne)
1. **Planification des rapports**
2. **Envoi automatique par email**

### Phase 5 : Paiements intégrés (Priorité Moyenne)
1. **Intégration Mobile Money**
2. **Intégration Carte bancaire**
3. **Rapprochement**

### Phase 6 : Sécurité avancée (Priorité Basse)
1. **Journal des connexions**
2. **Verrouillage temporaire**
3. **Audit étendu**

---

## 📊 Modèles de données à créer

### 1. TransfertStock

```csharp
public class TransfertStock
{
    public Guid Id { get; set; }
    public string Numero { get; set; } // Format: TRANSFERT-{BoutiqueId}-{Numéro}
    
    // Source
    public Guid DepotSourceId { get; set; }
    public Guid? BoutiqueSourceId { get; set; } // Pour transferts inter-boutiques
    
    // Destination
    public Guid DepotDestinationId { get; set; }
    public Guid? BoutiqueDestinationId { get; set; }
    
    public StatutTransfert Statut { get; set; } // EnAttente, Valide, EnTransit, Reçu, Annule
    
    public Guid? UtilisateurCreateurId { get; set; }
    public Guid? UtilisateurValidateurId { get; set; }
    public Guid? UtilisateurRecepteurId { get; set; }
    
    public DateTime DateCreation { get; set; }
    public DateTime? DateValidation { get; set; }
    public DateTime? DateReception { get; set; }
    public DateTime? DateAnnulation { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation
    public virtual Depot DepotSource { get; set; }
    public virtual Depot DepotDestination { get; set; }
    public virtual Boutique? BoutiqueSource { get; set; }
    public virtual Boutique? BoutiqueDestination { get; set; }
    public virtual ApplicationUser? UtilisateurCreateur { get; set; }
    public virtual ApplicationUser? UtilisateurValidateur { get; set; }
    public virtual ApplicationUser? UtilisateurRecepteur { get; set; }
    public virtual ICollection<LigneTransfertStock> LignesTransfert { get; set; }
}

public class LigneTransfertStock
{
    public Guid Id { get; set; }
    public Guid TransfertStockId { get; set; }
    public Guid ProduitBoutiqueId { get; set; }
    public int Quantite { get; set; }
    public int? QuantiteRecue { get; set; } // Peut différer de Quantite
    
    // Navigation
    public virtual TransfertStock TransfertStock { get; set; }
    public virtual ProduitBoutique ProduitBoutique { get; set; }
}

public enum StatutTransfert
{
    EnAttente = 0,
    Valide = 1,
    EnTransit = 2,
    Reçu = 3,
    Annule = 4
}
```

### 2. Notification

```csharp
public class Notification
{
    public Guid Id { get; set; }
    public Guid? UtilisateurId { get; set; } // null = notification globale
    public Guid? BoutiqueId { get; set; } // null = notification réseau
    
    public TypeNotification Type { get; set; }
    public string Titre { get; set; }
    public string Message { get; set; }
    public string? LienAction { get; set; } // URL vers l'action à effectuer
    
    public bool EstLue { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateLecture { get; set; }
    
    // Navigation
    public virtual ApplicationUser? Utilisateur { get; set; }
    public virtual Boutique? Boutique { get; set; }
}

public enum TypeNotification
{
    StockBas = 0,
    RuptureStock = 1,
    EcartInventaire = 2,
    AnnulationVente = 3,
    TransfertEnAttente = 4,
    TransfertRecu = 5,
    RapportDisponible = 6,
    Autre = 99
}
```

### 3. RapportProgramme

```csharp
public class RapportProgramme
{
    public Guid Id { get; set; }
    public Guid? BoutiqueId { get; set; } // null = rapport consolidé réseau
    
    public TypeRapport TypeRapport { get; set; }
    public FrequenceRapport Frequence { get; set; } // Journalier, Hebdomadaire, Mensuel
    
    public string EmailDestinataire { get; set; }
    public bool EstActif { get; set; }
    
    public DateTime DateCreation { get; set; }
    public DateTime? DerniereExecution { get; set; }
    public DateTime? ProchaineExecution { get; set; }
    
    // Navigation
    public virtual Boutique? Boutique { get; set; }
}

public enum TypeRapport
{
    Ventes = 0,
    Stocks = 1,
    Achats = 2,
    Consolidé = 3
}

public enum FrequenceRapport
{
    Journalier = 0,
    Hebdomadaire = 1,
    Mensuel = 2
}
```

### 4. JournalConnexion

```csharp
public class JournalConnexion
{
    public Guid Id { get; set; }
    public Guid? UtilisateurId { get; set; } // null si échec de connexion
    
    public string Email { get; set; } // Email utilisé pour la tentative
    public string AdresseIP { get; set; }
    public bool Succes { get; set; }
    public string? RaisonEchec { get; set; }
    
    public DateTime DateTentative { get; set; }
    
    // Navigation
    public virtual ApplicationUser? Utilisateur { get; set; }
}
```

### 5. PaiementIntegre

```csharp
public class PaiementIntegre
{
    public Guid Id { get; set; }
    public Guid VenteId { get; set; }
    
    public TypePaiementIntegre Type { get; set; } // MobileMoney, CarteBancaire
    public string ReferenceExterne { get; set; } // Référence du paiement externe
    public decimal Montant { get; set; }
    
    public StatutPaiement Statut { get; set; } // EnAttente, Valide, Echec, Rembourse
    
    public DateTime DateCreation { get; set; }
    public DateTime? DateValidation { get; set; }
    public string? DonneesReponse { get; set; } // JSON de la réponse du prestataire
    
    // Navigation
    public virtual Vente Vente { get; set; }
}

public enum TypePaiementIntegre
{
    MobileMoney = 0,
    CarteBancaire = 1
}

public enum StatutPaiement
{
    EnAttente = 0,
    Valide = 1,
    Echec = 2,
    Rembourse = 3
}
```

---

## 🏗️ Architecture des services

### Services à créer

1. **ITransfertStockService**
   - Créer un transfert
   - Valider un transfert
   - Recevoir un transfert
   - Annuler un transfert
   - Mettre à jour les stocks automatiquement

2. **INotificationService**
   - Créer une notification
   - Marquer comme lue
   - Obtenir les notifications non lues
   - Envoyer via SignalR

3. **IRapportProgrammeService**
   - Créer un rapport programmé
   - Exécuter les rapports programmés (background service)
   - Envoyer par email

4. **IPaiementIntegreService**
   - Initier un paiement Mobile Money
   - Initier un paiement Carte bancaire
   - Vérifier le statut d'un paiement
   - Rapprocher les paiements

5. **IJournalConnexionService**
   - Enregistrer une tentative de connexion
   - Obtenir l'historique des connexions
   - Détecter les tentatives suspectes

---

## 📁 Structure des fichiers à créer

### Domain/Models
- `TransfertStock.cs`
- `LigneTransfertStock.cs`
- `Notification.cs`
- `RapportProgramme.cs`
- `JournalConnexion.cs`
- `PaiementIntegre.cs`
- `Enums/StatutTransfert.cs`
- `Enums/TypeNotification.cs`
- `Enums/TypeRapport.cs`
- `Enums/FrequenceRapport.cs`
- `Enums/TypePaiementIntegre.cs`
- `Enums/StatutPaiement.cs`

### Infrastructure/Services
- `TransfertStockService.cs`
- `NotificationService.cs`
- `RapportProgrammeService.cs`
- `PaiementIntegreService.cs`
- `JournalConnexionService.cs`
- `SignalRHub.cs` (pour les notifications temps réel)

### Infrastructure/BackgroundServices
- `RapportProgrammeBackgroundService.cs` (service en arrière-plan pour exécuter les rapports)

### Controllers
- `TransfertsController.cs`
- `NotificationsController.cs`
- `RapportsProgrammesController.cs`
- `PaiementsIntegresController.cs`
- `JournalConnexionController.cs`

### Views
- `Transferts/Index.cshtml`
- `Transferts/Create.cshtml`
- `Transferts/Details.cshtml`
- `Transferts/Valider.cshtml`
- `Transferts/Recevoir.cshtml`
- `Notifications/Index.cshtml`
- `RapportsProgrammes/Index.cshtml`
- `RapportsProgrammes/Create.cshtml`
- `PaiementsIntegres/Index.cshtml`
- `JournalConnexion/Index.cshtml`

---

## 🔄 Workflow des transferts

### Transfert inter-dépôts (même boutique)

```
1. Création
   - Sélection dépôt source
   - Sélection dépôt destination (même boutique)
   - Ajout des lignes (produit, quantité)
   - Statut: EnAttente

2. Validation
   - Vérification des stocks disponibles
   - Sortie du stock source
   - Création MouvementStock (Type: Sortie)
   - Statut: Valide

3. Réception
   - Confirmation de réception
   - Entrée dans le stock destination
   - Création MouvementStock (Type: Entrée)
   - Statut: Reçu
```

### Transfert inter-boutiques

```
1. Création
   - Sélection boutique source
   - Sélection dépôt source
   - Sélection boutique destination
   - Sélection dépôt destination
   - Ajout des lignes
   - Statut: EnAttente

2. Validation (boutique source)
   - Vérification stocks
   - Sortie du stock source
   - Statut: Valide

3. Réception (boutique destination)
   - Confirmation de réception
   - Vérification que le produit existe dans la boutique destination
   - Si non: création ProduitBoutique automatique
   - Entrée dans le stock destination
   - Statut: Reçu
```

---

## 🔔 Système de notifications

### Types de notifications

1. **Stock bas** : Quand quantité ≤ seuil
2. **Rupture stock** : Quand quantité = 0
3. **Écart inventaire** : Quand écart > seuil configuré
4. **Annulation vente** : Quand une vente est annulée
5. **Transfert en attente** : Quand un transfert nécessite validation
6. **Transfert reçu** : Quand un transfert est reçu
7. **Rapport disponible** : Quand un rapport programmé est généré

### Implémentation SignalR

```csharp
public class NotificationHub : Hub
{
    // Envoyer une notification à un utilisateur spécifique
    public async Task SendNotificationToUser(string userId, Notification notification)
    
    // Envoyer une notification à tous les utilisateurs d'une boutique
    public async Task SendNotificationToBoutique(string boutiqueId, Notification notification)
    
    // Envoyer une notification globale (tous les utilisateurs)
    public async Task SendGlobalNotification(Notification notification)
}
```

---

## 📧 Rapports programmés

### Configuration

- Type de rapport (Ventes, Stocks, Achats, Consolidé)
- Fréquence (Journalier, Hebdomadaire, Mensuel)
- Email destinataire
- Boutique (ou consolidé réseau)

### Exécution

- Service en arrière-plan (BackgroundService)
- Exécution selon la fréquence configurée
- Génération du rapport (CSV ou PDF)
- Envoi par email
- Mise à jour de `DerniereExecution` et `ProchaineExecution`

---

## 💳 Paiements intégrés

### Mobile Money

1. Initiation du paiement
   - Appel API du prestataire Mobile Money
   - Génération d'une référence unique
   - Enregistrement du `PaiementIntegre` (Statut: EnAttente)

2. Callback de confirmation
   - Webhook reçu du prestataire
   - Vérification de la référence
   - Mise à jour du statut (Valide ou Echec)
   - Notification à l'utilisateur

### Carte bancaire

1. Initiation du paiement
   - Redirection vers la page de paiement
   - Token de transaction généré
   - Enregistrement du `PaiementIntegre` (Statut: EnAttente)

2. Retour de paiement
   - Callback de succès/échec
   - Mise à jour du statut
   - Notification à l'utilisateur

---

## 🔒 Sécurité avancée

### Journal des connexions

- Enregistrement de chaque tentative (succès/échec)
- Adresse IP, email, date/heure
- Raison en cas d'échec

### Verrouillage temporaire

- Détection de tentatives échouées multiples (ex: 5 en 15 minutes)
- Verrouillage du compte pour X minutes
- Notification à l'administrateur

### Audit étendu

- Enregistrement des actions critiques :
  - Création/Modification/Suppression d'utilisateur
  - Changement de rôle
  - Annulation de vente
  - Validation de transfert
  - Modification de prix

---

## 📝 Migrations à créer

1. `AddTransfertStock`
2. `AddNotification`
3. `AddRapportProgramme`
4. `AddJournalConnexion`
5. `AddPaiementIntegre`

---

## 🧪 Tests à prévoir

### Tests unitaires
- Services de transfert
- Services de notification
- Services de rapport programmé

### Tests d'intégration
- Workflow complet de transfert
- Envoi de notifications via SignalR
- Génération et envoi de rapports

---

## 📅 Estimation (indicative)

- **Phase 1 (Fondations)** : 2-3 semaines
- **Phase 2 (Transferts)** : 2-3 semaines
- **Phase 3 (Notifications)** : 2 semaines
- **Phase 4 (Rapports programmés)** : 1-2 semaines
- **Phase 5 (Paiements intégrés)** : 3-4 semaines
- **Phase 6 (Sécurité avancée)** : 1-2 semaines

**Total estimé** : 11-16 semaines

---

## ✅ Critères d'acceptation

### Transferts
- [ ] Création de transfert inter-dépôts fonctionnelle
- [ ] Création de transfert inter-boutiques fonctionnelle
- [ ] Workflow validation → réception opérationnel
- [ ] Mise à jour automatique des stocks
- [ ] Traçabilité complète (mouvements de stock)

### Notifications
- [ ] Notifications temps réel via SignalR
- [ ] Centre de notifications accessible
- [ ] Marquer comme lu/non lu
- [ ] Alertes stock bas automatiques
- [ ] Alertes transferts en attente

### Rapports programmés
- [ ] Configuration de rapports programmés
- [ ] Exécution automatique selon fréquence
- [ ] Envoi par email fonctionnel
- [ ] Rapports consolidés réseau

### Paiements intégrés
- [ ] Intégration Mobile Money opérationnelle
- [ ] Intégration Carte bancaire opérationnelle
- [ ] Callbacks de confirmation fonctionnels
- [ ] Historique des paiements

### Sécurité
- [ ] Journal des connexions complet
- [ ] Verrouillage temporaire après tentatives échouées
- [ ] Audit des actions critiques

---

**Document créé le** : 2024  
**Version** : 1.0


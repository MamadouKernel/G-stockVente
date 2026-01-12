# Refonte UI/UX – Gestion Stock & Vente

## Vision
- Interface 2026, premium, minimaliste, fluide, orientée efficacité métier.
- Moins de clics, moins de bruit, plus de sens. Informations clés en premier.
- Accessible, performante, cohérente, mémorable, sans gradients.

## Direction artistique
- Couleurs marque: Bleu `#3481c0` (primary), Or `#dbb438` (accent), fonds clairs.
- Surfaces blanches, ombres très discrètes, bords arrondis doux (16-20px).
- Typo système moderne, forte lisibilité (poids 600-800 pour titres/valeurs).

## Audit (diagnostic rapide)
- Cohérence visuelle: variables centralisées manquantes avant refonte, styles dispersés.
- Hiérarchie: plusieurs pages mixaient filtres, stats et listes sans cap clair.
- Lisibilité: contrastes ok, mais densité parfois élevée (tables).
- Espacements: hétérogènes, manque d’échelle commune.
- Navigation: topbar ok; on renforce l’état actif + raccourcis contextuels.
- Feedbacks: vidage/erreur/chargement peu visibles initialement; ajout d’empty states.

Problèmes UX / UI / frustrations probables:
- Frictions dans le repérage des actions principales.
- Tables denses sans respirations ni accent sur les valeurs clés.
- Manque de feedbacks visuels harmonisés (vide/erreur/succès/chargement).

## Alternative (option B, plus audacieuse)
- Accent or plus présent (boutons primaires), tables compactées, chips colorées pour les tags et statuts, CTA flottants contextuels.

---

## Design System

### Couleurs (variables CSS)
```
:root {
  --color-primary: #3481c0;
  --color-accent: #dbb438;
  --color-success: #12b76a;
  --color-warning: #dbb438;
  --color-danger: #ef4444;
  --color-info: #2e90fa;
  --color-bg: #f4f6fb;
  --color-surface: #ffffff;
  --color-text: #101828;
  --color-muted: #667085;
  --color-border: rgba(16,24,40,.10);
}
```

### Typographie
- Famille: system-ui stack
- Titres: h1 32/38, h2 24/30, h3 20/28 (poids 700-800)
- Corps: 14-16/22-24 (poids 400-500)
- Small/Caption: 12/18

### Espacements
- Échelle: 4, 8, 12, 16, 24, 32, 48

### Composants
- Boutons: primary, outline, ghost, danger (hover/focus/disabled/loading)
- Inputs/Select/Textarea: focus ring or, placeholder grisé cohérent
- Cards/Surfaces: bords 16-20px, ombre subtile, header fin
- Tables: entête légère, hover row, cellules aérées
- Badges/Chips: bords 999px, tons success/warning/danger/info
- Alerts: styles doux, icône discrète (optionnel)
- Modals/Dropdowns: arrondis et ombres cohérents
- Tabs/Pagination/Breadcrumb: soulignements fins, état actif net
- Empty state: pictogramme/emoji optionnel, CTA contextualisé
- Loader/Skeleton: spinner léger / skeleton barres

---

## Navigation & structure UX
- Topbar horizontale (préférence projet). État actif explicite.
- Page header: titre + sous-titre + actions primaires à droite.
- Raccourcis contextuels (ex: “Mouvements” depuis “Stocks”).
- Priorités: KPI essentiels avant filtres/liste.

---

## Refactor des écrans (méthodo)
Pour chaque vue:
1) Objectif, CTA principal
2) Layout: Header (titre+CTA), StatBoxes, Filtres, Liste/Contenu, Footer
3) Lisibilité: colonnes clés, alignements droite/gauche cohérents
4) States: empty/loading/error/success

Exemples réalisés: Ventes/Index, Stocks/Mouvements.

Wireframe textuel type:
- Header: Titre + Actions
- Row: StatBoxes (4)
- Surface: Filtres
- Card: Liste/Table
- Empty/Error/Loading states intégrés

---

## Architecture front
```
/wwwroot/css
  variables.css
  base.css
  components.css
  layout.css

/Views/Shared
  _Layout.cshtml
  _Topbar.cshtml
  _Footer.cshtml

/Views/Shared/Components
  _Card.cshtml
  _StatBox.cshtml
  _EmptyState.cshtml
  _Loader.cshtml
```

---

## Plan de migration
1) Fondations: variables.css, base.css, components.css, layout.css (fait)
2) Layout & nav: _Layout + _Topbar + _Footer (fait)
3) Pages prioritaires: Dashboard, POS, Ventes, Stocks (fait)
4) Rapports, Achats, Utilisateurs, Boutiques (fait)
5) Paliers QA: contrastes, responsive, focus/keyboard, perf Lighthouse

Checklist qualité:
- [ ] Responsive ≥ 320px, 768px, 1200px
- [ ] Contraste AA min
- [ ] Focus visible (clavier)
- [ ] Pas de régression fonctionnelle
- [ ] Temps chargement stable
- [ ] Comportement cohérent (hover/focus/active/disabled)

---

Règle d’or: le backend et le métier ne sont pas modifiés; seule la présentation évolue.



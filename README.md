Procedural Generation
---
Ce dépôt regoupe notre semaine théorique de génération procédurale.
Nous avons pu apprendre et coder nos propres algorithmes de génération procédurale à travers diverses techniques.
Dans ce dépôt vous pourrez retrouvre des explications ainsi que le code des algorithmes Simple Room Placement, Binary Space Partition, Cellular Automata et Noise Generator.
---
## Table of Contents 
- 1. [Getting started](#getting-started)
- 2. [Explication de l'architecture](#explication-de-larchitecture)
- 3. [Initialisation](#initialisation)
- 4. [Simple Room Placement](#simple-room-placement)
- 5. [Binary Space Room (BSP)](#binary-space-room-bsp)
- 6. [Cellular Automata](#cellular-automata)
- 7. [Noise](#noise)

---
     
1. ## Getting started

- Créer un projet Unity 6 (6000.0.58f2)
- Installer Unitask avec [OpenUP](https://openupm.com/packages/com.cysharp.unitask/#modal-manualinstallation)
---

2. ## Explication de l'architecture

   1. La Grille (Grid)
    La Grille représente notre conteneur spatial qui va diviser l'espace de notre jeu en une grille 2D de cellules. Elle va s'occuper de la structure de données, c'est à dire un tableau 2D de Cell[,]  et une list<Cell>. De plus, elle va s'occuper aussi de transformer les coordonnées de grille (x,y) en position monde pour Unity.

Il faut savoir qu'il y a plusieurs méthodes à retenir comme :
- TryGetCellByCoordinates() : Récupère une cellule par ses indices
- TryGetCellByPosition() : Trouve la cellule à une position monde donnée
- GetCelllInCircle() : Sélectionne toutes les cellules dans un cercle

  2. La Cellule (Cell)
   Les cellules vont représenter un emplacement unique dans la grille, c'est un conteneur qui va s'occuper    des données qu'elle a et aussi son rendu.
  Elle vont stocker la position, contenir un objet (comme GridObject ou bien GridObjectController).
  De plus, elle peuvent gérer aussi leur contenu avec AddObject(),ClearGridObejct() et ContainObject.

  3. Méthode de Génération (ProceduralGenerationMethod)
     C'est uen classe abstraite ScriptableObject qui donne la structure pour les algorithmes de génération que nous allons voir après.

     Cetet classe va permette de créer notre différents algorithmes de génération en la faisant hérité à nos algorithmes. Eratum, dans cette classe nous avons besoin de unity Task.

  Il faut savoir qu'il y a plusieurs méthodes à retenir comme :
  - Generate() : génère le cycle de vie du code (nettoyage, annulation).
  - ApplyGeneration() : Méthode à implémenter dans les classe enfant.
  - CanPlaceRoom() : vérifier si une pièce peut être placée.
  - AddTileToCell() : palce un tuile dans une cellule.

---
3. ## Initialisation

Pour ajouter un nouvel algorithme, il suffit de créer une nouvelle classe est de la faire hérité de ProceduralGenerationMethod et d'implémenter la méthode abstraie ApplyGeneration.
Après ça vous aurez accès à toutes les méthodes nécessaires pour créer votre algorithmes de génération procédurale.

Dans la majorité des cas l'algorithmes ressembelra à :

- Initialiser
- Itérer
- Placer
- Vérifier l'annulation
- Ajouter des délais
- 
je vous laisse vosu référer au méthode pour les associer au bonnes étapes.

---
4. ## Simple Room Placement





     
     




  







 




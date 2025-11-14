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
je vous laisse vous référer au méthode pour les associer au bonnes étapes.

---
4. ## Simple Room Placement

Le Simple Room Placement est un algorithme qui génère un donjon simple avec des pièces rectangulaires connectées par des couloirs. 

Dans un premier temps, les variables :
<img width="315" height="39" alt="image" src="https://github.com/user-attachments/assets/f8cec5d6-a7b8-4490-ba15-da2eaac8c463" />


Dans la fonction ApplyGeneration, on réalise cette boucle afin de placer nos rooms de taille aléatoire, de position aléatoire. On vérifie si il n'y pas de collsion entre eux, et on répète jusqu'a _maxSteps.
<img width="457" height="450" alt="image" src="https://github.com/user-attachments/assets/b9425254-edcf-4d0e-8451-d279d11a0e9a" />

On obtient bien des pièces dispersées aléatorirement et séparées.

Toujours dans la fonction ApplyGeneration, on commence par chosir une première pièce comme connectée,
ensuite, on va chercher la pièce déconnecté la plus proche d'une pièce connectée. On trace des couloirs entre eux d'abord horizontal puis vertical, c'est pièce est alors marqué comme connecté. On répète alors jusqu'à ce que toutes les pièces soit connectées.

<img width="428" height="482" alt="image" src="https://github.com/user-attachments/assets/1a62bf4f-e418-4bc9-9216-3b272b9fa8fc" />


ET pour finir, on appelle BuildGround(), pour mettre un sol à la fin de la boucle ApplyGeneration().
<img width="408" height="164" alt="image" src="https://github.com/user-attachments/assets/5e1f80bf-44a9-45a2-a51b-b001c1860261" />

---

On l'utilise si :
- on veut un donjon simple
- des rooms délimité pour les combats
- on veut des donjons prévisible
  
On évite si :
- un rendu plus naturel et moins prévisible
- une structure logique dans la progression
- Des chemins alternatifs en grand nombres

5. ## Binary Space Room (BSP)

Le BSP est un algorithme qui va diviser l'espace des rooms de façon récursives via des nodes, on y place alors une pièce dans chaque zone finale.


<img width="301" height="151" alt="image" src="https://github.com/user-attachments/assets/1321b90d-7a28-41f4-9ea6-6df1765df894" />

c'es nodes sont placé dans un ordre hiérarchique, ce qui va nous permettre de contruitre petit notre en suivant l'arbre, puis en connctant les nodes entre eux pour créer nos couloirs.




     
     




  







 




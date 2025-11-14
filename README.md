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
- On veut un donjon simple
- Des rooms délimité pour les combats
- On veut des donjons prévisible
  
On évite si :
- Un rendu plus naturel et moins prévisible
- Une structure logique dans la progression
- Des chemins alternatifs en grand nombres

5. ## Binary Space Room (BSP)

Le BSP est un algorithme qui va diviser l'espace des rooms de façon récursives via des nodes, on y place alors une pièce dans chaque zone finale.


<img width="301" height="151" alt="image" src="https://github.com/user-attachments/assets/1321b90d-7a28-41f4-9ea6-6df1765df894" /> Source : Diapo, Yona Rutkowski

ces nodes sont placé dans un ordre hiérarchique, ce qui va nous permettre de construitre petit notre en suivant l'arbre, puis en connctant les nodes entre eux pour créer nos couloirs.

En amont, il faut créer notre classe node qui va nous permettre d'avoir les méthodes de disivions des des zones et la logique d'enfants, parents.
<img width="517" height="461" alt="image" src="https://github.com/user-attachments/assets/3c38177a-5db6-4f82-bc86-0add9ba77e57" />

<img width="336" height="358" alt="image" src="https://github.com/user-attachments/assets/eb1917a3-4ad3-4d13-a919-90972c3c858f" />

Pour commencer, on récupére la grille de base, que l'on va diviser en 2 parties soit horizontal ou vertical. On répète ensuite jusqu'à _maxDepth ou !node.CanSplit().

<img width="401" height="113" alt="image" src="https://github.com/user-attachments/assets/4dc8b045-109f-4d94-954a-752ea40e057e" />

On génère ensuite des rooms que l'on palce dasn les zones créer au préalabre.

<img width="271" height="134" alt="image" src="https://github.com/user-attachments/assets/dfa92af0-738a-40a8-b59a-44dce9cf700e" />

<img width="410" height="236" alt="image" src="https://github.com/user-attachments/assets/4acf1688-2b87-4600-84d5-6c4beff37707" />


La dernière partie consiste à remonter l'arbre deppuis les feuilles que l'on à créer.
On a donc pour chaque noeud parent, 2 enfant que l'on va connecter. On cherche la pièce la plus proche du sous-arbre à gauche, apreil pour le sous-arbre à droite(ConnectSiblingNodes(BSPNode node, CancellationToken cancellationToken)), puis on trace un couloir en L entre elles(CreateOptimizedCorridor(RectInt room1, RectInt room2, CancellationToken cancellationToken)). On évite auss iles doublous avec _connectedPairs. Constuire le sol après.

<img width="408" height="361" alt="image" src="https://github.com/user-attachments/assets/a7c057bf-8399-498e-a228-b84fa286a057" />

<img width="365" height="305" alt="image" src="https://github.com/user-attachments/assets/bc640948-84d4-4148-b77f-ae5cb9aef237" />

<img width="474" height="458" alt="image" src="https://github.com/user-attachments/assets/785e1fc4-83ad-40a9-b74c-c39ec28b4c0f" />

<img width="349" height="316" alt="image" src="https://github.com/user-attachments/assets/c3e031a8-b04c-41df-a8ec-0753f271c964" />

---

On l'utilise si :
- On veut des donjons avec un distribution uniforme
- On veut garantir qu'il n'y a pas de grandes zones vides
- On veut une structure hiérarchique (thématiques)
  
On évite si :
- Si on ne veut pas un rendu trop régulier
- Si on ne veut pas que toutes les zones soit occuper
- Plus complexe à comprendre.

---
6. ## Cellular Automata

Le cellular Automata est un algorithme qui simule l'évolution naturelle d'un sytème.
Chaque cellule va avoir un temps de vie selon ses voisins permettant de créer des formes organques comme des cavernes, des îles ou des lacs.

<img width="245" height="118" alt="image" src="https://github.com/user-attachments/assets/9f7b77e7-3d3b-431d-8768-9ed8ecde63f9" />

Dans un premiere temps, on généère une grilel aléatoire, si le résultat  est inférieur à notre _noiseDensity alors les celulles se sera de l'herbe sinon de l'eau 

<img width="235" height="139" alt="image" src="https://github.com/user-attachments/assets/ed6f045e-cb0b-4a74-a8a5-6254a9cef422" />

Ensuite dans le ApplyGeneration(), On compte les voisins de chaque cellules qui sont de la l'herbe et on applique 2 règles :

- Si la cellue est de l'herbe, alors elle survit si grassNeighbors est suppérieur ou égale _grassSurvivalThreshold, sinon elle meurt.
- Si la cellule est de l'eau, alors elle devient de l'herbe si grassNeighbors est supérieur ou égale à _grassBirthThreashold, sinon elle reste de l'eau.

On répète cette étape jusqu'à _maxSteps

<img width="371" height="236" alt="image" src="https://github.com/user-attachments/assets/ccad2c2c-165e-42fb-b4e0-458d6ca5a47f" />

---

On l'utilise si :
- On veut des cavernes naturelles, îles organiques, forêts, végétations, lacs vont avoir un air anturel

  
On évite si :
- Si on veut un controle précis
- Si on veut un connectivité assuré
- Aucune logique d'entré/sortie
- Lent pour de grandes gilles

---

7. ## Noise

Pour cette algorithmes, nous nous sommes servi du FastNoiseLite de Jordan Peck afin de créer un Noise Generator pour générer une map procédurale.
Un Noise Generator est un algorithmes qui utilise des fonctions de bruits mathématiques pour générer des terrains naturel et réalistes avec des transitiosn douces entre les biomes.
Le Nosie Generator va calculer directement la valeur de chaque cellule à partir de ses cordordonnées sur sa texture.

<img width="262" height="201" alt="image" src="https://github.com/user-attachments/assets/eb660d4c-f864-4e39-8011-99f83d13bd73" />

Pour chaque cellule de coordonnées (x,y), on calcule une valeur de bruit entre -1 et +1. Par exemple, avec le perlinNoise qui un type de bruit : noise = PerlinNoise(x,y, fréquence, octaves...). De plus, vous pouvez choisir un bruit secondaire pour l'ajouter au principale et faire un mix comme mix(nois1, nois2, poids).


<img width="192" height="110" alt="image" src="https://github.com/user-attachments/assets/1ffcbaff-17b2-4411-8024-23cb5a7e93fb" />

On choisit ensuite valeur de bruit et y assigner une tile. Pour ça on comapre la noisevalue au threshold.

On applique tout ensuite dans le ApplyGeneration().

<img width="376" height="424" alt="image" src="https://github.com/user-attachments/assets/6075b711-bb09-4d1e-acc9-31e10988b88b" />

Comme, on peut voir dans les variables, il y a plusieurs paramètres que l'on peut prendre en compte pour générer nos terrains. Voici un memo, pour savoir à quoi correspond ces paramètre :

- Frequency : fréquances basse(0.005 - 0.02) -> grandes zones uniformes et fréquence hautes(0.1+) -> petites zones variées. Controles la teaille des biomes.
  
- Octaves : 1 - 2 octaves -> grandes vagues et 6+ octaves -> détails moyens, petits détails, micro-variations.
  Ajoute du détails en superposant plusieurs couches.
  
- Gain : Gain (0,5) -> détails subtils et Gain (0,8) -> détails trsè visibles. Contrôle l'amplitude entre les octaves.
  
- Bruit secondaire : Combine deux bruits différents pour plus de complexité.

Vous pouvez aussi choisir parmis différent type de noise comme perlin ou OpenSimplex2 et bien d'autres.
Chacun ayant ces particularités.

---

On l'utilise si :
- On a besoind de heightmaps
- On besoin de biomes avec des transitions naturelles
- îles,continent réalistes
- Variations de texture
- Bien optimisé pour de l'open-world

  
On évite si :
- Si on veut des donjons avce des couloirs
- Si on veut des formes vraiment spécifiques
- Connectivité aléatoires
- Peut etre répétitif
- Génère juste un terrain

---
  



  







 




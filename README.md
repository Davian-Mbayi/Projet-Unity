# MazeGame

Jeu de labyrinthe 3D vue du dessus (top-down) developpe sous **Unity 6** en C#, dans le cadre d'un projet Bachelor Informatique & Developpement.

Le joueur doit collecter l'ensemble des fragments dores disperses dans un labyrinthe genere proceduralement avant l'expiration du chronometre. Une fois tous les fragments ramasses, la porte de sortie se deverrouille — il faut alors l'atteindre pour progresser au niveau suivant.

---

## Fonctionnalites

- Generation procedurale de labyrinthe via l'algorithme **DFS iteratif** (Recursive Backtracking)
- Deplacement physique du joueur avec `Rigidbody.MovePosition` et rotation lissee `Quaternion.Slerp`
- Systeme de chronometre en decompte avec alerte visuelle sous les 10 secondes
- Minimap orthographique synchronisee en temps reel
- Progression sur deux niveaux avec difficulte croissante
- Optimisation GPU par `StaticBatchingUtility.Combine()` (1 draw call pour tous les murs)
- Architecture **Singleton** pour la communication inter-scripts

---

## Structure du projet

```
Assets/
├── Scripts/
│   ├── GameManager.cs              # Orchestrateur central (Singleton)
│   ├── MazeGeneratorAdaptive.cs    # Generation procedurale DFS
│   ├── PlayerController.cs         # Deplacement et rotation joueur
│   ├── TopDownCamera.cs            # Camera avec effet elastique
│   ├── MinimapController.cs        # Camera minimap orthographique
│   ├── Coin.cs                     # Collectible — notifie le GameManager
│   ├── ExitTrigger.cs              # Detection de la sortie
│   └── LevelConfig.cs              # Configuration serialisable des niveaux
├── Scenes/
│   ├── Level1.unity
│   └── Level2.unity
└── ...
```

---

## Technologies

| Element | Detail |
|---|---|
| Moteur | Unity 6 |
| Langage | C# |
| Rendu | 3D Top-Down — camera inclinee a 65° |
| Generation | DFS Iteratif (Recursive Backtracking) |
| UI | TextMeshPro |
| Physique | Rigidbody, BoxCollider, triggers |

---

## Lancer le projet

1. Cloner le depot
2. Ouvrir le projet avec **Unity 6** via Unity Hub
3. Ouvrir la scene `Level1` depuis `Assets/Scenes/`
4. Appuyer sur **Play**

> Les deux scenes `Level1` et `Level2` doivent etre presentes dans les **Build Settings** pour que la transition de niveau fonctionne correctement.

---

## Build

Pour generer un executable :

1. `File > Build Settings`
2. Ajouter `Level1` et `Level2` dans la liste des scenes (dans cet ordre)
3. Selectionner la plateforme cible (Windows, Mac ou Linux)
4. Cliquer sur **Build**

L'executable genere doit rester dans son dossier de build — les fichiers `_Data/` et `UnityPlayer.dll` sont indispensables au lancement.

---

## Architecture technique

Le projet repose sur le patron **Singleton** pour le `GameManager`, point de communication unique entre tous les scripts. Cela elimine les recherches `GameObject.Find()` et garantit un acces O(1) depuis n'importe quel script.

La generation du labyrinthe utilise une `Stack<Vector2Int>` explicite plutot qu'une recursion naive, evitant tout risque de `StackOverflowException` sur les grandes grilles.

---

*Projet realise dans le cadre du Bachelor Informatique & Developpement.*

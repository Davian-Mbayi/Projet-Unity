using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGeneratorAdaptive : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  INSPECTEUR
    // ─────────────────────────────────────────────────────────────────────────
 
    [Header("Surface de référence")]
    [Tooltip("Le GameObject de sol (Plane, Quad, Terrain…). Sa taille détermine les dimensions du labyrinthe.")]
    public GameObject floorObject;
 
    [Tooltip("Si aucun floorObject n'est fourni, ces dimensions (en unités Unity) sont utilisées.")]
    public Vector2 fallbackFloorSize = new Vector2(20f, 20f);
 
    [Header("Joueur & Espacement")]
    [Tooltip("Largeur de la capsule du joueur (CharacterController.radius * 2). Le couloir sera au moins cette valeur.")]
    public float playerWidth = 0.8f;
 
    [Tooltip("Marge supplémentaire autour du joueur pour le confort de déplacement (en unités Unity).")]
    public float corridorMargin = 0.4f;
 
    [Header("Murs")]
    [Tooltip("Prefab représentant un mur. Son pivot doit être au centre.")]
    public GameObject wallPrefab;
 
    [Tooltip("Hauteur des murs générés.")]
    public float wallHeight = 2.5f;
 
    [Tooltip("Épaisseur des murs (axe X ou Z selon l'orientation).")]
    [Range(0.05f, 0.5f)]
    public float wallThickness = 0.15f;
 
    [Header("Entrée / Sortie")]
    [Tooltip("Ouvre automatiquement une entrée sur le bord Nord et une sortie sur le bord Sud.")]
    public bool createEntryExit = true;
 
    [Header("Debug")]
    public bool showGizmos = true;
    [Header("Mécanique de Jeu (Pièces & Sortie)")]
    [Tooltip("Le prefab de la pièce (doit contenir le script Coin)")]
    public GameObject coinPrefab;
    [Tooltip("Nombre de pièces à générer dans le labyrinthe")]
    public int totalCoinsToSpawn = 5;
    

    private int[,] maze;          // 1 = mur, 0 = couloir
    private int gridWidth;        // Nombre de colonnes de la grille
    private int gridDepth;        // Nombre de lignes   de la grille
    private float cellSize;       // Taille d'une cellule = largeur d'un couloir
    private Vector3 originOffset; // Coin bas-gauche du labyrinthe dans le monde
    private GameObject exitWallObject;

    void Start()
    {
        ComputeGridParameters();
        GenerateMazeData();
        if (createEntryExit) OpenEntryAndExit();
        DrawMaze3D();
        SpawnCoins();
    }

    void ComputeGridParameters()
    {
        // 1. Récupérer la taille physique du sol
        Vector2 floorSize = fallbackFloorSize;
        if (floorObject != null)
        {
            Renderer rend = floorObject.GetComponent<Renderer>();
            if (rend != null)
                floorSize = new Vector2(rend.bounds.size.x, rend.bounds.size.z);
            else
                floorSize = new Vector2(floorObject.transform.lossyScale.x, floorObject.transform.lossyScale.z);
        }
 
        float minCorridorWidth = playerWidth + corridorMargin;
        cellSize = Mathf.Max(minCorridorWidth, 0.5f);
 
        // 3. Calcul du nombre de couloirs réels
        int corridorsX = Mathf.Max(1, Mathf.FloorToInt((floorSize.x - wallThickness) / (cellSize + wallThickness)));
        int corridorsZ = Mathf.Max(1, Mathf.FloorToInt((floorSize.y - wallThickness) / (cellSize + wallThickness)));
 
        gridWidth = 2 * corridorsX + 1;
        gridDepth = 2 * corridorsZ + 1;
 
        // FIX 1: Diviser par le nombre de couloirs réels (corridorsX) et non par gridWidth
        float adjustedCellSizeX = (floorSize.x - wallThickness) / corridorsX - wallThickness;
        float adjustedCellSizeZ = (floorSize.y - wallThickness) / corridorsZ - wallThickness;
        
        cellSize = Mathf.Min(adjustedCellSizeX, adjustedCellSizeZ);
        cellSize = Mathf.Max(cellSize, minCorridorWidth);
 
        Vector3 floorCenter = floorObject != null ? floorObject.transform.position : transform.position;
 
        // FIX 2: La taille physique totale = taille des couloirs + taille des murs
        float totalWidth = corridorsX * cellSize + (corridorsX + 1) * wallThickness;
        float totalDepth = corridorsZ * cellSize + (corridorsZ + 1) * wallThickness;
 
        originOffset = floorCenter - new Vector3(totalWidth / 2f, 0f, totalDepth / 2f);
    }
 
    void GenerateMazeData()
    {
        maze = new int[gridWidth, gridDepth];
 
        // Tout remplir de murs
        for (int x = 0; x < gridWidth; x++)
            for (int z = 0; z < gridDepth; z++)
                maze[x, z] = 1;
 
        // Creuser depuis (1,1)
        CarvePath(1, 1);
    }

    void CarvePath(int startX, int startZ)
    {
        // On initialise notre propre pile
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        
        // On marque le point de départ comme un couloir et on l'ajoute à la pile
        maze[startX, startZ] = 0;
        stack.Push(new Vector2Int(startX, startZ));
 
        // Tant que notre pile n'est pas vide, on continue de creuser
        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek(); // On regarde la cellule actuelle
            int x = current.x;
            int z = current.y;
 
            int[] directions = { 1, 2, 3, 4 };
            Shuffle(directions);
 
            bool carvedNewPath = false;
 
            // On cherche un voisin valide vers lequel creuser
            foreach (int dir in directions)
            {
                int nextX = x, nextZ = z;
                int midX  = x, midZ  = z;
 
                switch (dir)
                {
                    case 1: nextZ = z + 2; midZ = z + 1; break; // Haut
                    case 2: nextZ = z - 2; midZ = z - 1; break; // Bas
                    case 3: nextX = x + 2; midX = x + 1; break; // Droite
                    case 4: nextX = x - 2; midX = x - 1; break; // Gauche
                }
 
                bool inBounds = nextX > 0 && nextX < gridWidth - 1
                             && nextZ > 0 && nextZ < gridDepth - 1;
 
                // Si la prochaine case est un mur intact
                if (inBounds && maze[nextX, nextZ] == 1)
                {
                    // On casse le mur intermédiaire et on marque la nouvelle cellule
                    maze[midX, midZ] = 0;
                    maze[nextX, nextZ] = 0;
                    
                    // On avance en ajoutant cette nouvelle position sur le dessus de la pile
                    stack.Push(new Vector2Int(nextX, nextZ));
                    carvedNewPath = true;
                    break; // On sort du foreach pour explorer cette nouvelle cellule au prochain tour du while
                }
            }
 
            // Si on n'a trouvé aucun voisin valide (cul-de-sac)
            if (!carvedNewPath)
            {
                // On retire la cellule actuelle de la pile pour revenir en arrière (Backtrack)
                stack.Pop();
            }
        }
    }
 
 
   void OpenEntryAndExit()
{
    maze[gridWidth - 2, gridDepth - 1] = 1; 
}

    void DrawMaze3D()
    {
        // On regroupe tous les murs sous un parent pour garder la hiérarchie propre
        GameObject mazeParent = new GameObject("Maze_Walls");
        mazeParent.transform.SetParent(this.transform, false);
 
        float floorY = floorObject != null
            ? floorObject.transform.position.y
            : transform.position.y;
 
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridDepth; z++)
            {
                if (maze[x, z] != 1) continue;
 
                // Position du centre de la cellule
                Vector3 worldPos = CellToWorld(x, z, floorY);
 
                // Déterminer si le mur est "horizontal" (longe l'axe X) ou "vertical" (longe Z)
                bool isHorizontalWall = (z % 2 == 0) && (x % 2 == 1);
                bool isVerticalWall   = (x % 2 == 0) && (z % 2 == 1);
                bool isPost           = (x % 2 == 0) && (z % 2 == 0); // Pilier de coin
 
                Vector3 scale;
                Quaternion rot = Quaternion.identity;
 
                if (isHorizontalWall)
                {
                    // S'étend le long de X
                    scale = new Vector3(cellSize + wallThickness * 2f, wallHeight, wallThickness);
                }
                else if (isVerticalWall)
                {
                    // S'étend le long de Z
                    scale = new Vector3(wallThickness, wallHeight, cellSize + wallThickness * 2f);
                }
                else // Pilier (coin) ou mur plein dans les coins de grille
                {
                    scale = new Vector3(wallThickness + cellSize * 0.5f,
                                        wallHeight,
                                        wallThickness + cellSize * 0.5f);
 
                    // Pour les vrais piliers (x et z pairs) on utilise juste l'épaisseur
                    if (isPost)
                        scale = new Vector3(wallThickness, wallHeight, wallThickness);
                }
 
                GameObject wall = Instantiate(wallPrefab, worldPos, rot, mazeParent.transform);
                wall.transform.localScale = scale;
                wall.name = $"Wall_{x}_{z}";

                // --- NOUVEAU : Sauvegarder le mur de sortie ---
                // La sortie prévue est au bord Nord, sur l'avant-dernière colonne
                if (x == gridWidth - 2 && z == gridDepth - 1)
                {
                    wall.name = "Exit_Door";
                    exitWallObject = wall;
                }
            }
        }
        StaticBatchingUtility.Combine(mazeParent);
    }
    float GetPhysicalCoordinate(int index)
    {
        int wallsBefore = (index + 1) / 2;
        int corridorsBefore = index / 2;
 
        float position = wallsBefore * wallThickness + corridorsBefore * cellSize;
 
        // Centrer sur la cellule actuelle
        if (index % 2 == 0) // C'est un mur
            position += wallThickness / 2f;
        else // C'est un couloir
            position += cellSize / 2f;
 
        return position;
    }
 
    Vector3 CellToWorld(int gx, int gz, float floorY)
    {
        // FIX 3: On utilise les vraies dimensions physiques pour calculer les coordonnées
        float wx = originOffset.x + GetPhysicalCoordinate(gx);
        float wz = originOffset.z + GetPhysicalCoordinate(gz);
        float wy = floorY + wallHeight / 2f;
 
        return new Vector3(wx, wy, wz);
    }

    void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }
 

    void OnDrawGizmos()
    {
        if (!showGizmos || maze == null) return;
 
        float floorY = floorObject != null ? floorObject.transform.position.y : transform.position.y;
 
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridDepth; z++)
            {
                Vector3 pos = CellToWorld(x, z, floorY);
                pos.y = floorY + 0.05f;
 
                Gizmos.color = maze[x, z] == 1 ? new Color(1f, 0.3f, 0.3f, 0.6f)
                                                : new Color(0.3f, 1f, 0.5f, 0.3f);
                Gizmos.DrawCube(pos, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
            }
        }
    }

    public void SpawnCoins()
    {
        if (coinPrefab == null) return;

        List<Vector2Int> deadEnds = new List<Vector2Int>();
        List<Vector2Int> allEmptyCells = new List<Vector2Int>();

        // 1. Analyser le labyrinthe pour trouver les culs-de-sac
        for (int x = 1; x < gridWidth - 1; x++)
        {
            for (int z = 1; z < gridDepth - 1; z++)
            {
                if (maze[x, z] == 0) // Si c'est un couloir
                {
                    allEmptyCells.Add(new Vector2Int(x, z));

                    // Compter les murs autour de cette case
                    int wallCount = 0;
                    if (maze[x + 1, z] == 1) wallCount++; // Droite
                    if (maze[x - 1, z] == 1) wallCount++; // Gauche
                    if (maze[x, z + 1] == 1) wallCount++; // Haut
                    if (maze[x, z - 1] == 1) wallCount++; // Bas

                    // Une impasse possède exactement 3 murs autour d'elle
                    if (wallCount >= 3)
                    {
                        // On évite de placer une pièce juste sur l'entrée (1, 0)
                        if (!(x == 1 && z == 1)) 
                        {
                            deadEnds.Add(new Vector2Int(x, z));
                        }
                    }
                }
            }
        }

        // 2. Déterminer la liste à utiliser (priorité aux culs-de-sac)
        List<Vector2Int> validSpawnPoints = deadEnds.Count >= totalCoinsToSpawn ? deadEnds : allEmptyCells;
        
        int coinsToSpawn = Mathf.Min(totalCoinsToSpawn, validSpawnPoints.Count);
        totalCoinsToSpawn = coinsToSpawn; 

        float floorY = floorObject != null ? floorObject.transform.position.y : transform.position.y;

        // 3. Instancier les pièces
        for (int i = 0; i < coinsToSpawn; i++)
        {
            int randomIndex = Random.Range(0, validSpawnPoints.Count);
            Vector2Int pos = validSpawnPoints[randomIndex];
            validSpawnPoints.RemoveAt(randomIndex); 

            Vector3 worldPos = CellToWorld(pos.x, pos.y, floorY);
            worldPos.y = floorY + 1f; 

        Instantiate(coinPrefab, worldPos, Quaternion.identity, this.transform);
            
        }
        GameManager.Instance.SetupLevel(totalCoinsToSpawn, exitWallObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// Génère un labyrinthe procédural qui s'adapte automatiquement à la surface du sol
/// et garantit un espacement suffisant pour que le joueur puisse passer.
/// Basé sur l'algorithme Recursive Backtracking.
/// </summary>
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
    public float corridorMargin = 0.2f;
 
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
 
    // ─────────────────────────────────────────────────────────────────────────
    //  DONNÉES PRIVÉES
    // ─────────────────────────────────────────────────────────────────────────
 
    private int[,] maze;          // 1 = mur, 0 = couloir
    private int gridWidth;        // Nombre de colonnes de la grille
    private int gridDepth;        // Nombre de lignes   de la grille
    private float cellSize;       // Taille d'une cellule = largeur d'un couloir
    private Vector3 originOffset; // Coin bas-gauche du labyrinthe dans le monde
 
    // ─────────────────────────────────────────────────────────────────────────
    //  DÉMARRAGE
    // ─────────────────────────────────────────────────────────────────────────
 
    void Start()
    {
        ComputeGridParameters();
        GenerateMazeData();
        if (createEntryExit) OpenEntryAndExit();
        DrawMaze3D();
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    //  CALCUL DES PARAMÈTRES DE GRILLE
    // ─────────────────────────────────────────────────────────────────────────
 
    /// <summary>
    /// Déduit la taille de cellule et les dimensions de la grille à partir
    /// du sol de référence et de la taille du joueur.
    /// </summary>
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
 
    // ─────────────────────────────────────────────────────────────────────────
    //  GÉNÉRATION DES DONNÉES
    // ─────────────────────────────────────────────────────────────────────────
 
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
 
    /// <summary>
    /// Algorithme Recursive Backtracking (DFS) – creuse des couloirs.
    /// </summary>
    void CarvePath(int x, int z)
    {
        maze[x, z] = 0;
 
        int[] directions = { 1, 2, 3, 4 };
        Shuffle(directions);
 
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
 
            if (inBounds && maze[nextX, nextZ] == 1)
            {
                maze[midX, midZ] = 0;
                CarvePath(nextX, nextZ);
            }
        }
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    //  ENTRÉE / SORTIE
    // ─────────────────────────────────────────────────────────────────────────
 
    void OpenEntryAndExit()
    {
        // Entrée : bord Sud (z = 0), première colonne couloir
        maze[1, 0] = 0;
 
        // Sortie : bord Nord (z = gridDepth-1), dernière colonne couloir
        maze[gridWidth - 2, gridDepth - 1] = 0;
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    //  CONSTRUCTION 3D
    // ─────────────────────────────────────────────────────────────────────────
 
    /// <summary>
    /// Instancie et met à l'échelle les murs pour correspondre exactement
    /// à la grille calculée.
    /// </summary>
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
            }
        }
    }

    /// <summary>
    /// Calcule la position réelle sur un axe en fonction de l'indice logique,
    /// en séparant proprement l'épaisseur des murs de la largeur des couloirs.
    /// </summary>
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
 
    /// <summary>
    /// Convertit des coordonnées de grille (x, z) en position mondiale Unity.
    /// </summary>
    Vector3 CellToWorld(int gx, int gz, float floorY)
    {
        // FIX 3: On utilise les vraies dimensions physiques pour calculer les coordonnées
        float wx = originOffset.x + GetPhysicalCoordinate(gx);
        float wz = originOffset.z + GetPhysicalCoordinate(gz);
        float wy = floorY + wallHeight / 2f;
 
        return new Vector3(wx, wy, wz);
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    //  UTILITAIRES
    // ─────────────────────────────────────────────────────────────────────────
    void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }
 
    // ─────────────────────────────────────────────────────────────────────────
    //  GIZMOS (visualisation dans l'éditeur)
    // ─────────────────────────────────────────────────────────────────────────
 
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
}
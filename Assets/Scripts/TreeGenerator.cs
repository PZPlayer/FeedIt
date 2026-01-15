using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class TreeGenerator : MonoBehaviour
{
    [Header("Настройки генерации деревьев")]
    [SerializeField] private int _minTrees = 800;
    [SerializeField] private int _maxTrees = 1000;
    [SerializeField] private float _minDistanceBetweenTrees = 2.0f;
    [SerializeField] private int _maxAttemptsPerTree = 30;

    private Terrain terrain;
    private TerrainData terrainData;
    private TerrainCollider terrainCollider;

    void Start()
    {
        GenerateTreesOnTerrain();
    }

    public void GenerateTreesOnTerrain()
    {
        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("Terrain component not found!");
            return;
        }

        terrainCollider = GetComponent<TerrainCollider>();
        if (terrainCollider == null)
        {
            Debug.LogError("TerrainCollider component not found!");
            return;
        }

        terrainData = terrain.terrainData;
        if (terrainData == null)
        {
            Debug.LogError("TerrainData not found!");
            return;
        }

        TreePrototype[] treePrototypes = terrainData.treePrototypes;
        if (treePrototypes.Length == 0)
        {
            Debug.LogError("No tree prototypes found in terrain! Add tree prototypes first.");
            return;
        }

        terrainCollider.enabled = false;

        int targetTreeCount = Random.Range(_minTrees, _maxTrees + 1);
        List<TreeInstance> trees = new List<TreeInstance>();
        List<Vector3> treePositions = new List<Vector3>();

        int treesPlaced = 0;
        int attempts = 0;

        while (treesPlaced < targetTreeCount && attempts < targetTreeCount * _maxAttemptsPerTree)
        {
            Vector3 randomPosition = GetRandomTerrainPosition();

            if (IsPositionValid(randomPosition, treePositions, _minDistanceBetweenTrees))
            {
                TreeInstance tree = CreateTreeInstance(randomPosition, treePrototypes);
                trees.Add(tree);
                treePositions.Add(randomPosition);
                treesPlaced++;
            }

            attempts++;
        }

        terrainData.treeInstances = trees.ToArray();

        terrainCollider.enabled = true;

        Debug.Log($"Успешно размещено {treesPlaced} деревьев из {targetTreeCount} запланированных");
        Debug.Log($"Попыток: {attempts}");
    }

    private Vector3 GetRandomTerrainPosition()
    {
        float x = Random.Range(0f, 1f);
        float z = Random.Range(0f, 1f);

        float terrainHeight = terrainData.GetHeight(
            Mathf.RoundToInt(x * terrainData.heightmapResolution),
            Mathf.RoundToInt(z * terrainData.heightmapResolution)
        ) / terrainData.size.y;

        return new Vector3(x, terrainHeight, z);
    }

    private bool IsPositionValid(Vector3 position, List<Vector3> existingPositions, float minDistance)
    {
        foreach (Vector3 existingPos in existingPositions)
        {
            Vector3 worldPos1 = new Vector3(
                existingPos.x * terrainData.size.x,
                existingPos.y * terrainData.size.y,
                existingPos.z * terrainData.size.z
            );

            Vector3 worldPos2 = new Vector3(
                position.x * terrainData.size.x,
                position.y * terrainData.size.y,
                position.z * terrainData.size.z
            );

            if (Vector3.Distance(worldPos1, worldPos2) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    private TreeInstance CreateTreeInstance(Vector3 position, TreePrototype[] prototypes)
    {
        TreeInstance tree = new TreeInstance();

        int prototypeIndex = Random.Range(0, prototypes.Length);

        tree.position = position;
        tree.prototypeIndex = prototypeIndex;
        tree.widthScale = Random.Range(0.8f, 1.2f);
        tree.heightScale = Random.Range(0.8f, 1.2f);
        tree.color = Color.white;
        tree.lightmapColor = Color.white;
        tree.rotation = Random.Range(0f, 2f * Mathf.PI);

        return tree;
    }

    [ContextMenu("Сгенерировать деревья")]
    private void RegenerateTrees()
    {
        GenerateTreesOnTerrain();
    }
}
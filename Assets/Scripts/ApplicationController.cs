using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEditor;

public class ApplicationController : SingletonMono<ApplicationController>
{
    public CameraController CamController { get; private set; }
    public float CurrentBrushSize { get; private set; } = 5f;
    public float CurrentBrushStrength { get; private set; } = 1f;
    public GameObject currentTerrainMeshGO { get; private set; }
    public FirstPersonPlayer FPSPlayer { get; private set; }

    public List<GameObject> GeneratedTrees = new List<GameObject>();

    private bool leftMouseDown = false;
    private bool shiftDown = false;
    private bool ctrlDown = false;
    private bool altDown = false;

    //TODO: RMB controls camera rotation, but needs world hit point, so it's done here. Just lazy and obfuscating.
    private Vector3 mouseRightDownPosition = Vector3.zero;
    private Vector3 mouseRightDownDelta = Vector3.zero;
    private bool mouseRightDrag = false;
    private Vector3? rightDownHitpoint;

    [RuntimeInitializeOnLoadMethod]
    static void OnInit()
    {
        Instance.Init();
    }

    #region public
    public void Init()
    {
        //Setup and initialize camera controller for main camera
        CamController = Camera.main.gameObject.AddComponent<CameraController>();
        CamController.Init(Camera.main);

        //Subscribe to input (not need to dispose, this is singleton)
        InputController.Instance.MouseLeftDown.Subscribe(b => leftMouseDown = b);
        InputController.Instance.MouseRightDown.Subscribe(b => HandleMouseRightDown(b));
        InputController.Instance.ShiftDown.Subscribe(b => shiftDown = b);
        InputController.Instance.CtrlDown.Subscribe(b => ctrlDown = b);
        InputController.Instance.AltDown.Subscribe(b => altDown = b);
        InputController.Instance.EscDown.Subscribe(b => { if (b) DisableFPSPlayer(); });

        //Instantiate FPS player
        FPSPlayer = Instantiate<FirstPersonPlayer>(Resources.Load<FirstPersonPlayer>("FirstPersonRig"));
        FPSPlayer.gameObject.SetActive(false);        
    }

    public void DropFPSPlayerToTerrain(Vector3 screenPosition)
    {
        Vector3? terrainHitpoint = ScreenPointRaycast(screenPosition)?.point ?? null;
        if (terrainHitpoint.HasValue)
        {
            FPSPlayer.MouseLook.SetCursorLock(true);
            FPSPlayer.FirstPersonCamera.gameObject.SetActive(true);
            FPSPlayer.transform.position = terrainHitpoint.Value;
            FPSPlayer.gameObject.SetActive(true);
        }
    }

    public void DisableFPSPlayer()
    {
        FPSPlayer.FirstPersonCamera.gameObject.SetActive(false);
        FPSPlayer.MouseLook.SetCursorLock(false);
        FPSPlayer.gameObject.SetActive(false);
    }

    /// <summary>
    /// Set size and strength of push/pull/flatten brush.
    /// </summary>
    /// <param name="size">Brush size.</param>
    /// <param name="strength">Brush strength.</param>
    public void SetBrushSizeAndStrength(float size, float strength)
    {
        CurrentBrushSize = size;
        CurrentBrushStrength = strength;
    }

    public Texture2D GenerateTerrainAndNoiseTexture(int size, float height, float noiseScale, bool smooth, bool plateau, int octaves = 4, float lacunarity = 3f, bool withOctaves = false)
    {
        //Randomize seed, offset and octaves to produce different kinds of noise
        Vector2 offset = new Vector2(Random.Range(0f, 10f), Random.Range(0f, 10f));
        int seed = Random.Range(0, 10000);

        float[] noise;

        if (withOctaves) //either generate more complex noise with octaves, persistence and lacunarity...
            noise = Noise.PerlinNoiseWithOctaves(size, size, seed, noiseScale, offset, octaves, 0.5f, lacunarity);
        else //...or simple (smooth) noise with single perlin noise sample
            noise = Noise.SimplePerlinNoise(size, size, seed, noiseScale, offset);

        Color[] colors = Noise.FloatArrayToBWColorArray(noise, size, size);
        Texture2D texture = Noise.ColorArrayToTexture(colors, size, size);

        if (currentTerrainMeshGO != null)
            GameObject.DestroyImmediate(currentTerrainMeshGO);

        currentTerrainMeshGO = CreateTerrainMesh(size - 1, Resources.Load<Material>("GroundTriplanar"));
        TerrainGenerator.DeformTerrainMesh(height, colors, plateau, currentTerrainMeshGO);

        if (!smooth)
            TerrainGenerator.RemoveSharedVertices(currentTerrainMeshGO.GetComponent<MeshFilter>().sharedMesh);

        
        AddTrees(.1f, .6f);

        return texture;
    }

    public void AddTrees(float addPercentualRandomChance, float pushToGroundDistance)
    {
        GeneratedTrees.ForEach(treeObject => DestroyImmediate(treeObject));
        GeneratedTrees = new List<GameObject>();

        if (currentTerrainMeshGO != null)
        {
            Stack<Vector3> niceObjectVertexPositions = TerrainGenerator.GetVerticesWithNormalAngleUpTreshold(currentTerrainMeshGO.GetComponent<MeshFilter>().mesh, 15f);
            while (niceObjectVertexPositions.Count > 0)
            {
                Vector3 possibleTreePos = niceObjectVertexPositions.Pop();
                if (Random.Range(0f, 1f) < .01f)
                {
                    GameObject tree = Instantiate<GameObject>(Resources.Load<GameObject>("Tree9_2"));
                    tree.transform.position = possibleTreePos;
                    tree.transform.Translate(Vector3.down * pushToGroundDistance, Space.Self);
                    tree.transform.Rotate(new Vector3(0, Random.Range(-90f, 90f), 0));
                    float randomUniformScale = Random.Range(.5f, 1f);
                    tree.transform.localScale = Vector3.one * randomUniformScale;
                    GeneratedTrees.Add(tree);
                }
            }
        }
    }

    public void SaveTerrainAsset()
    {

        if (currentTerrainMeshGO != null)
        {
            Mesh meshToSave = currentTerrainMeshGO.GetComponent<MeshFilter>().mesh;

            string filePath =
            EditorUtility.SaveFilePanelInProject("Save Terrain Mesh Asset", "ProceduralTerrainMesh", "asset", "");
            if (filePath == "") return;
            AssetDatabase.CreateAsset(meshToSave, filePath);
        }
        else
            Debug.LogError("Generate terrain first!");
    }

    #endregion

    #region private

    private void HandleMouseRightDown(bool b)
    {
        mouseRightDrag = b;

        if (b)
        {
            mouseRightDownPosition = Input.mousePosition;
            rightDownHitpoint = ScreenPointRaycast(Input.mousePosition)?.point ?? null;
        }
        else
            mouseRightDownDelta = Vector3.zero;
    }

    private RaycastHit? ScreenPointRaycast(Vector3 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
            return hit;
        else
            return null;
    }

    private GameObject CreateTerrainMesh(int _size, Material _material)
    {
        GameObject terrainPlane = TerrainGenerator.CreatePlane(_size);
        terrainPlane.GetComponent<Renderer>().sharedMaterial = _material;

        return terrainPlane;
    }

    #endregion

    #region Unity
    private void FixedUpdate()
    {
        Vector3? hitpoint = ScreenPointRaycast(Input.mousePosition)?.point ?? null;

        //Terrain tools
        if (leftMouseDown && hitpoint.HasValue && currentTerrainMeshGO != null)
        {
            if (shiftDown) //Flatten mode
                TerrainGenerator.PushPullVerticesBrush(hitpoint.Value, currentTerrainMeshGO, CurrentBrushSize, 0f, true);
            else if (ctrlDown) //Pull mode
                TerrainGenerator.PushPullVerticesBrush(hitpoint.Value, currentTerrainMeshGO, CurrentBrushSize, CurrentBrushStrength, false);
            else if (altDown) //Push mode
                TerrainGenerator.PushPullVerticesBrush(hitpoint.Value, currentTerrainMeshGO, CurrentBrushSize, -CurrentBrushStrength, false);
        }
        //Rotate camera
        else if (mouseRightDrag && rightDownHitpoint.HasValue)
        {
            mouseRightDownDelta = Input.mousePosition - mouseRightDownPosition;
            CamController.RotateAroundWorldPoint(rightDownHitpoint.Value, .2f * mouseRightDownDelta.x + Mathf.Sign(mouseRightDownDelta.x) * 50f);
        }

        //Keep fps cam in same pos/rot as top-down cam when FPS player not active.
        //This gives nicer effect of zooming in (it's really fast lerp though) when dropping the player to terrain.
        if (!FPSPlayer.gameObject.activeInHierarchy)
        {
            FPSPlayer.FirstPersonCamera.Cam.transform.position = CamController.Cam.transform.position;
            FPSPlayer.FirstPersonCamera.Cam.transform.rotation = CamController.Cam.transform.rotation;
        }

        #endregion
    }
}
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

        Color[] colors;

        if (withOctaves) //either generate more complex noise with octaves, persistence and lacunarity...
            colors = Noise.PerlinNoiseWithOctaves(size, size, seed, noiseScale, offset, octaves, 0.5f, lacunarity);
        else //...or simple (smooth) noise with single perlin noise sample
            colors = Noise.SimplePerlinNoise(size, size, seed, noiseScale, offset);
        
        Texture2D texture = Noise.ColorArrayToTexture(colors, size, size);

        if (currentTerrainMeshGO != null)
            GameObject.DestroyImmediate(currentTerrainMeshGO);

        currentTerrainMeshGO = CreateTerrainMesh(size - 1, Resources.Load<Material>("GroundTriplanar"));
        TerrainGenerator.DeformTerrainMesh(height, colors, plateau, currentTerrainMeshGO);

        if (!smooth)
            TerrainGenerator.RemoveSharedVertices(currentTerrainMeshGO.GetComponent<MeshFilter>().sharedMesh);

        return texture;
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
        

        #endregion
    }
}
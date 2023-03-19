using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIController : MonoBehaviour
{
    //Buttons
    public Button ButtonGenerateTerrain;
    public Button ButtonGenerateFancierTerrain;
    public Button ButtonSaveAsAsset;
    //TMP InputFields
    public TMP_InputField InputFieldSize;
    public TMP_InputField InputFieldHeight;
    public TMP_InputField InputNoiseScale;
    //Toggles
    public Toggle ToggleSmooth;
    public Toggle TogglePlateau;
    //Sliders
    public Slider SliderBrushSize;
    public Slider SliderBrushStrength;
    public Slider SliderOctaves;
    public Slider SliderLacunarity;
    //Raw image for terrain texture
    public RawImage RawImageNoiseTexture;

    #region Private

    /// <summary>
    /// Call GenerateTerrainAndNoiseTexture with UI values.
    /// </summary>
    /// <param name="fancy">Whether or not to use more sophisticated noise generation.</param>
    private void HandleButtonGenerateTerrainClick(bool fancy)
    {
        int size = Int32.Parse(InputFieldSize.text);

        RawImageNoiseTexture.texture =
            ApplicationController.Instance.GenerateTerrainAndNoiseTexture(
                size, 
                float.Parse(InputFieldHeight.text), 
                float.Parse(InputNoiseScale.text), 
                ToggleSmooth.isOn, 
                TogglePlateau.isOn, 
                (int)SliderOctaves.value, 
                SliderLacunarity.value, 
                fancy);
    }

    /// <summary>
    /// Call SaveTerrainAsset to save generated terrain mesh as an asset.
    /// </summary>
    private void HandleButtonSaveAsAsset()
    {
        ApplicationController.Instance.SaveTerrainAsset();
    }
    #endregion

    #region Unity
    /// <summary>
    /// Start listening UI events
    /// </summary>
    private void Awake()
    {
        ButtonGenerateTerrain.onClick.AddListener(() => HandleButtonGenerateTerrainClick(false));
        ButtonGenerateFancierTerrain.onClick.AddListener(() => HandleButtonGenerateTerrainClick(true));
        ButtonSaveAsAsset.onClick.AddListener(HandleButtonSaveAsAsset);

        SliderBrushSize.onValueChanged.AddListener(value => ApplicationController.Instance.SetBrushSizeAndStrength(value, SliderBrushStrength.value));
        SliderBrushStrength.onValueChanged.AddListener(value => ApplicationController.Instance.SetBrushSizeAndStrength(SliderBrushSize.value, value));
    }
    #endregion
}

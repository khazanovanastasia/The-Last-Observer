using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

    public event System.Action<ViewMode> OnModeChanged;
    public event System.Action<int, bool> OnStaticChanged; // (cameraIndex, hasStatic)
    public event System.Action<int> OnCameraSelected;
    public event Action<bool> OnPauseStateChanged;

    [Header("References")]
    public FirstPersonLook firstPersonLook;
    public CameraGridUI cameraGridUI;
    public DetailViewUI detailViewUI;
    public BlueprintUI blueprintUI;
    public PauseUI pauseUI;

    [Header("Camera Settings")]
    public Camera[] securityCameras;
    public Vector2 gridImageSize = new Vector2(170, 128);
    public Vector2 detailImageSize = new Vector2(680, 512);
    public float renderUpdateInterval = 0.1f;

    [Header("Static Effect Settings")]
    [Tooltip("Number of initially broken cameras based on difficulty")]
    public int easyModeStaticCount = 3;
    public int normalModeStaticCount = 5;
    public int hardModeStaticCount = 8;

    [Tooltip("Chance per second for a working camera to break during gameplay")]
    [Range(0f, 1f)]
    public float easyModeBreakChance = 0.001f; 
    [Range(0f, 1f)]
    public float normalModeBreakChance = 0.003f; 
    [Range(0f, 1f)]
    public float hardModeBreakChance = 0.005f;

    [Header("Difficulty Settings")]
    public DifficultyLevel currentDifficulty = DifficultyLevel.Normal;

    private CameraData[] cameras;
    private Coroutine renderCoroutine;
    private Coroutine cameraBreakCoroutine;
    // private Coroutine gridUpdateCoroutine;
    // private Coroutine overviewCoroutine;

    private ViewMode currentMode = ViewMode.Panopticon;

    private ViewMode modeBeforePause;
    private bool isPaused = false;

    #region Initialization
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeCameras();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCameras()
    {
        ShuffleCameras();

        cameras = new CameraData[securityCameras.Length];

        for (int i = 0; i < securityCameras.Length; i++)
        {
            cameras[i] = new CameraData(
                securityCameras[i],
                i,
                gridImageSize,
                detailImageSize
            );
        }

        InitializeStaticCameras();
    }

    private void ShuffleCameras()
    {
        for (int i = securityCameras.Length - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Camera temp = securityCameras[i];
            securityCameras[i] = securityCameras[randomIndex];
            securityCameras[randomIndex] = temp;
        }
    }

    private void InitializeStaticCameras()
    {
        int staticCount = GetStaticCameraCount();

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < cameras.Length; i++)
        {
            availableIndices.Add(i);
        }

        for (int i = 0; i < staticCount && availableIndices.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            int cameraIndex = availableIndices[randomIndex];

            SetCameraStatic(cameraIndex, true);
            availableIndices.RemoveAt(randomIndex);
        }
    }

    private int GetStaticCameraCount()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                return easyModeStaticCount;
            case DifficultyLevel.Normal:
                return normalModeStaticCount;
            case DifficultyLevel.Hard:
                return hardModeStaticCount;
            default:
                return normalModeStaticCount;
        }
    }

    private float GetCameraBreakChance()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                return easyModeBreakChance;
            case DifficultyLevel.Normal:
                return normalModeBreakChance;
            case DifficultyLevel.Hard:
                return hardModeBreakChance;
            default:
                return normalModeBreakChance;
        }
    }
    #endregion

    #region Mode Switching
    public void SwitchToOverview()
    {
        StopRenderCoroutine();

        currentMode = ViewMode.CameraOverview;
        firstPersonLook.enabled = false;

        SwitchToGridTextures();
        EnableAllCameras();

        renderCoroutine = StartCoroutine(UpdateGridTexturesCoroutine());

        OnModeChanged?.Invoke(ViewMode.CameraOverview);

        SetCursorState(true);
    }

    public void SwitchToDetailView(int cameraIndex)
    {
        if (cameraIndex < 0 || cameraIndex >= cameras.Length)
        {
            Debug.LogWarning($"Invalid camera index: {cameraIndex}");
            return;
        }

        StopRenderCoroutine();

        currentMode = ViewMode.DetailView;

        SwitchToDetailTextures();
        EnableOnlyCamera(cameraIndex);

        if (!cameras[cameraIndex].hasStatic)
        {
            cameras[cameraIndex].camera.Render();
        }

        renderCoroutine = StartCoroutine(UpdateDetailTextureCoroutine(cameraIndex));

        OnModeChanged?.Invoke(ViewMode.DetailView);
        OnCameraSelected?.Invoke(cameraIndex);

        SetCursorState(true);
    }

    public void SwitchToFloorPlanView()
    {
        currentMode = ViewMode.FloorPlan;
        firstPersonLook.enabled = false;

        DisableAllCameras();

        OnModeChanged?.Invoke(ViewMode.FloorPlan);

        SetCursorState(true);
    }

    public void SwitchToPanopticon()
    {
        StopRenderCoroutine();

        currentMode = ViewMode.Panopticon;

        DisableAllCameras();
        firstPersonLook.enabled = true;

        OnModeChanged?.Invoke(ViewMode.Panopticon);

        SetCursorState(false);
    }

    public void SwitchToNextCamera()
    {
        if (currentMode != ViewMode.DetailView) return;

        int currentIndex = detailViewUI.GetCurrentCameraIndex();
        int nextIndex = (currentIndex + 1) % cameras.Length;

        SwitchToDetailView(nextIndex);
    }

    public void SwitchToPreviousCamera()
    {
        if (currentMode != ViewMode.DetailView) return;

        int currentIndex = detailViewUI.GetCurrentCameraIndex();
        int prevIndex = (currentIndex - 1 + cameras.Length) % cameras.Length;

        SwitchToDetailView(prevIndex);
    }
    #endregion

    #region Camera Control
    private void EnableAllCameras()
    {
        foreach (CameraData data in cameras)
        {
            if (!data.hasStatic)
            {
                data.camera.enabled = true;
            }
        }
    }

    private void DisableAllCameras()
    {
        foreach (CameraData data in cameras)
        {
            data.camera.enabled = false;
        }
    }

    public void EnableOnlyCamera(int cameraIndex)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].camera.enabled = (i == cameraIndex && !cameras[i].hasStatic);
        }
    }

    private void SwitchToGridTextures()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].camera.targetTexture = cameras[i].gridTexture;
        }
    }

    private void SwitchToDetailTextures()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].camera.targetTexture = cameras[i].detailTexture;
        }
    }
    #endregion

    #region Static Camera Management
    public void SetCameraStatic(int index, bool isStatic)
    {
        if (index < 0 || index >= cameras.Length)
        {
            Debug.LogWarning($"Invalid camera index: {index}");
            return;
        }

        cameras[index].hasStatic = isStatic;

        if (isStatic)
        {
            cameras[index].camera.enabled = false;
        }

        OnStaticChanged?.Invoke(index, isStatic);
    }

    /*
    /// <summary>
    /// Resets all cameras to working state and re-randomizes static cameras
    /// Use this when starting a new game
    /// </summary>
    public void ResetStaticCameras()
    {
        // Clear all static states
        for (int i = 0; i < cameras.Length; i++)
        {
            SetCameraStatic(i, false);
        }

        // Re-initialize with new random selection
        InitializeStaticCameras();
    }*/

    public void StartCameraBreakingSystem()
    {
        if (cameraBreakCoroutine == null)
        {
            cameraBreakCoroutine = StartCoroutine(RandomCameraBreakCoroutine());
        }
    }

    public void StopCameraBreakingSystem()
    {
        if (cameraBreakCoroutine != null)
        {
            StopCoroutine(cameraBreakCoroutine);
            cameraBreakCoroutine = null;
        }
    }

    private IEnumerator RandomCameraBreakCoroutine()
    {
        float checkInterval = 1f; 
        float breakChance = GetCameraBreakChance();

        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            List<int> workingCameras = new List<int>();
            for (int i = 0; i < cameras.Length; i++)
            {
                if (!cameras[i].hasStatic)
                {
                    workingCameras.Add(i);
                }
            }

            if (workingCameras.Count > 0 && UnityEngine.Random.value < breakChance)
            {
                int randomIndex = UnityEngine.Random.Range(0, workingCameras.Count);
                int cameraToBreak = workingCameras[randomIndex];

                SetCameraStatic(cameraToBreak, true);
            }
        }
    }
    #endregion

    #region Rendering Coroutines
    private IEnumerator UpdateGridTexturesCoroutine()
    {
        while (currentMode == ViewMode.CameraOverview)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].camera.enabled && !cameras[i].hasStatic)
                {
                    cameras[i].camera.Render();
                }
            }
            yield return new WaitForSeconds(renderUpdateInterval);
        }
    }

    private IEnumerator UpdateDetailTextureCoroutine(int cameraIndex)
    {
        while (currentMode == ViewMode.DetailView)
        {
            if (cameras[cameraIndex].camera.enabled && !cameras[cameraIndex].hasStatic)
            {
                cameras[cameraIndex].camera.Render();
            }
            yield return new WaitForSeconds(renderUpdateInterval);
        }
    }

    private void StopRenderCoroutine()
    {
        if (renderCoroutine != null)
        {
            StopCoroutine(renderCoroutine);
            renderCoroutine = null;
        }
    }
    #endregion

    #region Pause System
    public void TogglePause()
    {
        if (isPaused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        modeBeforePause = currentMode;

        Time.timeScale = 0f;

        StopCameraBreakingSystem();

        if (firstPersonLook != null)
        {
            firstPersonLook.enabled = false;
        }

        OnPauseStateChanged?.Invoke(true);

        SetCursorState(true);
    }

    public void Unpause()
    {
        if (!isPaused) return;

        isPaused = false;

        Time.timeScale = 1f;

        StartCameraBreakingSystem();
        firstPersonLook.enabled = true;
        /*
        if (modeBeforePause == ViewMode.Panopticon)
        {
            StartCameraBreakingSystem();
        }*/
        /*
        if (modeBeforePause == ViewMode.Panopticon && firstPersonLook != null)
        {
            firstPersonLook.enabled = true;
        }*/

        OnPauseStateChanged?.Invoke(false);
        
        SetCursorState(false);
        //SetCursorState(modeBeforePause != ViewMode.Panopticon);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
    #endregion

    #region Utility Methods
    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.Confined : CursorLockMode.Locked;
    }
    #endregion

    #region Public Getters
    public Camera GetCamera(int index)
    {
        if (index >= 0 && index < cameras.Length)
            return cameras[index].camera;
        return null;
    }

    public CameraData GetCameraData(int index)
    {
        if (index >= 0 && index < cameras.Length)
            return cameras[index];
        return null;
    }

    public RenderTexture GetGridTexture(int index)
    {
        if (index >= 0 && index < cameras.Length)
            return cameras[index].gridTexture;
        return null;
    }

    public RenderTexture GetDetailTexture(int index)
    {
        if (index >= 0 && index < cameras.Length)
            return cameras[index].detailTexture;
        return null;
    }

    public int GetCameraCount()
    {
        return cameras.Length;
    }

    public ViewMode GetCurrentMode()
    {
        return currentMode;
    }

    public bool IsPlayerMonitoringCameras()
    {
        return currentMode == ViewMode.CameraOverview || currentMode == ViewMode.DetailView;
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        StopCameraBreakingSystem();

        if (cameras != null)
        {
            foreach (CameraData data in cameras)
            {
                data.Release();
            }
        }
    }
    #endregion
}

#region Enums
public enum ViewMode
{
    Panopticon,
    CameraOverview,
    DetailView,
    FloorPlan
}

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard
}
#endregion

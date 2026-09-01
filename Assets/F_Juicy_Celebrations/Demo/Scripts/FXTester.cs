using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace Fateloom
{
public class FXTester : MonoBehaviour
{
    [Serializable]
    public class FXEntry
    {
        public string effectName = "New Effect";
        public GameObject prefab;
    }

    [Header("FX Library")]
    public List<FXEntry> effects = new List<FXEntry>();

    [Header("Spawn")]
    public Transform[] spawnLocations;
    public bool cycleSpawnPoints = true;
    public float cameraSpawnDistance = 5f;
    public float autoDestroyTime = 8f;

    [Header("UI")]
    public TextMeshProUGUI fxNameDisplay;

    private int _currentIndex = 0;
    private int _currentSpawnIndex = 0;
    private List<GameObject> _spawnedEffects = new List<GameObject>();

    private InputAction _nextAction;
    private InputAction _prevAction;
    private InputAction _spawnAction;
    private InputAction _clearAction;

    private void Awake()
    {
        _nextAction = new InputAction("NextFX", InputActionType.Button);
        _nextAction.AddBinding("<Keyboard>/e");

        _prevAction = new InputAction("PrevFX", InputActionType.Button);
        _prevAction.AddBinding("<Keyboard>/q");

        _spawnAction = new InputAction("SpawnFX", InputActionType.Button);
        _spawnAction.AddBinding("<Keyboard>/space");
        _spawnAction.AddBinding("<Mouse>/leftButton");

        _clearAction = new InputAction("ClearFX", InputActionType.Button);
        _clearAction.AddBinding("<Keyboard>/x");

        UpdateDisplay();
    }

    private void OnEnable()
    {
        _nextAction?.Enable();
        _prevAction?.Enable();
        _spawnAction?.Enable();
        _clearAction?.Enable();
    }

    private void OnDisable()
    {
        _nextAction?.Disable();
        _prevAction?.Disable();
        _spawnAction?.Disable();
        _clearAction?.Disable();
    }

    private void OnDestroy()
    {
        _nextAction?.Dispose();
        _prevAction?.Dispose();
        _spawnAction?.Dispose();
        _clearAction?.Dispose();
    }

    private void Update()
    {
        if (_nextAction.WasPerformedThisFrame()) CycleEffect(1);
        if (_prevAction.WasPerformedThisFrame()) CycleEffect(-1);
        if (_spawnAction.WasPerformedThisFrame()) SpawnCurrent();
        if (_clearAction.WasPerformedThisFrame()) ClearAll();
    }

    private void CycleEffect(int dir)
    {
        if (effects.Count == 0) return;
        _currentIndex = (_currentIndex + dir + effects.Count) % effects.Count;
        UpdateDisplay();
    }

    private void SpawnCurrent()
    {
        if (effects.Count == 0 || effects[_currentIndex].prefab == null) return;

        Vector3 pos;
        Quaternion rot;
        GetSpawnPoint(out pos, out rot);

        GameObject obj = Instantiate(effects[_currentIndex].prefab, pos, rot);
        obj.name = effects[_currentIndex].effectName;
        _spawnedEffects.Add(obj);

        if (autoDestroyTime > 0f) Destroy(obj, autoDestroyTime);

        if (spawnLocations != null && spawnLocations.Length > 0 && cycleSpawnPoints)
            _currentSpawnIndex = (_currentSpawnIndex + 1) % spawnLocations.Length;
    }

    private void GetSpawnPoint(out Vector3 pos, out Quaternion rot)
    {
        if (spawnLocations != null && spawnLocations.Length > 0)
        {
            Transform t = spawnLocations[cycleSpawnPoints ? _currentSpawnIndex : UnityEngine.Random.Range(0, spawnLocations.Length)];
            if (t != null) { pos = t.position; rot = t.rotation; return; }
        }

        Camera cam = Camera.main;
        if (cam != null) { pos = cam.transform.position + cam.transform.forward * cameraSpawnDistance; rot = Quaternion.identity; }
        else { pos = Vector3.zero; rot = Quaternion.identity; }
    }

    private void ClearAll()
    {
        foreach (var obj in _spawnedEffects) if (obj != null) Destroy(obj);
        _spawnedEffects.Clear();
    }

    private void UpdateDisplay()
    {
        if (fxNameDisplay == null) return;
        if (effects.Count == 0) { fxNameDisplay.text = "---"; return; }
        fxNameDisplay.text = $"{effects[_currentIndex].effectName}  ({_currentIndex + 1}/{effects.Count})";
    }
}
}

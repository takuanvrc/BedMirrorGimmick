using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public enum ToggleState
{
    OFF = 0,
    ON = 1
}

public class BedColliderToggle : UdonSharpBehaviour
{
    [Header("デフォルトの状態")]
    [SerializeField] private ToggleState currentState = ToggleState.OFF;
    
    [Header("BedCollider")]
    [SerializeField] private GameObject[] onObjects; // ONの時に表示するオブジェクト
    
    [Header("デバッグ")]
    [SerializeField] private bool enableDebugLog = false;

    void Start()
    {
        // 初期状態を設定
        UpdateState();
    }

    public override void Interact()
    {
        // ステートを切り替える（OFF⇆ON）
        currentState = (currentState == ToggleState.OFF) ? ToggleState.ON : ToggleState.OFF;
        
        if (enableDebugLog)
        {
            Debug.Log($"[BedColliderToggle] ステートが {currentState} に変更されました");
        }
        
        UpdateState();
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case ToggleState.OFF: // 全てOFF
                SetObjectsActive(onObjects, false);
                if (enableDebugLog) Debug.Log("[BedColliderToggle] ステートOFF: オブジェクトOFF");
                break;
                
            case ToggleState.ON: // ONオブジェクトを表示
                SetObjectsActive(onObjects, true);
                if (enableDebugLog) Debug.Log("[BedColliderToggle] ステートON: オブジェクトON");
                break;
        }
    }

    private void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null) return;
        
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    // エディタ用：手動でステートをリセット
    [ContextMenu("Reset to State OFF")]
    public void ResetState()
    {
        currentState = ToggleState.OFF;
        UpdateState();
    }
    
    // エディタ用：ステートを切り替え
    [ContextMenu("Toggle State")]
    public void ToggleStateManual()
    {
        Interact();
    }
    
    // エディタ用：ONステートに設定
    [ContextMenu("Set to ON")]
    public void SetToON()
    {
        currentState = ToggleState.ON;
        UpdateState();
    }
}
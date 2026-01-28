using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public enum MirrorState
{
    OFF = 0,
    LowMirror = 1,
    HighMirror = 2
}

public class BedMirrorToggle : UdonSharpBehaviour
{
    [Header("デフォルトの状態")]
    [SerializeField] private MirrorState currentState = MirrorState.OFF;
    
    [Header("モード設定")]
    [SerializeField, Tooltip("ONにするとLowMirrorだけ出ます")]
    private bool LowMirrorのみ = false; // LowMirrorのみ
    
    [SerializeField, Tooltip("ONにするとHighMirrorだけ出ます")]
    private bool HighMirrorのみ = false; // HighMirrorのみ
    
    [Header("ボタンの色設定")]
    [SerializeField] private Color offColor = new Color(0.827f, 0.910f, 0.929f, 0.502f);     // ステート0の色
    [SerializeField] private Color state1Color = new Color(0.373f, 0.702f, 0.784f, 0.502f);  // ステート1の色
    [SerializeField] private Color state2Color = new Color(0.000f, 0.737f, 1.000f, 0.502f);   // ステート2の色
    
    [Header("--- Options ---")]
    
    [SerializeField] private GameObject[] state1Objects; // ステート1でONにするオブジェクト
    
    
    [SerializeField] private GameObject[] state2Objects; // ステート2でONにするオブジェクト
    
    
    [SerializeField] private Renderer buttonRenderer; // ボタンのRenderer
    
    
    [SerializeField] private bool enableDebugLog = false;
    
    // 前回の状態を記録（排他制御用）
    private bool _prevLowMirrorOnly = false;
    private bool _prevHighMirrorOnly = false;

    void Start()
    {
        // 初期状態を記録
        _prevLowMirrorOnly = LowMirrorのみ;
        _prevHighMirrorOnly = HighMirrorのみ;
        
        // 初期状態を設定
        UpdateState();
    }

    // インスペクターで値が変更された時の排他制御
    void OnValidate()
    {
        // LowMirrorのみが新しくONになった場合
        if (LowMirrorのみ && !_prevLowMirrorOnly)
        {
            HighMirrorのみ = false;
            if (enableDebugLog) Debug.Log("[BedMirrorToggle] LowMirrorのみがONになったため、HighMirrorのみをOFFにしました");
        }
        // HighMirrorのみが新しくONになった場合
        else if (HighMirrorのみ && !_prevHighMirrorOnly)
        {
            LowMirrorのみ = false;
            if (enableDebugLog) Debug.Log("[BedMirrorToggle] HighMirrorのみがONになったため、LowMirrorのみをOFFにしました");
        }
        
        // 前回の状態を更新
        _prevLowMirrorOnly = LowMirrorのみ;
        _prevHighMirrorOnly = HighMirrorのみ;
    }

    public override void Interact()
    {
        // ステートを次に進める
        if (LowMirrorのみ)
        {
            // LowMirrorのみモード: OFF→LowMirror→OFF...の循環
            currentState = (currentState == MirrorState.OFF) ? MirrorState.LowMirror : MirrorState.OFF;
        }
        else if (HighMirrorのみ)
        {
            // HighMirrorのみモード: OFF→HighMirror→OFF...の循環
            currentState = (currentState == MirrorState.OFF) ? MirrorState.HighMirror : MirrorState.OFF;
        }
        else
        {
            // 通常モード: OFF→LowMirror→HighMirror→OFF...の循環
            currentState = (MirrorState)(((int)currentState + 1) % 3);
        }
        
        if (enableDebugLog)
        {
            string mode = LowMirrorのみ ? "LowMirrorのみモード" : 
                         HighMirrorのみ ? "HighMirrorのみモード" : "3ステートモード";
            Debug.Log($"[BedMirrorToggle] {mode} - ステートが {currentState} に変更されました");
        }
        
        UpdateState();
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case MirrorState.OFF: // 全てOFF
                SetObjectsActive(state1Objects, false);
                SetObjectsActive(state2Objects, false);
                SetButtonColor(offColor);
                if (enableDebugLog) Debug.Log("[BedMirrorToggle] ステートOFF: 全てOFF");
                break;
                
            case MirrorState.LowMirror: // LowMirrorのみON
                SetObjectsActive(state1Objects, true);
                SetObjectsActive(state2Objects, false);
                SetButtonColor(state1Color);
                if (enableDebugLog) Debug.Log("[BedMirrorToggle] ステートLowMirror: LowMirrorオブジェクトON");
                break;
                
            case MirrorState.HighMirror: // HighMirrorのみON
                SetObjectsActive(state1Objects, false);
                SetObjectsActive(state2Objects, true);
                SetButtonColor(state2Color);
                if (enableDebugLog) Debug.Log("[BedMirrorToggle] ステートHighMirror: HighMirrorオブジェクトON");
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

    private void SetButtonColor(Color color)
    {
        if (buttonRenderer != null)
        {
            if (buttonRenderer.material != null)
            {
                buttonRenderer.material.color = color;
            }
        }
    }

    // エディタ用：手動でステートをリセット
    [ContextMenu("Reset to State OFF")]
    public void ResetState()
    {
        currentState = MirrorState.OFF;
        UpdateState();
    }
    
    // エディタ用：次のステートに進める
    [ContextMenu("Next State")]
    public void NextState()
    {
        Interact();
    }
    
    // エディタ用：LowMirrorのみモードの切り替え
    [ContextMenu("Toggle LowMirror Only Mode")]
    public void ToggleLowMirrorOnlyMode()
    {
        LowMirrorのみ = !LowMirrorのみ;
        if (LowMirrorのみ) HighMirrorのみ = false; // 他のモードを無効化
        
        if (enableDebugLog)
        {
            string mode = LowMirrorのみ ? "LowMirrorのみモード" : "3ステートモード";
            Debug.Log($"[BedMirrorToggle] {mode} に変更されました");
        }
        
        // モード変更時の状態調整
        AdjustStateForMode();
    }
    
    // エディタ用：HighMirrorのみモードの切り替え
    [ContextMenu("Toggle HighMirror Only Mode")]
    public void ToggleHighMirrorOnlyMode()
    {
        HighMirrorのみ = !HighMirrorのみ;
        if (HighMirrorのみ) LowMirrorのみ = false; // 他のモードを無効化
        
        if (enableDebugLog)
        {
            string mode = HighMirrorのみ ? "HighMirrorのみモード" : "3ステートモード";
            Debug.Log($"[BedMirrorToggle] {mode} に変更されました");
        }
        
        // モード変更時の状態調整
        AdjustStateForMode();
    }
    
    // モード変更時に適切な状態に調整
    private void AdjustStateForMode()
    {
        if (LowMirrorのみ && currentState == MirrorState.HighMirror)
        {
            // LowMirrorのみモードでHighMirrorにいる場合、OFFに戻す
            currentState = MirrorState.OFF;
            UpdateState();
        }
        else if (HighMirrorのみ && currentState == MirrorState.LowMirror)
        {
            // HighMirrorのみモードでLowMirrorにいる場合、OFFに戻す
            currentState = MirrorState.OFF;
            UpdateState();
        }
    }
}
/// <summary>
/// author: Toshiki Nanbo
/// カーソルの初期化と十字キーの入力を処理するクラス
/// </summary>

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{

    [SerializeField] VirtualMouseInput virtualMouse;
    private KeyActions keyActions;

    // HUDボタンリスト
    private List<Button> hudButtons = new List<Button>();
    // ダイアログボタンリスト
    private List<Button> dialogButtons = new List<Button>();

    // HUDボタンリスト
    private List<ScrollRect> hudScroll = new List<ScrollRect>();
    // ダイアログボタンリスト
    private List<ScrollRect> dialogScroll = new List<ScrollRect>();

    // ダイアログが開いているか
    private bool isDialogOpen = false;
    // HUD判定用Canvas（必要に応じて設定）
    [SerializeField] GameObject hudField;
    // ダイアログ判定用Canvas（必要に応じて設定）
    [SerializeField] GameObject dialogField;

    void Start()
    {
        virtualMouse.enabled = true;
        keyActions = new KeyActions();
        keyActions.Cross.Cross.performed += OnMoveCursor;
        keyActions.Enable();

        // HUDボタンリスト取得
        if (hudField != null)
        {
            hudButtons = new List<Button>(hudField.GetComponentsInChildren<Button>(true));
            hudScroll = new List<ScrollRect>(hudField.GetComponentsInChildren<ScrollRect>(true));
        }
        // ダイアログボタンリストはダイアログ開いた時に取得

        //カーソル初期位置調整
        var position = new Vector2(Screen.width/2 , Screen.height/2);
        virtualMouse.cursorTransform.anchoredPosition = position;
        InputState.Change(virtualMouse.virtualMouse.position, position);
    }

    void Update()
    {
        OnMoveScroll();
    }

    void OnDestroy()
    {
        keyActions.Disable();
        keyActions.Dispose();
    }

    public void OnMoveCursor(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        
        // カーソルのスクリーン座標を取得
        var cursorTf = virtualMouse.cursorTransform;
        Camera mainCam = Camera.main;
        Vector2 cursorScreenPos = RectTransformUtility.WorldToScreenPoint(mainCam, cursorTf.position);
        // 対象ボタンリスト
        List<Button> targetButtons = isDialogOpen ? dialogButtons : hudButtons;
        
        // 表示中かつ画面内のボタンのみ抽出
        // 入力方向で内積一定以下で一番近いボタンを選択
        Vector2 nearestBtnCursorPos = Vector2.zero;
        Button nearestBtn = null;
        foreach (var btn in targetButtons)
        {
            if (!btn.gameObject.activeInHierarchy) continue;
            var rectTrans = btn.GetComponent<RectTransform>();
            if (rectTrans == null) continue;

            PointerEventData pointer = new PointerEventData(EventSystem.current) {
                position = RectTransformUtility.WorldToScreenPoint(mainCam, rectTrans.position)
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);
            if (!results.Any(r => r.gameObject == btn.gameObject)) continue; // マスクされている場合は対象外

            Vector2 btnPos = rectTrans.anchoredPosition;
            // カーソルのローカル座標系に変換
            Vector2 cursorLocalPos = -rectTrans.InverseTransformPoint(cursorTf.position);
            
            //現在選択中の判定
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTrans, cursorScreenPos, mainCam)) continue;

            Vector2 toBtn = btnPos - cursorLocalPos;
            Vector2 dir = toBtn.normalized;
            float dot = Vector2.Dot(cursorLocalPos.normalized, input.normalized);
            if (dot > 0.5f) // ある程度入力方向に近いものだけ対象
            {
                if (nearestBtnCursorPos != Vector2.zero) {
                    if(nearestBtnCursorPos.magnitude > cursorLocalPos.magnitude) {
                        nearestBtn = btn;
                        nearestBtnCursorPos = cursorLocalPos;
                    }
                } else {
                    nearestBtn = btn;
                    nearestBtnCursorPos = cursorLocalPos;
                }
            }
            
        }

        if (nearestBtn != null)
        {
            // ボタン位置にカーソル移動（スクリーン座標→ローカル座標変換）
            var rectTrans = nearestBtn.GetComponent<RectTransform>();
            Vector2 btnScreenPos = RectTransformUtility.WorldToScreenPoint(mainCam, rectTrans.position);
            RectTransform cursorParent = virtualMouse.cursorTransform.parent as RectTransform;
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(cursorParent, btnScreenPos, mainCam, out localPos);
            localPos.x += Screen.width/2;
            localPos.y += Screen.height/2;
            virtualMouse.cursorTransform.anchoredPosition = localPos;
            InputState.Change(virtualMouse.virtualMouse.position, localPos);
        }

    }

    public void OnMoveScroll()
    {
        //yの入力が小さい、選択していない場合はreturn
        Vector2 input = keyActions.Cross.Scroll.ReadValue<Vector2>();
        if (Mathf.Abs(input.y) < 0.1f) return;
        
        // カーソルのスクリーン座標を取得
        var cursorTf = virtualMouse.cursorTransform;
        Camera mainCam = Camera.main;
        Vector2 cursorScreenPos = RectTransformUtility.WorldToScreenPoint(mainCam, cursorTf.position);

        List<ScrollRect> targetScroll = isDialogOpen ? dialogScroll : hudScroll;
        foreach (var target in targetScroll) {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTrans, cursorScreenPos, mainCam)) {
                var delta = input.y * 2f * Time.deltaTime;
                target.verticalNormalizedPosition += delta;
            }
        }

    }

    // ダイアログを開いた時に呼ぶ,
    // 引数：ダイアログのgameobject
    public void SetDialogButtons(GameObject go)
    {
        dialogButtons = new List<Button>(go.GetComponentsInChildren<Button>(true));
        dialogScroll = new List<ScrollRect>(go.GetComponentsInChildren<ScrollRect>(true));
        isDialogOpen = true;
    }

    // ダイアログを閉じた時に呼ぶ
    public void CloseDialog()
    {
        isDialogOpen = false;
        dialogButtons.Clear();
        dialogScroll.Clear();
    }
}


/// <summary>
/// author: Toshiki Nanbo
/// ダイアログ表示用manager
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [SerializeField] CursorController cursorController;
    public static DialogManager Instance { get; private set; }

    Stack<GameObject> dialogs = new Stack<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void CreateDialog(string dialogName)
    {
        //プレハブ生成
        var go = Instantiate((GameObject)Resources.Load(dialogName));
        go.transform.SetParent(transform, false);
        dialogs.Push(go);

        //カーソルコントローラー対応    
        cursorController.SetDialogButtons(go);
    }

    public void CloseDialog()
    {
        dialogs.Pop();
        cursorController.CloseDialog();
    }
}

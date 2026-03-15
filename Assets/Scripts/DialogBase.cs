/// <summary>
/// author: Toshiki Nanbo
/// ダイアログ共通
/// </summary>
using UnityEngine;

public class DialogBase : MonoBehaviour
{
    public void Close()
    {
        if (DialogManager.Instance != null) {
            DialogManager.Instance.CloseDialog();
            if (gameObject != null)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
            
            
        }
    }
}

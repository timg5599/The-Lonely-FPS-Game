using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class scr_CraftItemTypeMenu : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]ItemType[] itemTypes;
    [SerializeField]scr_CraftingNPC parent;

    private void OnEnable() {
        this.GetComponent<Button>().onClick.RemoveAllListeners();
        this.GetComponent<Button>().onClick.AddListener(ClickedItemMenu);
    }

    private void ClickedItemMenu() {
        Debug.Log(itemTypes.Length);
        parent.UpdateItemList(itemTypes); 
    }
}

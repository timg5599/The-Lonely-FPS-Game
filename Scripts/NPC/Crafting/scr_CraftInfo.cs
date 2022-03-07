using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class scr_CraftInfo : MonoBehaviour
{
    public scr_ItemObject itemSelected;
    [SerializeField]Text itemName;

    [SerializeField]RectTransform recipeParent;
    [SerializeField]Button createButton;
    [SerializeField]Text combineCost;
    [SerializeField]Image itemImage;

    [SerializeField]GameObject recipePrefab;
    
    [SerializeField]GameObject createAmountUI;
    [SerializeField]Text createAmount;
    [SerializeField]Button increaseButton;
    [SerializeField]Button decreaseButton;
    [SerializeField]scr_CharacterController enterplayer;

    [SerializeField]GameObject WarningText;


 private void OnEnable() {
    UpdateOnce();
    createButton.onClick.AddListener(CreateItem);
 }

void increaseAmount(){
    Debug.Log("Increase");
        
    var _input =  int.Parse(createAmount.text);;
    if (_input<100){
        _input++;
        combineCost.text = (_input * itemSelected.combinePrice).ToString("n0");
        createAmount.text = (int.Parse(createAmount.text.ToString())+1).ToString();
        foreach (Transform child in recipeParent.transform) {
        GameObject.Destroy(child.gameObject);
         }
    foreach(Recipe x in itemSelected.itemRecipe){
        var newRecipe = recipePrefab.GetComponent<scr_CraftInfoPrefab>();
        newRecipe.recipeItem = x;
        newRecipe.playerAmount = enterplayer.inventoryETC.FindAllItemsInInventory(x.recipeItem.data);

        
        var eachRecipe = Instantiate(newRecipe,Vector3.zero,Quaternion.identity);
        eachRecipe.GetComponent<scr_CraftInfoPrefab>().UpdateAmountNeed(_input);
        eachRecipe.transform.SetParent(recipeParent.transform);
     }
    }
    
}
void decreaseAmount(){
    Debug.Log("Decrease");
    var _input =  int.Parse(createAmount.text);;
    combineCost.text = (_input * itemSelected.combinePrice).ToString("n0");
    if (_input>1){
        _input--;
        createAmount.text = (int.Parse(createAmount.text.ToString())-1).ToString();
    foreach (Transform child in recipeParent.transform) {
        GameObject.Destroy(child.gameObject);
    }
    foreach(Recipe x in itemSelected.itemRecipe){
        var newRecipe = recipePrefab.GetComponent<scr_CraftInfoPrefab>();
        newRecipe.recipeItem = x;
        newRecipe.playerAmount = enterplayer.inventoryETC.FindAllItemsInInventory(x.recipeItem.data);
        var eachRecipe = Instantiate(newRecipe,Vector3.zero,Quaternion.identity);
        eachRecipe.GetComponent<scr_CraftInfoPrefab>().UpdateAmountNeed(_input);
        eachRecipe.transform.SetParent(recipeParent.transform);
        
     }
    }
   
}

public void UpdateOnce(){
    
     foreach (Transform child in recipeParent.transform) {
        GameObject.Destroy(child.gameObject);
     }
     itemName.text = itemSelected.name;
     itemImage.sprite = itemSelected.UIDisplay;
     combineCost.text = itemSelected.combinePrice.ToString("n0");
     foreach(Recipe x in itemSelected.itemRecipe){
        var newRecipe = recipePrefab.GetComponent<scr_CraftInfoPrefab>();
        newRecipe.recipeItem = x;
        newRecipe.playerAmount = enterplayer.inventoryETC.FindAllItemsInInventory(x.recipeItem.data);
        var eachRecipe = Instantiate(newRecipe,Vector3.zero,Quaternion.identity);
        eachRecipe.transform.SetParent(recipeParent.transform);
     }
     createAmount.text = 1.ToString();
    increaseButton.onClick.RemoveAllListeners();
    decreaseButton.onClick.RemoveAllListeners();
    increaseButton.onClick.AddListener(increaseAmount);
    decreaseButton.onClick.AddListener(decreaseAmount);
    if(itemSelected.IsItemQuant(itemSelected)){
        createAmountUI.SetActive(true);
    }else{
        createAmountUI.SetActive(false);
    }
    WarningText.SetActive(false);
}
void CreateItem(){
    Debug.Log(itemSelected);
    Debug.Log("CheckInventory");
    Debug.Log("CheckInventorySlot and craft");
    for(int i=0;i<int.Parse(createAmount.text);i++){
        var canCreate = true;
        foreach(Recipe x in itemSelected.itemRecipe){
            try{
                if(enterplayer.inventoryETC.FindItemOnInventory(x.recipeItem.data).amount < x.recipeAmount){
                    canCreate =false;
                }
            }catch(Exception e){
                WarningText.SetActive(true);
                
            }
        }
            if(canCreate){
                foreach(Recipe x in itemSelected.itemRecipe){
                        enterplayer.inventoryETC.FindItemOnInventory(x.recipeItem.data).SubtractAmount(x.recipeAmount);
                }
                enterplayer.playerAddItemToInv(itemSelected.CreateItem());
            }

    }
    UpdateOnce();
}
}

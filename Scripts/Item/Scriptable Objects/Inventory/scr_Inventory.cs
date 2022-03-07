using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;
using System.Runtime.Serialization;
using SimpleJSON;

public enum InterfaceType{
    Inventory,Equipment,Chest,Quest
}
[CreateAssetMenu(fileName ="New Inventory",menuName = "Inventory System/Inventory")]
public class scr_Inventory :ScriptableObject
{
    public string savePath;
    public ItemDataBaseObject database;
    public InterfaceType type;
    public Inventory Container;


    public InventorySlot[] GetSlots{get{return Container.slots;}}


 
    public bool AddItem(Item _item,int _amount){
        // Find first Empty InventorySlot
        if (EmptySlotCount <= 0)
            return false;
        InventorySlot slot = FindItemsOnInventoryNot200(_item);

        if(!database.ItemObjects[_item.Id].stackable || slot == null)
        {
            SetEmptySlot(_item, _amount);
            return true;
        }
        if(slot.amount+_amount > 200){
            var addtoSlot = 200-slot.amount;
            var leftover= _amount-addtoSlot;
            slot.AddAmount(addtoSlot);
            SetEmptySlot(_item, leftover);
            return true;
        }
        
        slot.AddAmount(_amount);
        return true;
    }
    public int EmptySlotCount
    {
        get
        {
            int counter = 0;
            for (int i = 0; i < GetSlots.Length; i++)
            {
                if (GetSlots[i].item.Id <= -1)
                {
                    counter++;
                }
            }
            return counter;
        }
    }
    public InventorySlot FindItemOnInventory(Item _item)
    {
        for (int i = 0; i < GetSlots.Length; i++)
        {
            if(GetSlots[i].item.Id == _item.Id)
            {
                return GetSlots[i];
            }
        }
        return null;
    }
    public InventorySlot FindItemsOnInventoryNot200(Item _item){
        for (int i = 0; i < GetSlots.Length; i++)
            {
                if(GetSlots[i].item.Id == _item.Id && GetSlots[i].amount <200)
                {
                   return GetSlots[i];
                }
            }
        return null;
    }
    public int FindAllItemsInInventory(Item _item){
        var total = 0;
        for (int i = 0; i < GetSlots.Length; i++)
            {
                if(GetSlots[i].item.Id == _item.Id)
                {
                   total += GetSlots[i].amount;
                }
            }
        return total;
    }
    public InventorySlot SetEmptySlot(Item _item,int _amount){
        //Used to Find first Empty Slot
           for(int i =0; i< GetSlots.Length;i++){
            if(GetSlots[i].item.Id<=-1){
                GetSlots[i].UpdateSlot(_item,_amount);
                return GetSlots[i];
            }
        }
        //Set up function when full inventory
        return null;
    }
    public void SwapItems(InventorySlot item1, InventorySlot item2){
        if(item2.CanPlaceInSlot(item1.ItemObject) && item1.CanPlaceInSlot(item2.ItemObject)){
            InventorySlot temp = new InventorySlot(item2.item,item2.amount);
            item2.UpdateSlot(item1.item,item1.amount);
            item1.UpdateSlot(temp.item,temp.amount);
        }
    }

    public void RemoveItem(Item _item){
        for(int i = 0 ; i < GetSlots.Length;i++){
            if(GetSlots[i].item==_item){
                GetSlots[i].UpdateSlot(null,0);
            }
        }
    }
    [ContextMenu("Save")]
    public void Save()
    {
        //    string saveData = JsonUtility.ToJson(this, true);
        //     BinaryFormatter bf = new BinaryFormatter();
        //     FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        //     bf.Serialize(file, saveData);
        //     file.Close();
        
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, Container);
        Debug.Log("SAVE:" +stream.Length);
        stream.Close();

    }
    [ContextMenu("Load")]
    public void Load()
    {
        //   if(File.Exists(string.Concat(Application.persistentDataPath, savePath))){
        //         BinaryFormatter bf = new BinaryFormatter();
        //         FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
        //         JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this);
        //         file.Close();
        //     }
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
            Inventory newContainer = (Inventory)formatter.Deserialize(stream);
            for(int i = 0 ; i < GetSlots.Length; i++){
                Container.slots[i].UpdateSlot(newContainer.slots[i].item,newContainer.slots[i].amount);
            }
            stream.Close();
    }
    public byte[] SaveToServer()
    {

        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        formatter.Serialize(stream, Container);

        var x = stream.ToArray();
        Debug.Log("SAVE TO SERVER:" + stream.Length);
        Debug.Log("X:" + x.Length);
        stream.Close();
        //LoadFromServer(x);

        return x;
    }
    public void LoadFromServer(byte[] x)
    {
        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream(x);
        Inventory newContainer = (Inventory)formatter.Deserialize(stream);
        
        for (int i = 0; i < GetSlots.Length; i++)
        {
            Container.slots[i].UpdateSlot(newContainer.slots[i].item, newContainer.slots[i].amount);
            Debug.Log("Loaded"+(newContainer.slots[i].item.Name));
        }
        stream.Close();
    }

    [ContextMenu("Clear")]
    public void Clear(){
        Container.Clear();
    }


    
}


[System.Serializable]
public class Inventory{
    public InventorySlot[] slots = new InventorySlot[28];
    public void Clear(){
        for(int i = 0 ; i<slots.Length;i++){
            slots[i].RemoveItem();
        }
    }

}

public delegate void SlotUpdated(InventorySlot _slot);
[System.Serializable]
public class InventorySlot{
    public ItemType[] AllowedItems = new ItemType[0];
    [System.NonSerialized]
    public scr_UserInterface parent;
    [System.NonSerialized]
    public GameObject slotDisplay;
    [System.NonSerialized]
    public SlotUpdated OnAfterUpdate;
    [System.NonSerialized]
    public SlotUpdated OnBeforeUpdate;
    public Item item = new Item();
    public int amount;
    public scr_ItemObject ItemObject
    {
        get{
            if(item.Id >=0){
                return parent.inventory.database.ItemObjects[item.Id];
            }
            return null;
        }
    }
    public bool CanPlaceInSlot(scr_ItemObject _itemObject){
        if(AllowedItems.Length <=0 || _itemObject == null||_itemObject.data.Id< 0){
            return true;
        }
        for(int i = 0; i < AllowedItems.Length;i++){
            if(_itemObject.type == AllowedItems[i]){
                return true;
            }
        }
        return false;

    }
    public InventorySlot(){
        UpdateSlot(new Item(),0);

    }
    public InventorySlot(Item _item,int _amount){
        UpdateSlot(_item,_amount);
    }   
  
    public void UpdateSlot(Item _item,int _amount){
        if(OnBeforeUpdate!=null)
            OnBeforeUpdate.Invoke(this);
        item = _item;
        amount = _amount;
        if(OnAfterUpdate!= null)
            OnAfterUpdate.Invoke(this);
        
    } 
    public void AddAmount(int value){
        UpdateSlot(item,amount+=value);
    }
    public void SubtractAmount(int value){
        if(amount-value <=0)this.RemoveItem();
        UpdateSlot(item,amount-=value);
    }
    public void RemoveItem(){
        UpdateSlot(new Item(),0);

    }

}
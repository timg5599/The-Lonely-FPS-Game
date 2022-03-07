using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;
using System.Runtime.Serialization;
using System.Linq;
using System;

public class ServerTalker : MonoBehaviour
{
    [SerializeField] string serverAddress = "http://localhost:3000/users";
    [SerializeField] string serverInvAddress = "http://localhost:3000/inventory";

    [SerializeField] scr_CharacterController player;
    // Start is called before the first frame update
    void Start()
    {
        //Make a web request to get info from the server
        //this will be a text response
    }

    public void UpdateUser(string id, string field, string amount)
    {
        StartCoroutine(UpdateUserinfo(id, field, amount));
    }
    public void GetUserByID(string id)
    {
        StartCoroutine(GetUserData(id));
    }
    public void SaveInventory(string userId,string inventoryType, byte[] byteArray)
    {
        StartCoroutine(SaveUserInventoryWithStream(userId, inventoryType, byteArray));
    }
    public void LoadInventory(string userId, string inventoryType)
    {
        StartCoroutine(LoadUserInventoryWithStream(userId, inventoryType));
    }

    public IEnumerator GetUserData(string userId)
    {
        UnityWebRequest www = UnityWebRequest.Get(serverAddress +"/"+ userId);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Something went wrong : " + www.error);
            yield return null;
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            JSONNode node = JSON.Parse(www.downloadHandler.text)[0][0];
            Debug.Log(node);
            player.playerLevel = node["level"];
            player.playerCurrentExp = node["exp"];
            player.diamond = node["diamond"];
            player.honorPoint = node["honorPoint"];
            player.coins = node["bero"];

        }
    }

    IEnumerator UpdateUserinfo(string userId, string info, string amount)
    {
        UnityWebRequest www = UnityWebRequest.Post(serverAddress + "/" + userId + "/" + info + "/" + amount, "");
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Something went wrong : " + www.error);
        }

    }
    IEnumerator SaveUserInventoryWithStream(string userId,string InventoryType, byte[] binary)
    {
        BinaryData binaryHolder = new BinaryData();
        binaryHolder.binary = binary;
        string json = JsonUtility.ToJson(binaryHolder);
        //Debug.Log(json);
        using (UnityWebRequest request = UnityWebRequest.Post(serverInvAddress + "/" + userId + "/" + InventoryType, json))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if(request.isNetworkError || request.isHttpError)
            {
                Debug.Log("request.error");
            }
            else
            {
                //Debug.Log(request.downloadHandler.text);
            }
        }

    }
    IEnumerator LoadUserInventoryWithStream(string userId, string InventoryType)
    {
        UnityWebRequest www = UnityWebRequest.Get(serverInvAddress + "/" + userId + "/" + InventoryType);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Something went wrong : " + www.error);
            yield return null;
        }
        else
        {
            var x = new List<byte>();
            string node = JSON.Parse(www.downloadHandler.text)[0][0][0];
            string[] split = node.Split(',');
            foreach(string str in split)
            {
                byte number = Convert.ToByte(str);
                x.Add(number);
            }
            //Debug.Log(x +""+ x.Count);

            //Debug.Log(node);
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(x.ToArray());
            //Debug.Log(stream.Length);
            Inventory newContainer = (Inventory)formatter.Deserialize(stream);
            switch (InventoryType)
            {
                case "EQPInventory":
                    for (int i = 0; i < player.inventoryEQP.GetSlots.Length; i++)
                    {
                        player.inventoryEQP.Container.slots[i].UpdateSlot(newContainer.slots[i].item, newContainer.slots[i].amount);
                    }
                    break;
                case "USEInventory":
                    for (int i = 0; i < player.inventoryUSE.GetSlots.Length; i++)
                    {
                        player.inventoryUSE.Container.slots[i].UpdateSlot(newContainer.slots[i].item, newContainer.slots[i].amount);
                    }
                    break;
                case "ETCInventory":
                    for (int i = 0; i < player.inventoryETC.GetSlots.Length; i++)
                    {
                        player.inventoryETC.Container.slots[i].UpdateSlot(newContainer.slots[i].item, newContainer.slots[i].amount);
                    }
                    break;
                case "CurrentEquipment":
                    for (int i = 0; i < player.equipment.GetSlots.Length; i++)
                    {
                        player.equipment.Container.slots[i].UpdateSlot(newContainer.slots[i].item, newContainer.slots[i].amount);
                    }
                    break;
                case "QuickSlot":
                    for (int i = 0; i < player.quickSlot.GetSlots.Length; i++)
                    {
                        player.quickSlot.Container.slots[i].UpdateSlot(newContainer.slots[i].item, newContainer.slots[i].amount);
                    }
                    break;
                default:
                    break;                
        }
            stream.Close();


        }

    }


}

[System.Serializable]
public class BinaryData
{
    public byte[] binary;
}

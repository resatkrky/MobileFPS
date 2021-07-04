using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MobileFPSGameManager : MonoBehaviour
{
    [SerializeField]
    GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady) //Swat karakterini oyun içinde oluşturma
        {
            if(playerPrefab != null)
            {
                int randomPoint = Random.Range(-10, 10);
                PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(randomPoint, 0f, randomPoint), Quaternion.identity);
            }
            else
            {
                Debug.Log("Place Prefab");
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

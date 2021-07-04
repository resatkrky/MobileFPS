using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Header's 

    [Header("Login UI Panel")] // Headerlar Unity'de GameObject'e tıklayınca sağda açılan kısımlara yerleştirmeyi sağlar 
    public InputField playerNameInput;
    public GameObject Login_UI_Panel;

    [Header("Connection Status")]
    public Text connectionStatusText;

    [Header("Game Options UI Panel")]
    public GameObject GameOptions_UI_Panel;

    [Header("Create Room UI Panel")]
    public GameObject CreateRoom_UI_Panel;
    public InputField roomNameInputField;

    public InputField maxPlayerInputField;

    [Header("Inside Room UI Panel")]
    public GameObject InsideRoom_UI_Panel;
    public Text roomInfoText;
    public GameObject playerListPrefab;
    public GameObject playerListContent;
    public GameObject startGameButton;

    [Header("Room List UI Panel")]
    public GameObject RoomList_UI_Panel;
    public GameObject roomListEntryPrefab;
    public GameObject roomListParentGameObject;

    [Header("Join Random Room UI Panel")]
    public GameObject JoinRandomRoom_UI_Panel;

    #endregion

    #region Dictionary
    
    private Dictionary<string, RoomInfo> cachedRoomList;

    private Dictionary<string, GameObject> roomListGameObjects;

    private Dictionary<int, GameObject> playerListGameObjects;

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        ActivePanel(Login_UI_Panel.name); //Başlangıcta Login kısmı açılması için
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
        connectionStatusText.text = "Bağlantı durumu: " + PhotonNetwork.NetworkClientState; //Internete bağlıysak bilgi verir Consale'da
    }

    #endregion

    #region UI Callbacks

    public void OnLoginButtonClicked() //Logine tıklayınca Player Adını alır
    {
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Oyuncu adı gecersiz");
        }
    }

    public void OnCreateRoomButtonClicked() //Create Room butonuna tıklayınca
    {
        string roomName = roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(maxPlayerInputField.text);


        PhotonNetwork.CreateRoom(roomName,roomOptions); //Photon oda kurur
    }

    public void OnCancelButtonClicked()
    {
        ActivePanel(GameOptions_UI_Panel.name);
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivePanel(RoomList_UI_Panel.name);
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivePanel(GameOptions_UI_Panel.name);
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        ActivePanel(JoinRandomRoom_UI_Panel.name);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }

        
    }

    #endregion

    #region Photon Callbacks
   
    public override void OnConnected() //Network'e bağlı mı 
    {
        Debug.Log("Internete baglandı"); // Console'a yazar
    }

    public override void OnConnectedToMaster() //Photon'a bağlı mı 
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Photon'a baglandı"); //Console'a yazar
        ActivePanel(GameOptions_UI_Panel.name); // Photon'a bağlandığında GameOptions panelini açması için 
    }

    public override void OnCreatedRoom() //Oda Oluşturma
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " oluşturuldu.");//Console'a yazar
    }

    public override void OnJoinedRoom() // Odaya girme
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " girildi " + PhotonNetwork.CurrentRoom.Name); //Console'a yazar
        ActivePanel(InsideRoom_UI_Panel.name); //InsideRoom paneli açar

        if (PhotonNetwork.LocalPlayer.IsMasterClient) //Odayı biz mi kurduk
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }

        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " + 
                            "Players/Max.players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                             PhotonNetwork.CurrentRoom.MaxPlayers;

        if(playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach(Player player in PhotonNetwork.PlayerList) //Odadaki oyuncu isimleri
        {
            GameObject playerListGameObject = Instantiate(playerListPrefab);
            playerListGameObject.transform.SetParent(playerListContent.transform);
            playerListGameObject.transform.localScale = Vector3.one;

            //playerListGameObject Child'ına ulaşmak için
            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            if(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
            }
            else
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
            }

            playerListGameObjects.Add(player.ActorNumber, playerListGameObject);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) // Başka birisi oda açınca o odaya bağlanma
    {
        ClearRoomListView();

        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name); // Konsola odanın adını yazar

            if(!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList.Remove(room.Name);
                }
            }
            else
            {
                if (cachedRoomList.ContainsKey(room.Name)) //Oda listesini günceller
                {
                    cachedRoomList[room.Name] = room;
                }

                else // oda listesine yeni oda ekler
                {
                    cachedRoomList.Add(room.Name, room);
                }
            }
        }

        foreach(RoomInfo room in cachedRoomList.Values)
        {
            GameObject roomListEntryGameObject = Instantiate(roomListEntryPrefab); // Odaya katılmadaki kısımların işlevlerini gösteriyo
            roomListEntryGameObject.transform.SetParent(roomListParentGameObject.transform);
            roomListEntryGameObject.transform.localScale = Vector3.one;

            //roomListEntryGameObject Child'ına ulaşmak için
            roomListEntryGameObject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListEntryGameObject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + " / " +room.MaxPlayers;
            roomListEntryGameObject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(()=>OnJoinRoomButtonClicked(room.Name)) ;

            roomListGameObjects.Add(room.Name, roomListEntryGameObject);
        }

    }

    public override void OnLeftLobby()
    {
        ClearRoomListView();
        cachedRoomList.Clear();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) //Odadayken başka oyuncu bağlandığında bilgi verir
    {
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                            "Players/Max.players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                             PhotonNetwork.CurrentRoom.MaxPlayers;

        GameObject playerListGameObject = Instantiate(playerListPrefab);
        playerListGameObject.transform.SetParent(playerListContent.transform);
        playerListGameObject.transform.localScale = Vector3.one;

        //playerListGameObject Child'ına ulaşmak için
        playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;

        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
        }
        else
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
        }

        playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
                            "Players/Max.players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                             PhotonNetwork.CurrentRoom.MaxPlayers;

        Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);

        if (PhotonNetwork.LocalPlayer.IsMasterClient) //Odanın kurucusu oyundan çıkarsa
        {
            startGameButton.SetActive(true);
        }
    }

    public override void OnLeftRoom()
    {
        

        ActivePanel(GameOptions_UI_Panel.name);

        foreach(GameObject playerListGameObject in playerListGameObjects.Values)
        {
            Destroy(playerListGameObject);
        }

        playerListGameObjects.Clear();
        playerListGameObjects = null;
    }

    public override void OnJoinRandomFailed(short returnCode,string message)
    {
        Debug.Log(message);

        string roomName = "Room" + Random.Range(1000, 10000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;

        PhotonNetwork.CreateRoom(roomName, roomOptions);


    }

    #endregion

    #region Private Methods 

    void OnJoinRoomButtonClicked(string _roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(_roomName);
    }

    void ClearRoomListView()
    {
        foreach (var roomListGameObject in roomListGameObjects.Values)
        {
            Destroy(roomListGameObject);
        }

        roomListGameObjects.Clear();
    }

    #endregion

    #region Public Methods ActivePanel (Sırayla açılacak panelleri ayarla)

    public void ActivePanel(string panelToBeActivated)  //Tek bir panel açık hale gelir diğer paneller kapanır( biri açılınca diğeri kapanır)
    {
        Login_UI_Panel.SetActive(panelToBeActivated.Equals(Login_UI_Panel.name)); // panelToBeActivated parametresini uyuyor mu diye bakar ona göre çalıştırır
        GameOptions_UI_Panel.SetActive(panelToBeActivated.Equals(GameOptions_UI_Panel.name)); // panelToBeActivated parametresini uyuyor mu diye bakar ona göre çalıştırır
        CreateRoom_UI_Panel.SetActive(panelToBeActivated.Equals(CreateRoom_UI_Panel.name));
        InsideRoom_UI_Panel.SetActive(panelToBeActivated.Equals(InsideRoom_UI_Panel.name));
        RoomList_UI_Panel.SetActive(panelToBeActivated.Equals(RoomList_UI_Panel.name));
        JoinRandomRoom_UI_Panel.SetActive(panelToBeActivated.Equals(JoinRandomRoom_UI_Panel.name));
        
    }

    #endregion
}

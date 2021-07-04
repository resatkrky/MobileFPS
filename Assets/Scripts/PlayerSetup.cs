using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;


public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public GameObject[] FPS_Hand_ChildGameobject;
    public GameObject[] Soldier_ChildGameobject;

    public GameObject playerUIPrefab;
    private PlayerMovementJoystickController playerMovementController;

    public Camera FPSCamera;

    private Animator animator;

    private Shooting shooter;

    // Start is called before the first frame update
    void Start()
    {
        shooter = GetComponent<Shooting>();
        animator = GetComponent<Animator>();
        playerMovementController = GetComponent<PlayerMovementJoystickController>();

        if (photonView.IsMine) //Bizim karakter ise eller aktif karakter deaktif
        {
            foreach (GameObject gameObject in FPS_Hand_ChildGameobject)
            {
                gameObject.SetActive(true);
            }
            foreach (GameObject gameObject in Soldier_ChildGameobject)
            {
                gameObject.SetActive(false);
            }

            GameObject playerUIGameobject = Instantiate(playerUIPrefab); //Local kullanıcının joystiğinin görünmesi
            playerMovementController.joystick = playerUIGameobject.transform.Find("Fixed Joystick").GetComponent<Joystick>();
            playerMovementController.fixedTouchField = playerUIGameobject.transform.Find("RotationTouchField").GetComponent<FixedTouchField>();

            playerUIGameobject.transform.Find("FireButton").GetComponent<Button>().onClick.AddListener(()=>shooter.Fire());

            FPSCamera.enabled = true;

            animator.SetBool("IsSoldier",false);

        }
        else //Bizim karakter değil düşman ise eller deaktif karakter aktif
        {
            foreach (GameObject gameObject in FPS_Hand_ChildGameobject)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject gameObject in Soldier_ChildGameobject)
            {
                gameObject.SetActive(true);
            }

            playerMovementController.enabled = false;
            GetComponent<RigidbodyFirstPersonController>().enabled = false;
            FPSCamera.enabled = false;

            animator.SetBool("IsSoldier", true);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

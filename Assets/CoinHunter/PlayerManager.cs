using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;
using TMPro;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class PlayerManager : NetworkBehaviour
{
    public float Playerspeed = 10f;
    public GameObject coinCounter;
    public GameObject joinMessage;
    [SyncVar(hook = nameof(HideJoinMessage))]
    private bool hasJoined = false;

    [SyncVar(hook = nameof(HideCoinCounter))]
    private bool alreadyCollected = false;


    [SyncVar(hook = nameof(SetColor))]
    public Color color;

    public SpriteRenderer sr;
    [SyncVar(hook = nameof(OnCoinCountChanged))]//En cuanto se actualiza el valor abajo, la siguiente funcion se ejecuta
    private int coinsCollected = 0;

    public bool busy = false;
  
    #region Unity Callbacks

    /// <summary>
    /// Add your validation code here after the base.OnValidate(); call.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
    }

    // NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.
    void Awake()
    {
    }

    private void Start()
    {
        if (hasJoined) return;//Si no es el jugador local se ignora
        /*if (isServer)//Entra si esta en el server spawneado
        {
            AssignColorRandom();
        }*/
        
        joinMessage.SetActive(true);


        Invoke("CommandHideJoin", 2);

    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX * Playerspeed, moveY * Playerspeed, 0) * Time.deltaTime;
        transform.position += movement;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            if (!isServer) return;
            NetworkServer.Destroy(collision.gameObject);
            coinsCollected++;
        }
    }

    #endregion

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() 
    {
        //AssignColorRandom();
        CommandSetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
        //Invoke("HideJoinMessage", 2);
    }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() { }

    /// <summary>
    /// Called when the local player object is being stopped.
    /// <para>This happens before OnStopClient(), as it may be triggered by an ownership message from the server, or because the player object is being destroyed. This is an appropriate place to deactivate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStopLocalPlayer() {}

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion
    //[ClientRpc]
    private void OnCoinCountChanged(int oldCount, int newCount)
    {
        
        coinCounter.GetComponent<TMP_Text>().text = "Coins: " + coinsCollected;
        //coinCounter.gameObject.SetActive(true);
        if (!busy)
        {
            switchCoin();
        }
        
    }

    [Command]
    private void switchCoin() 
    {
        alreadyCollected = !alreadyCollected; 
    }
    [ClientRpc]
    private void HideCoinCounter(bool oldState, bool newState)
    {
        busy = newState;
        coinCounter.SetActive(newState);
        if(newState == true)
        {
            
            Invoke("switchCoin", 2);
        }
        
    }

    [Command]
    private void CommandHideJoin()
    {

        hasJoined = true;
    }

    [ClientRpc]
    private void HideJoinMessage(bool oldState, bool newState)
    {
        if (joinMessage != null)//Esto es en caso de que se me olvide asignarlo (paso)
        {
            joinMessage.SetActive(false);
        Debug.Log("JoinMessage Deleted");
        }
    }
    [Command]
    private void CommandSetColor(Color newColor)
    {
        color = newColor;
    }

    private void SetColor(Color oldColor, Color newColor)
    {
        sr.color = newColor;
    }
    
}

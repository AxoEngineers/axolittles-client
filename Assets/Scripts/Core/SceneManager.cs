using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Infteract;
using LwNetworking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManager : Mingleton<SceneManager>
{
    public static string Nickname
    {
        get => PlayerPrefs.GetString("SappyNickname", "Seal");
        set => PlayerPrefs.SetString("SappyNickname", value);
    }
    
    // OPTIONS
    
    // CORE
    public SocketClient Client;
    
    // PANELS
    public GameObject LoadingPanel;
    public GameObject AvatarSelectionPanel;
    public GameObject BrowserPanel;
    public GameObject ConnectPanel;
    
    // UI
    public Image _PixelverseBackground;
    public Text _AuthInformationTxt;
    public Button _MetamaskConnectBtn;
    public Text _LoadingText;
    public Text _WalletConnectText;
    
    // temporary variabes
    private bool loginFailed = false;
    private bool pleaseWait = true;
    private bool errorShown = false;
    private SappyIdentity identity = SappyIdentity.Null;
    
    public string Status
    {
        get { return _LoadingText.text; }
        set { _LoadingText.text = value; }
    }
    
    private void Start()
    {
        #if UNITY_EDITOR
        Application.targetFrameRate = 120;
        Debug.LogError("You cannot run this project in editor due to inability to use socket.io in editor."
                       + "\r\nYou must built the WebGL project and host it somewhere.");
        Status = "WebGL Build Required";
        LoadingPanel.gameObject.SetActive(false);
        ConnectPanel.gameObject.SetActive(false);
        _PixelverseBackground.gameObject.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main", LoadSceneMode.Additive);
        #endif

        Application.runInBackground = true;
        
        Client.Listener.AddListener(NetworkEventEnum.AuthFailure, arg0 => { Status = $"Authentication Failed ({(string)arg0})"; });
        Client.Listener.AddListener(NetworkEventEnum.AuthVerify, arg0 => { Status = $"Authentication Pending"; });
        Client.Listener.AddListener(NetworkEventEnum.AuthSuccess, arg0 => { Status = $"Authentication Success"; identity = JsonUtility.FromJson<SappyIdentity>((string)arg0); });
        Client.Listener.AddListener(NetworkEventEnum.Error, arg0 => { ShowErrorMessage((string)arg0); });
        
        MetamaskAuth.Instance.onLoginData.AddListener(wallet =>
        {
            _AuthInformationTxt.text = $"LOGGED INTO {Configuration.GetEnvName()}\r\nETH-ADDRESS: {wallet.eth_address}\r\nAVATARS OWNED: {MetamaskAuth.Instance.Wallet.avatars.Length}";
        });
        
        MetamaskAuth.Instance.onLoginFail.AddListener((code, message) =>
        {
            loginFailed = true;
            _WalletConnectText.text = $"An error with Metamask:\r\n {message} ({code})";
        });
    }

    IEnumerator GoToMainMenu()
    {
        SetLoadingScreen(true);
        yield return new WaitForSeconds(0.5f);
        SetLoadingScreen(false);
        AvatarSelectionPanel.gameObject.SetActive(true);
        _PixelverseBackground.gameObject.SetActive(true);
    }

    public void MetamaskAuthenticate()
    {
        ConnectPanel.SetActive(false);
        SetLoadingScreen(true);
        StartCoroutine(WaitForLogin());
    }

    IEnumerator WaitForLogin()
    {
        _MetamaskConnectBtn.interactable = false;
        _WalletConnectText.text = "";
        
        var auth = MetamaskAuth.Instance;
        Status = "Asking Metamask to authenticate you";
        yield return auth.Authenticate();
        Status = "Waiting for Metamask (check window)";
        while (!auth.Authenticated && !loginFailed)
        {
            yield return new WaitForEndOfFrame();
        }

        if (loginFailed) // Metamask Authentication Failed
        {
            Status = "Metamask Authentication Failed";
            _MetamaskConnectBtn.interactable = true;
            SetLoadingScreen(false);
            ConnectPanel.SetActive(true);
            loginFailed = false;
            yield break;
        }
        else if (MetamaskAuth.Instance.Wallet.avatars.Length < 1) // 1 avatar required at least
        {
            SetLoadingScreen(false);
            _MetamaskConnectBtn.interactable = true;
            Status = $"You must own at least 1 avatar to login.\r\nYou only have {MetamaskAuth.Instance.Wallet.avatars.Length} currently";
            _WalletConnectText.text = Status;
            ConnectPanel.SetActive(true);
            yield break;
        }
        
        Status = "Metamask Authentication Successful";
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(GoToMainMenu());
    }

    public void StartConnectServer(string ip, string room, string gameName)
    {
        StartCoroutine(LoginProtocolCoroutine(ip, room, gameName, reason =>
        {
            SetLoadingScreen(false);
            AvatarSelectionPanel.SetActive(true);
        }));
    }
    
    // the best time to know we're securely connected is by the player id (For now)
    IEnumerator LoginProtocolCoroutine(string address, string room, string gameName, UnityAction<string> failAction=null)
    {
        identity = SappyIdentity.Null;
        SetLoadingScreen(true);
        
        string ip = address;
        Status = "Connecting to\r\n " + gameName;
        yield return new WaitForSeconds(0.25f);
        Client.TryConnect(ip, room, Nickname, MetamaskAuth.Instance.SelectedAvatar.ToString());

        var timeout = Time.time + 5.0f;
        while (Time.time < timeout && !Client.Connected)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (!Client.Connected)
        {
            Status = "Failed to connect to server";
            if (failAction != null)
            {
                yield return new WaitForSeconds(3.0f);
                failAction.Invoke(Status);
            }
            yield break;
        }
        
        Status = "Auth Check Pending..";
        yield return new WaitForSeconds(0.1f);

        timeout = Time.time + 30.0f;
        while (identity.IsNull && Time.time < timeout)
        {
            yield return new WaitForEndOfFrame();
        }
        
        if (!Client.Connected || identity.IsNull)
        {
            // error message hasnt come in
            if (Status.StartsWith("Auth Check Pending"))
            {
                SocketClient.Instance.DisconnectFromServer();
                Status = "Auth Check Failure";
            }
            
            if (failAction != null)
            {
                yield return new WaitForSeconds(3.0f);
                failAction.Invoke(Status);
            }
            yield break;
        }
        
        yield return new WaitForSeconds(0.25f);

        Status = "Entering Game...";
        yield return new WaitForSeconds(0.25f);

        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Main", LoadSceneMode.Additive);

        Debug.Log("Creating new Main Scene instance");
        SetLoadingScreen(false);
    }

    public void ExitGame(IEnumerator beginAction=null)
    {
        SocketClient.Instance.DisconnectFromServer();
        SetLoadingScreen(true);
        StartCoroutine(UnloadOperation(beginAction));
    }

    IEnumerator UnloadOperation(IEnumerator beginAction=null)
    {
        if (beginAction != null)
        {
            yield return beginAction;
        }

        yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Main", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        
        SetLoadingScreen(false);
        AvatarSelectionPanel.SetActive(true);
        _PixelverseBackground.gameObject.SetActive(true);
    }

    public void SetLoadingScreen(bool state)
    {
        pleaseWait = state;
        if (!errorShown)
        {
            Status = "Please Wait...";
        }
        LoadingPanel.gameObject.SetActive(state);
        _PixelverseBackground.gameObject.SetActive(state);
    }

    public void ShowErrorMessage(string msg)
    {
        errorShown = true;
        SetLoadingScreen(true);
        Status = "Error: " + msg;
    }
    
}

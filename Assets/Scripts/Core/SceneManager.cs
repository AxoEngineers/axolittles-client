using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Infteract;
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
    
    // PANELS
    public GameObject LoadingPanel;
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
    private AvatarIdentity identity = AvatarIdentity.Null;
    
    public string Status
    {
        get { return _LoadingText.text; }
        set { _LoadingText.text = value; }
    }
    
    private void Start()
    {
        #if UNITY_EDITOR
        Application.targetFrameRate = 120;
        Debug.LogError("You cannot run this project in editor due to inability to use Metamask in editor."
                       + "\r\nYou must built the WebGL project and host it somewhere.");
        Status = "WebGL Build Required";
        LoadingPanel.gameObject.SetActive(false);
        ConnectPanel.gameObject.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main", LoadSceneMode.Additive);
        #endif

        Application.runInBackground = true;
        
        MetamaskAuth.Instance.onLoginData.AddListener(wallet =>
        {
            _AuthInformationTxt.text = $"LOGGED INTO {Configuration.GetEnvName()}\r\nETH-ADDRESS: {wallet.eth_address}\r\nAXOLITTLES OWNED: {MetamaskAuth.Instance.Wallet.avatars.Length}";
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main", LoadSceneMode.Additive);
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
            Status = $"You must own at least 1 axolitle to login.\r\nYou only have {MetamaskAuth.Instance.Wallet.avatars.Length} currently";
            _WalletConnectText.text = Status;
            ConnectPanel.SetActive(true);
            yield break;
        }
        
        Status = "Metamask Authentication Successful";
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(GoToMainMenu());
    }

    // the best time to know we're securely connected is by the player id (For now)
    IEnumerator LoginProtocolCoroutine(string address, string room, string gameName, UnityAction<string> failAction=null)
    {
        identity = AvatarIdentity.Null;
        SetLoadingScreen(true);
        
        string ip = address;
        Status = "Connecting to\r\n " + gameName;
        yield return new WaitForSeconds(0.25f);

        Status = "Entering Game...";
        yield return new WaitForSeconds(0.25f);

        yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Main", LoadSceneMode.Additive);

        Debug.Log("Creating new Main Scene instance");
        SetLoadingScreen(false);
    }

    public void ExitGame(IEnumerator beginAction=null)
    {
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
        ConnectPanel.SetActive(true);
    }

    public void SetLoadingScreen(bool state)
    {
        pleaseWait = state;
        if (!errorShown)
        {
            Status = "Please Wait...";
        }
        LoadingPanel.gameObject.SetActive(state);
    }

    public void ShowErrorMessage(string msg)
    {
        errorShown = true;
        SetLoadingScreen(true);
        Status = "Error: " + msg;
    }
    
}

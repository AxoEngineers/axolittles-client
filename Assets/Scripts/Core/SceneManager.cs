using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    public Button _ViewEthAddressBtn;
    public Text _LoadingText;
    public Text _WalletConnectText;
    public InputField _EthAddressInputField;
    
    // temporary variabes
    private static bool urlRunOnce = false;
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
        Application.runInBackground = true;

        // SUPPORT DIRECT ADDRESS IN URL (&ADDRESS={val})
        if (!urlRunOnce)
        {
            urlRunOnce = true;
            string url = Configuration.GetURL();
            var parameters = new Uri(url).DecodeQueryParameters();
            if (parameters.ContainsKey("address"))
            {
                ConnectPanel.SetActive(false);
                SetLoadingScreen(true);
                StartCoroutine(WaitForLogin(parameters["address"]));
                return;
            }
        }

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

    public void ViewRoomByEthAddress()
    {
        if (_EthAddressInputField.text == null)
            return;
        ConnectPanel.SetActive(false);
        SetLoadingScreen(true);
        StartCoroutine(WaitForLogin(_EthAddressInputField.text));
    }
    
    public void MetamaskAuthenticate()
    {
        ConnectPanel.SetActive(false);
        SetLoadingScreen(true);
        StartCoroutine(WaitForLogin(null));
    }

    IEnumerator WaitForLogin(string ethAddress)
    {
        _MetamaskConnectBtn.interactable = false;
        _ViewEthAddressBtn.interactable = false;
        _WalletConnectText.text = "";

        var auth = MetamaskAuth.Instance;
        if (ethAddress == null) // no eth address specified
        {
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
                _PixelverseBackground.gameObject.SetActive(true);
                SetLoadingScreen(false);
                ConnectPanel.SetActive(true);
                loginFailed = false;
                yield break;
            }
        }
        else // auth by eth address
        {
            if (ethAddress.Length != 42)
            {
                SetLoadingScreen(false);
                _PixelverseBackground.gameObject.SetActive(true);
                _MetamaskConnectBtn.interactable = true;
                Status = $"Invalid eth address";
                _WalletConnectText.text = Status;
                ConnectPanel.SetActive(true);
                yield break;
            }
            
            // get all axolittles
            UnityWebRequest www = UnityWebRequest.Get($"{Configuration.GetWeb3URL()}all/{ethAddress}");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) {
                auth.CompleteMetamaskAuth($"{ethAddress}||{www.downloadHandler.text}");
            }
        }
        
        if (MetamaskAuth.Instance.Wallet != null && MetamaskAuth.Instance.Wallet.avatars.Length < 1) // 1 avatar required at least
        {
            SetLoadingScreen(false);
            _PixelverseBackground.gameObject.SetActive(true);
            _MetamaskConnectBtn.interactable = true;
            Status =
                $"That address doesn't own any axolittles. Try another.";
            _WalletConnectText.text = Status;
            ConnectPanel.SetActive(true);
            yield break;
        }

        Status = "Data Retrieval Successful";
        yield return new WaitForSeconds(0.5f);
        
        Status = "Loading Asset Bundle...";

        if (MetamaskAuth.Instance.Wallet != null)
        {
            foreach (var avatar in MetamaskAuth.Instance.Wallet.avatars)
            {
                foreach (var asset in AxoModelGenerator.GetAssetsRequired(avatar.id))
                {
                    Status = "Loading " + asset;
                    AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(asset);
                    yield return handle;
                }
            }
        }

        Status = "Download Complete.. Launching";
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
        _PixelverseBackground.gameObject.SetActive(state);
        LoadingPanel.gameObject.SetActive(state);
    }

    public void ShowErrorMessage(string msg)
    {
        errorShown = true;
        SetLoadingScreen(true);
        Status = "Error: " + msg;
    }
    
}

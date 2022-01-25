using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

//using Random = System.Random;

public class SceneManager : Mingleton<SceneManager>
{
    public static string Nickname
    {
        get => PlayerPrefs.GetString("SappyNickname", "Seal");
        set => PlayerPrefs.SetString("SappyNickname", value);
    }
    
    // OPTIONS
    public Camera menuCamera;
    
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
        
        #if UNITY_EDITOR
        _EthAddressInputField.text = "0x1ca6e4643062e67ccd555fb4f64bee603340e0ea";
        #endif
        
        // SUPPORT DIRECT ADDRESS IN URL (&ADDRESS={val})
        if (!urlRunOnce)
        {
            urlRunOnce = true;
            #if !UNITY_EDITOR
            string url = Configuration.GetURL();
            var parameters = new Uri(url).DecodeQueryParameters();
            if (parameters.ContainsKey("address"))
            {
                ConnectPanel.SetActive(false);
                SetLoadingScreen(true);
                StartCoroutine(WaitForLogin(parameters["address"]));
                return;
            }
            #endif
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
        menuCamera.gameObject.SetActive(false);
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
        StatusTicker.Instance.Randomize();
        
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
                _ViewEthAddressBtn.interactable = true;
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
                _ViewEthAddressBtn.interactable = true;
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
            _ViewEthAddressBtn.interactable = true;
            Status =
                $"That address doesn't own any axolittles. Try another.";
            _WalletConnectText.text = Status;
            ConnectPanel.SetActive(true);
            yield break;
        }

        StatusTicker.Instance.active = true; //"Data Retrieval Successful";
        yield return new WaitForSeconds(0.5f);

        //"Loading Asset Bundle...";

        if (MetamaskAuth.Instance.Wallet != null)
        {
            int preloadInstances = 0;
            foreach (var avatar in MetamaskAuth.Instance.Wallet.avatars)
            {
                if (preloadInstances >= 4) // dont preload more than the first row
                    break;
                
                foreach (var asset in AxoModelGenerator.GetAssetsRequired(avatar.id))
                {
                    if (asset.Value == null)
                        continue;
                    //"Loading " + asset.Value;
                    AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(asset.Value);
                    yield return handle;
                }

                preloadInstances++;
            }
        }

        //"Download Complete.. Launching";
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(GoToMainMenu());
    }

    public void ExitGame(IEnumerator beginAction=null)
    {
        menuCamera.gameObject.SetActive(true);
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
        _MetamaskConnectBtn.interactable = true;
        _ViewEthAddressBtn.interactable = true;
        _PixelverseBackground.gameObject.SetActive(true);
    }

    public void SetLoadingScreen(bool state)
    {
        pleaseWait = state;
        if (!errorShown)
        {
            //"Please Wait...";
        }
        _PixelverseBackground.gameObject.SetActive(state);
        LoadingPanel.gameObject.SetActive(state);
        if (!state) StatusTicker.Instance.active = false;
    }

    public void ShowErrorMessage(string msg)
    {
        StatusTicker.Instance.active = false;
        errorShown = true;
        SetLoadingScreen(true);
        Status = "Error: " + msg;
    }
    
}

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class AvatarGrid : Mingleton<AvatarGrid>
{
    private readonly BetterGridElement[,] _map = new BetterGridElement[5, 1];
    private int _startIndex;
    
    public GameObject template;
    public Transform elementRoot;
    public Button nextPageBtn;
    public Button prevPageBtn;
    public Text pageNumber;
    public InputField searchText;

    public Dictionary<int, Sprite> spriteCache;

    private int IterAmt => _map.GetLength(0) * _map.GetLength(1);
    private bool CanUseNextPage => _startIndex + IterAmt < itemData.Count;
    private bool CanUsePreviousPage => _startIndex - IterAmt >= 0;
    private string SearchValue => searchText.text.ToLower();

    public NftAddress[] ownedAxos => MetamaskAuth.Instance.Wallet.avatars;
    public List<NftAddress> itemData = new List<NftAddress>();

    new void Awake()
    {
        base.Awake();
        
        spriteCache = new Dictionary<int, Sprite>();
        
        if (MetamaskAuth.Instance && MetamaskAuth.Instance.Wallet != null && MetamaskAuth.Instance.Wallet.avatars != null)
        {
            List<NftAddress> avatars = new List<NftAddress>();
            
            /*// For debugging
            for (int i = 0; i < 9999; i++)
            {
                var traits = AxoDatabase.Get(i);
                if (traits.type == "Cosmic")
                {
                    avatars.Add(new NftAddress("0x0", $"{traits.id}"));
                }
            }*/
            
            foreach (var avatar in MetamaskAuth.Instance.Wallet.avatars)
            {
                avatars.Add(avatar);
            }

            itemData = avatars;
            LoadGrid();
        }
        
        /*foreach (var id in _axoWallet)
        {
            AxoModelGenerator.Instance.Create(id, avatar => { LoadAxo(avatar); });
        }*/
    }

    private void LoadGrid()
    {
        for (int i = 0; i < _map.GetLength(0) * _map.GetLength(1); i++)
        {
            CreateElement(i % _map.GetLength(0), i / _map.GetLength(0));
        }
        
        RefreshGrid();
    }
    
    private void CreateElement(int x, int y)
    {
        BetterGridElement newGridElement = Instantiate(template, elementRoot, false).GetComponent<BetterGridElement>();
        newGridElement.name = $"[{x},{y}]";
        _map[x, y] = newGridElement;
    }
    private void RefreshGrid()
    {
        for (int y = 0; y < _map.GetLength(1); y++)
        {
            for (int x = 0; x < _map.GetLength(0); x++)
            {
                int i = _startIndex + ((x % _map.GetLength(0)) + (y * _map.GetLength(0)));
                _map[x, y].SetData(i >= 0 && i < itemData.Count ? itemData[i] : NftAddress.Null);
            }
        }

        prevPageBtn.interactable = CanUsePreviousPage;
        nextPageBtn.interactable = CanUseNextPage;
    }

    public void SetNextPageIndex()
    {
        var iterAmt = _map.GetLength(0) * _map.GetLength(1); 
        if (!CanUseNextPage)
            return;
        _startIndex += iterAmt;

        SetPageNumber();
        RefreshGrid();
    }

    public void SetPreviousPageIndex()
    {
        var iterAmt = _map.GetLength(0) * _map.GetLength(1); 
        if (!CanUsePreviousPage)
            return;
        _startIndex -= iterAmt;
        
        SetPageNumber();
        RefreshGrid();
    }

    private void SetPageNumber()
    {
        pageNumber.text = $"{1 + (_startIndex / IterAmt)}";
    }

    public void OnSearch()
    {
        itemData = GetAll();
        _startIndex = 0;
        SetPageNumber();
        RefreshGrid();
    }

    private List<NftAddress> GetAll()
    {
        var newData = new List<NftAddress>();
        foreach (var avatar in ownedAxos)
        {
            if ($"{avatar.id}".ToLower().Contains(SearchValue))
            {
                newData.Add(avatar);
            }
        }

        return newData;
    }
}

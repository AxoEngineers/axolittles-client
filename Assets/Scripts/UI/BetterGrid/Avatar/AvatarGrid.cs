using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class AvatarGrid : MonoBehaviour
{
    private readonly BetterGridElement[,] _map = new BetterGridElement[5, 1];
    private int _startIndex;
    
    public GameObject template;
    public Transform elementRoot;
    public Button nextPageBtn;
    public Button prevPageBtn;
    public Text pageNumber;
    public InputField searchText;

    private int IterAmt => _map.GetLength(0) * _map.GetLength(1);
    private bool CanUseNextPage => _startIndex + IterAmt < itemData.Count;
    private bool CanUsePreviousPage => _startIndex - IterAmt >= 0;
    private string SearchValue => searchText.text.ToLower();

    private int[] _axoWallet = { 541, 3718, 46, 2740, 5192, 3849, 1937, 3841, 391, 6810 };
    
    public List<AxoInfo> ownedAxos;
    public List<AxoInfo> itemData;

    private void Awake()
    {
        if (MetamaskAuth.Instance && MetamaskAuth.Instance.Wallet != null && MetamaskAuth.Instance.Wallet.avatars != null)
        {
            List<int> avatars = new List<int>();
            foreach (var avatar in MetamaskAuth.Instance.Wallet.avatars)
            {
                avatars.Add(avatar.id);
            }

            _axoWallet = avatars.ToArray();
        }
        
        foreach (var id in _axoWallet)
        {
            AxoModelGenerator.Instance.Create(id, avatar => { LoadAxo(avatar); });
        }
    }

    private void LoadAxo(AxoInfo info)
    {
        ownedAxos.Add(info);
        if (ownedAxos.Count == _axoWallet.Length) LoadGrid();
    }

    private void LoadGrid()
    {
        itemData = ownedAxos;
        
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
                _map[x, y].SetData(i >= 0 && i < itemData.Count ? itemData[i] : null);
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

    private List<AxoInfo> GetAll()
    {
        var newData = new List<AxoInfo>();
        foreach (var avatar in ownedAxos)
        {
            if (avatar.name.ToLower().Contains(SearchValue))
            {
                newData.Add(avatar);
            }
        }

        return newData;
    }
}

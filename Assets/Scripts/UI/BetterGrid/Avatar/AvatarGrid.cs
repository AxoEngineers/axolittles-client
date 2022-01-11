using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class AvatarGrid : MonoBehaviour
{
    public List<AxoInfo> ownedAxos;
    public List<AxoInfo> itemData;

    readonly BetterGridElement[,] _map = new BetterGridElement[5, 1];
    int _startIndex;
    
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
    
    readonly int[] _placeholderWallet = { 541, 3718, 46, 2740, 5192, 3849, 1937, 3841, 391, 6810 };

    private void Awake()
    {
        foreach (var id in _placeholderWallet)
        {
            AxoModelGenerator.Instance.Generate(id, avatar => { LoadAxo(avatar); });
        }
    }

    private void LoadAxo(AxoInfo info)
    {
        ownedAxos.Add(info);
        if (ownedAxos.Count == _placeholderWallet.Length) LoadGrid();
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

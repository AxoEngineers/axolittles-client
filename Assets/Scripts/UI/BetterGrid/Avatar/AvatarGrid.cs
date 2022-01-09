using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class AvatarGrid : MonoBehaviour
{
    // INITIALIZERS
    public List<AxoInfo> ItemData;
    
    // CORE
    BetterGridElement[,] Map = new BetterGridElement[5, 1];
    int startIndex = 0;

    // UI ELEMENTS
    public GameObject Template;
    public Transform ElementRoot;
    public Button NextPageBtn;
    public Button PrevPageBtn;
    public Text PageNumber;
    public InputField SearchText;

    private int IterAmt => Map.GetLength(0) * Map.GetLength(1);
    private bool CanUseNextPage => startIndex + IterAmt < ItemData.Count;
    private bool CanUsePreviousPage => startIndex - IterAmt >= 0;
    private string SearchValue => SearchText.text.ToLower();

    private List<AxoInfo> GeneratePlaceholderItems()
    {
        int[] placeholderWallet = {541, 3718, 46, 2740, 5192, 3849, 1937, 3841, 391, 6810 };
        List<AxoInfo> placeholderData = new List<AxoInfo>();

        for (int i = 0; i < placeholderWallet.Length; i++)
        {
            AxoInfo avatar = AxoModelGenerator.Instance.GenerateFromID(placeholderWallet[i]).GetComponent<AxoInfo>();
            placeholderData.Add(avatar);
        }
        
        return placeholderData;
    }

    void Start()
    {
        ItemData = GeneratePlaceholderItems();
        
        for (int i = 0; i < Map.GetLength(0) * Map.GetLength(1); i++)
        {
            CreateElement(i % Map.GetLength(0), i / Map.GetLength(0));
        }
        
        RefreshGrid();
    }
    
    private BetterGridElement CreateElement(int x, int y)
    {
        BetterGridElement newGridElement = Instantiate(Template, ElementRoot, false).GetComponent<BetterGridElement>();
        newGridElement.name = $"[{x},{y}]";
        Map[x, y] = newGridElement;
        
        return newGridElement;
    }
    private void RefreshGrid()
    {
        for (int y = 0; y < Map.GetLength(1); y++)
        {
            for (int x = 0; x < Map.GetLength(0); x++)
            {
                int i = startIndex + ((x % Map.GetLength(0)) + (y * Map.GetLength(0)));
                Map[x, y].SetData(i >= 0 && i < ItemData.Count ? ItemData[i] : null);
            }
        }

        PrevPageBtn.interactable = CanUsePreviousPage;
        NextPageBtn.interactable = CanUseNextPage;
    }

    public void SetNextPageIndex()
    {
        var iterAmt = Map.GetLength(0) * Map.GetLength(1); 
        if (!CanUseNextPage)
            return;
        startIndex += iterAmt;

        SetPageNumber();
        RefreshGrid();
    }

    public void SetPreviousPageIndex()
    {
        var iterAmt = Map.GetLength(0) * Map.GetLength(1); 
        if (!CanUsePreviousPage)
            return;
        startIndex -= iterAmt;
        
        SetPageNumber();
        RefreshGrid();
    }

    public void SetPageNumber()
    {
        PageNumber.text = $"{1 + (startIndex / IterAmt)}";
    }

    public void OnSearch()
    {
        ItemData = GetAll();
        startIndex = 0;
        SetPageNumber();
        RefreshGrid();
    }

    public List<AxoInfo> GetAll()
    {
        var newData = new List<AxoInfo>();
        foreach (var avatar in GeneratePlaceholderItems() /* PLACEHOLDER */)
        {
            if (avatar.name.ToLower().Contains(SearchValue))
            {
                newData.Add(avatar);
            }
        }

        return newData; //.OrderBy(x => x.GetComponent<AvatarInfo>().name).ToList(); <- problematic for numerical names
    }
}

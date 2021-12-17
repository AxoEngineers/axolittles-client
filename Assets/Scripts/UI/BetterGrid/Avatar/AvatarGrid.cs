using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class AvatarGrid : MonoBehaviour
{
    // INITIALIZERS
    public List<AvatarInfo> ItemData;
    
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

    public int IterAmt => Map.GetLength(0) * Map.GetLength(1);
    public bool CanUseNextPage => startIndex + IterAmt < ItemData.Count;
    public bool CanUsePreviousPage => startIndex - IterAmt >= 0;
    public string SearchValue => SearchText.text.ToLower();
    
    // START PLACEHOLDER CODE
    // There is also placeholder code in GetAll()
    public GameObject placeholderAvatar;
    public Sprite placeholderSprite;

    public List<AvatarInfo> GeneratePlaceholderItems()
    {
        List<AvatarInfo> placeholderData = new List<AvatarInfo>();

        for (int i = 0; i <= 500; i++)
        {
            AvatarInfo avatar = Instantiate(placeholderAvatar).GetComponent<AvatarInfo>();
            avatar.name = $"AXO #{i}";
            avatar.sprite = placeholderSprite;
            placeholderData.Add(avatar);
        }
        
        return placeholderData;
    }
    // END PLACEHOLDER CODE
    
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

    public List<AvatarInfo> GetAll()
    {
        var newData = new List<AvatarInfo>();
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

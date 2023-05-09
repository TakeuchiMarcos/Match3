using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class Board : MonoBehaviour
{
    private const float tweenDuration = 0.25f;
    public static Board Instance { get; private set; }

    public Row[] rows;

    public Tile[,] Tiles { get; private set; }

    private readonly List<Tile> selection = new List<Tile>();

    private void Awake() => Instance = this;

    public int width => Tiles.GetLength(0);
    public int height => Tiles.GetLength(1);

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.A)) return;
        
    }

    private async void Start()
    {
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = rows[x].tiles[y];
                tile.x = x;
                tile.y = y;
                Tiles[x, y] = tile;
                tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
            }
        }
        foreach (var tile in Tiles)
            tile.InitiateNeighborhood();

        var list = new List<Tile>();
        foreach (var tile in Tiles) list.Add(tile);

        await ChainPop(list);
    }

    bool isSwapping = false;
    public async void Select(Tile tile)
    {
        if (isSwapping) return;
        if(selection.Count == 0)
        {
            selection.Add(tile);
            return;
        }
        if(selection.Count == 1 && selection[0]==tile)
        {
            selection.Clear();
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }
        if (selection.Count == 1 && (selection[0].Right == tile || selection[0].Left == tile || selection[0].Top == tile|| selection[0].Bottom == tile))
            selection.Add(tile);
        else selection[0].button.Select();

        if (selection.Count < 2) return;

        isSwapping = true;

        await Swap(selection[0], selection[1]);
        
        var poppedList = new List<Tile>();
        poppedList.AddRange( await ChainPop(selection));

        if (!poppedList.Any())
        {
            await Swap(selection[1], selection[0]);
            selection.RemoveAt(1);
            selection[0].button.Select();
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            selection.Clear();
        }

        isSwapping = false;

    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, tweenDuration))
            .Join(icon2Transform.DOMove(icon1Transform.position, tweenDuration));

        await sequence.Play()
            .AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Item;

        tile1.Item = tile2.Item;
        tile2.Item = tile1Item;

    }

    public List<Tile> CanPop(Tile tile)
    {
        var result = new List<Tile>();

        result.AddRange(tile.GetConnectedTiles());

        return result;
    }
    private async Task<List<Tile>> ChainPop(List<Tile> list)
    {
        var poppedList = new List<Tile>();
        foreach (var selectedTile in list)
        {
            list = CanPop(selectedTile);
            if (list.Any())
            {
                await Pop(list, selectedTile);
                poppedList.AddRange(list);
            }
        }
        if (!poppedList.Any()) return poppedList;
        poppedList.AddRange(await ChainPop(poppedList));
        return poppedList;
    }
    private async Task Pop(List<Tile> list, Tile selectedTile)
    {
        var deflateSequence = DOTween.Sequence();

        foreach (var t in list)
            deflateSequence.Join(t.icon.transform.DOScale(0.0f, 0.5f));

        await deflateSequence.Play()
            .AsyncWaitForCompletion();

        ScoreContainer.Instance.score += selectedTile.Item.value * list.Count;

        foreach(var t in list)
            t.Item = ItemDatabase.Items[Random.Range(0,ItemDatabase.Items.Length)];

        var inflateSequence = DOTween.Sequence();

        foreach (var t in list)
            inflateSequence.Join(t.icon.transform.DOScale(1.0f, 0.5f));

        await inflateSequence.Play()
            .AsyncWaitForCompletion();
    }
}

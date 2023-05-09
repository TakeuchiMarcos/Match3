using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item _item;
    public Item Item
    {
        get { return _item; }
        set
        {
            if (_item == value) return;
            _item = value;
            icon.sprite = _item.sprite;
        }
    }

    public Image icon;

    public Button button;

    public Tile Left;
    public Tile Right;
    public Tile Top;
    public Tile Bottom;

    private void Start()
    {
        button.onClick.AddListener(()=>Board.Instance.Select(this));
    }
    public void InitiateNeighborhood()
    {
        Left = x > 0 ? Board.Instance.Tiles[x - 1, y] : null;
        Right = x < Board.Instance.width - 1 ? Board.Instance.Tiles[x + 1, y] : null;
        Top = y > 0 ? Board.Instance.Tiles[x, y - 1] : null;
        Bottom = y < Board.Instance.height - 1 ? Board.Instance.Tiles[x, y + 1] : null;
    }
    public List<Tile> GetConnectedTiles()
    {
        var result = new List<Tile>() { this };
        var horizontal = new List<Tile>();
        horizontal.AddRange(this.GetRightConnected());
        horizontal.AddRange(this.GetLeftConnected());
        if (horizontal.Count >= 2) result.AddRange(horizontal);

        var vertical = new List<Tile>();
        vertical.AddRange(this.GetTopConnected());
        vertical.AddRange(this.GetBottomConnected());
        if (vertical.Count >= 2) result.AddRange(vertical);

        if (result.Count <= 1) result.Clear();

        return result;
    }
    public List<Tile> GetRightConnected()
    {
        var result = new List<Tile>();
        if (Right == null) return result;
        if (Right.Item.GetInstanceID() != this.Item.GetInstanceID()) return result;
        result.Add(Right);
        result.AddRange(Right.GetRightConnected());
        return result;
    }
    public List<Tile> GetLeftConnected()
    {
        var result = new List<Tile>();
        if (Left == null) return result;
        if (Left.Item.GetInstanceID() != this.Item.GetInstanceID()) return result;
        result.Add(Left);
        result.AddRange(Left.GetLeftConnected());
        return result;
    }
    public List<Tile> GetTopConnected()
    {
        var result = new List<Tile>();
        if (Top == null) return result;
        if (Top.Item.GetInstanceID() != this.Item.GetInstanceID()) return result;
        result.Add(Top);
        result.AddRange(Top.GetTopConnected());
        return result;
    }
    public List<Tile> GetBottomConnected()
    {
        var result = new List<Tile>();
        if (Bottom == null) return result;
        if (Bottom.Item.GetInstanceID() != this.Item.GetInstanceID()) return result;
        result.Add(Bottom);
        result.AddRange(Bottom.GetBottomConnected());
        return result;
    }
}

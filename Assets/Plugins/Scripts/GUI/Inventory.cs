using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Inventory : NetworkBehaviour
{
    public GameObject CurrentSelected;

    public GameObject CurrentWeapon;

    public int horiSlots = 6;

    public Game.inventorySlot[] Inv;

    [HideInInspector]
    public bool InventoryVisible = false;

    public List<ItemDatabase.item> Item;

    [HideInInspector]
    public bool ShowEverything = false;

    public int TileWidth = 25;
    public int vertSlots = 4;
    private Animator anim;
    private int CurHoldingSlot;
    private ItemDatabase.item CurItem;
    private int CurrentHeld;
    private Game.inventorySlot CurrentHolding;
    private int HoldHash = Animator.StringToHash("Holdtype");
    private bool Holding = false;
    private int HotbarSlots = 6;
    private string Select;
    private int Selected = -1;

    public bool AddToInv(int ID, int Amount, int Slot)
    {
        if (Slot != -1)
        {
            if (Inv[Slot].ID == ID)
            {
                Inv[Slot].Amount += Amount;
                return true;
            }
            else if (Inv[Slot].ID == -1)
            {
                Inv[Slot] = new Game.inventorySlot();
                Inv[Slot].ID = ID;
                Inv[Slot].Amount = Amount;
                return true;
            }
            else
            {
                Debug.Log("The slot wasn't valid, dropping...");
                Drop(ID, Amount);
                return false;
            }
        }
        else
        {
            for (var i = 0; i < Inv.Length; i++)
            {
                if (Inv[i] == null)
                {
                    Inv[i] = new Game.inventorySlot();
                }
                if (Inv[i].ID == ID)
                {
                    Inv[i].Amount += Amount;
                    return true;
                }
                else if (Inv[i].ID == -1)
                {
                    Inv[i].ID = ID;
                    Inv[i].Amount = Amount;
                    return true;
                }
            }
            Drop(ID, Amount);
            return false;
        }
    }

    public void Drop(int ID, int Amount)
    {
    }

    public Game.inventorySlot Find(string ItemName)
    {
        foreach (Game.inventorySlot item in Inv)
        {
            if (item.ID == GetItem(ItemName).ID)
            {
                return item;
            }
        }
        return null;
    }

    public ItemDatabase.item GetItem(int ID)
    {
        if (ID < Item.Count && ID >= 0)
        {
            return Item[ID];
        }
        else
        {
            return null;
        }
    }

    public ItemDatabase.item GetItem(string Name)
    {
        for (int i = 0; i < Item.Count; i++)
        {
            if (Item[i].Name == Name)
            {
                return Item[i];
            }
        }
        return null;
    }

    public bool Remove(int Slot, int Amount)
    {
        if (Inv[Slot] != null)
        {
            if (Amount == -1)
            {
                Inv[Slot] = new Game.inventorySlot();
            }
            if (Inv[Slot].Amount > Amount)
            {
                Inv[Slot].Amount -= Amount;
                return true;
            }
            else if (Inv[Slot].Amount == Amount)
            {
                Inv[Slot] = new Game.inventorySlot();
                return true;
            }
        }
        return false;
    }

    private void OnGUI()
    {
        GUI.depth = 0;
        if (Game.player != null && Inv != null && ShowEverything)
        {
            GUI.skin = Game.GUISKIN;
            var cur = GUI.skin.label.alignment;
            Vector2 offset = Vector2.zero;
            offset.x = (Screen.width / 2) - ((HotbarSlots * (TileWidth + 1)) / 2);
            float h = Game.player.GetComponent<StoreVars>().Health;
            float w = Game.player.GetComponent<StoreVars>().Water;
            float f = Game.player.GetComponent<StoreVars>().Food;

            int max = HotbarSlots * (TileWidth + 1);

            Rect HLabel = new Rect(offset.x, Screen.height - (TileWidth + 26), (max - 1) * (h / 100.0f), 25);
            Rect WLabel = new Rect((offset.x - 2) - ((offset.x - 2) * (w / 100.0f)), Screen.height - 25, (offset.x - 2) * (w / 100.0f), 25);
            Rect FLabel = new Rect(Screen.width - offset.x, Screen.height - 25, (offset.x - 2) * (f / 100.0f), 25);

            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = 12;
            GUI.backgroundColor = Game.GUIColor;

            //Health
            GUI.backgroundColor = Game.SetColor(200, 0, 0, 0.5f);
            GUI.Label(HLabel, "");
            GUI.backgroundColor = new Color(0, 0, 0, 0);
            GUI.Label(new Rect(offset.x, HLabel.y, max, 25), "Health:" + h + "%");

            //Water
            GUI.backgroundColor = Game.SetColor(64, 164, 223, 0.5f);
            GUI.Label(WLabel, "");
            GUI.backgroundColor = new Color(0, 0, 0, 0);
            GUI.Label(new Rect(0, WLabel.y, offset.x - 2, 25), "Water:" + w + "%");

            //Food
            GUI.backgroundColor = Game.SetColor(148, 176, 1, 0.5f);
            GUI.Label(FLabel, "");
            GUI.backgroundColor = new Color(0, 0, 0, 0);
            GUI.Label(new Rect(Screen.width - offset.x, FLabel.y, offset.x - 2, 25), "Food:" + f + "%");

            GUI.skin.label.fontSize = 14;
            GUI.backgroundColor = Game.GUIColor;
            for (int i = 0; i < HotbarSlots; i++)
            {
                int xPosh = Mathf.RoundToInt((i * (TileWidth + 1)) + offset.x);
                Rect rec = new Rect(xPosh, Screen.height - TileWidth, TileWidth, TileWidth);
                if (i < Inv.Length)
                {
                    if (Inv[i] == null)
                    {
                        Inv[i] = new Game.inventorySlot();
                    }
                    Inv[i].ID = Mathf.Clamp(Inv[i].ID, -1, Item.Count - 1);
                    Color Current = GUI.backgroundColor;
                    if (i == Selected)
                    {
                        GUI.backgroundColor = Game.Color(128, 128, 128, 0.75f);
                    }
                    Inv[i].Slot = i;
                    if (Inv[i].ID != -1 && Inv[i].ID < Item.Count)
                    {
                        if (GUI.Button(rec, GetItem(Inv[i].ID).texture))
                        {
                            if (!Holding)
                            {
                                CurrentHolding = Inv[i];
                                CurHoldingSlot = i;
                                Holding = true;
                                Remove(i, -1);
                            }
                            else
                            {
                                if (Inv[i].ID == CurrentHolding.ID)
                                {
                                    Inv[i].Amount += CurrentHolding.Amount;
                                    CurrentHolding = new Game.inventorySlot();
                                    CurHoldingSlot = -1;
                                    Holding = false;
                                }
                                else
                                {
                                    var temp1 = CurrentHolding;
                                    CurrentHolding = Inv[i];
                                    CurHoldingSlot = i;
                                    Inv[i] = temp1;
                                }
                            }
                        }
                        GUI.backgroundColor = Game.Color(0, 0, 0, 0);
                        GUI.skin.label.alignment = TextAnchor.LowerRight;
                        GUI.Label(rec, Inv[i].Amount.ToString());
                    }
                    else
                    {
                        if (Holding)
                        {
                            if (GUI.Button(rec, ""))
                            {
                                Inv[i] = CurrentHolding;
                                CurrentHolding = new Game.inventorySlot();
                                Holding = false;
                            }
                        }
                        else
                        {
                            GUI.Button(rec, "");
                        }
                    }
                    GUI.backgroundColor = Game.Color(0, 0, 0, 0);
                    GUI.skin.label.alignment = TextAnchor.LowerLeft;
                    GUI.Label(rec, (i + 1).ToString());
                    GUI.backgroundColor = Current;
                }
            }
            if (InventoryVisible)
            {
                GUI.backgroundColor = Game.GUIColor;
                offset.x = (Screen.width / 2);// - ((horiSlots * (TileWidth + 1)) / 2);
                offset.y = (Screen.height / 2) - ((vertSlots * (TileWidth + 1) - TileWidth) / 2);
                for (int x = 0; x < horiSlots; x++)
                {
                    for (int y = 0; y < vertSlots; y++)
                    {
                        int xPos = Mathf.RoundToInt((x * (TileWidth + 1)) + offset.x);
                        int yPos = Mathf.RoundToInt((y * (TileWidth + 1)) + offset.y);
                        Rect rect = new Rect(xPos, yPos, TileWidth, TileWidth);
                        int CurSlot = HotbarSlots + ((y * (horiSlots)) + x);
                        if (CurSlot < Inv.Length)
                        {
                            if (Inv[CurSlot] != null)
                            {
                                Inv[CurSlot].Slot = CurSlot;
                            }
                            if (Inv[CurSlot] != null && Inv[CurSlot].ID != -1 && GetItem(Inv[CurSlot].ID) != null)
                            {
                                if (GUI.Button(rect, GetItem(Inv[CurSlot].ID).texture))
                                {
                                    if (!Holding)
                                    {
                                        CurrentHolding = Inv[CurSlot];
                                        CurHoldingSlot = CurSlot;
                                        Holding = true;
                                        Remove(CurSlot, -1);
                                    }
                                    else
                                    {
                                        if (Inv[CurSlot].ID == CurrentHolding.ID)
                                        {
                                            Inv[CurSlot].Amount += CurrentHolding.Amount;
                                            CurrentHolding = new Game.inventorySlot();
                                            CurHoldingSlot = -1;
                                            Holding = false;
                                        }
                                        else
                                        {
                                            //Create a temporary instance of the current held object.
                                            var temp = CurrentHolding;
                                            //Make the item that we are holding into the item that was on the slot before.
                                            CurrentHolding = Inv[CurSlot];
                                            //Same for holdingSlot.
                                            CurHoldingSlot = CurSlot;
                                            //Make the item before into the item from the current slot
                                            Inv[CurSlot] = temp;
                                        }
                                    }
                                    return;
                                }
                                GUI.backgroundColor = Game.Color(0, 0, 0, 0);
                                GUI.skin.label.alignment = TextAnchor.LowerRight;
                                GUI.Label(rect, Inv[CurSlot].Amount.ToString());
                                GUI.backgroundColor = Game.GUIColor;
                            }
                            else
                            {
                                if (GUI.Button(rect, ""))
                                {
                                    if (Holding)
                                    {
                                        Inv[CurHoldingSlot] = new Game.inventorySlot();
                                        Inv[CurSlot] = CurrentHolding;
                                        CurrentHolding = new Game.inventorySlot();
                                        Holding = false;
                                    }
                                }
                            }
                        }
                    }
                    Vector2 pos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                    for (int x1 = 0; x1 < horiSlots; x1++)
                    {
                        for (int y1 = 0; y1 < vertSlots; y1++)
                        {
                            int xPos1 = Mathf.RoundToInt((x1 * (TileWidth + 1)) + offset.x);
                            int yPos1 = Mathf.RoundToInt((y1 * (TileWidth + 1)) + offset.y);
                            Rect rect1 = new Rect(xPos1, yPos1, TileWidth, TileWidth);
                            int CurSlot1 = (y1 * horiSlots) + x1;
                            if (Inv[CurSlot1] != null && Inv[CurSlot1].ID != -1)
                            {
                                if (rect1.Contains(pos))
                                {
                                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                                    string Somestring = GetItem(Inv[CurSlot1].ID).Name;
                                    Vector2 size = GUI.skin.label.CalcSize(new GUIContent(Somestring));
                                    GUI.Label(new Rect(pos.x, pos.y, size.x, 25), Somestring);
                                }
                            }
                        }
                    }
                }
                if (Holding)
                {
                    Rect re = new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, TileWidth, TileWidth);

                    GUI.Label(re, GetItem(CurrentHolding.ID).texture);

                    GUI.backgroundColor = Game.Color(0, 0, 0, 0);
                    GUI.skin.label.alignment = TextAnchor.LowerLeft;
                    GUI.Label(re, CurrentHolding.Amount.ToString());
                    GUI.backgroundColor = Game.GUIColor;
                }
            }
            else
            {
                GUI.Label(new Rect(Screen.width / 2 - 1.5f, Screen.height / 2 - 1.5f, 3, 3), "");
            }
            GUI.skin.label.alignment = cur;
        }
    }

    private void OnSpawn()
    {
        AddToInv(6, 1, -1);
    }

    private void Start()
    {
        if (Inv == null || Inv.Length == 0)
        {
            Inv = new Game.inventorySlot[(vertSlots * horiSlots) + HotbarSlots];
            for (var i = 0; i < Inv.Length; i++)
            {
                if (Inv[i] == null)
                {
                    Inv[i] = new Game.inventorySlot();
                }
            }
        }
        Game.SetMouse(false);
        anim = transform.Find("Hands").GetComponent<Animator>();
        Item = Game.gameobject.GetComponent<ItemDatabase>().Item;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            InventoryVisible = !InventoryVisible;
            if (InventoryVisible == false && Holding)
            {
                AddToInv(CurrentHolding.ID, CurrentHolding.Amount, CurrentHolding.Slot);
                Holding = false;
            }
            Game.SetMouse(InventoryVisible);
        }
        if (Input.inputString.Length > 0)
        {
            Select = Input.inputString;
            for (var o = 1; o < 7; o++)
            {
                if (o.ToString() == Select)
                {
                    Selected = o - 1;
                    break;
                }
            }
        }
        if (Inv != null)
        {
            GetComponent<StoreVars>().Inventory = Inv;
            for (var i = 0; i < Inv.Length; i++)
            {
                if (Inv[i] != null)
                {
                    if (Inv[i].Amount <= 0 && Inv[i].ID != -1)
                    {
                        Inv[i].Amount = 0;
                    }
                    else if (Inv[i].ID == -1 && Inv[i].Amount != 0)
                    {
                        Inv[i].Amount = 0;
                    }
                }
            }
            if (Selected != CurrentHeld || CurrentWeapon != CurrentSelected)
            {
                CurrentHeld = Selected;
                if (CurrentHeld != -1 && CurrentHeld < Inv.Length && Inv[CurrentHeld] != null)
                {
                    int ID = Inv[CurrentHeld].ID;
                    if (ID != -1)
                    {
                        CurItem = GetItem(ID);
                        if (CurItem.Type == Game.ItemType.Weapon)
                        {
                            if (CurrentWeapon)
                            {
                                Destroy(CurrentWeapon);
                            }
                            if (CurItem.WeaponType == Game.wt.Nothing)
                            {
                                Debug.LogError(CurItem.Name + "'s weapon type isn't set");
                            }
                            anim.SetInteger(HoldHash, (int)CurItem.WeaponType);
                            //If the item is a weapon do this:
                            GameObject bone = GameObject.FindGameObjectWithTag("Hand");
                            CurrentWeapon = (GameObject)Instantiate(CurItem.Gameobject, Vector3.zero, Quaternion.identity);
                            CurrentWeapon.transform.parent = bone.transform;
                            CurrentWeapon.transform.localPosition = Vector3.zero;
                            CurrentWeapon.transform.localRotation = Quaternion.identity;
                            CurrentWeapon.transform.localScale = Vector3.one;

                            CurrentSelected = CurrentWeapon;
                        }
                    }
                }
                else if (CurrentWeapon != null)
                {
                    GameObject.Destroy(CurrentWeapon);
                }
            }
            if (CurrentHeld == Selected && CurrentSelected != null)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    CurItem.Use();
                }
            }
        }
    }
}
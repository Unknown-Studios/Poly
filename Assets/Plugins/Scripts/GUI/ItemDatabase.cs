using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDatabase : MonoBehaviour
{
    #region Fields

    public List<item> Item = new List<item>();

    #endregion Fields

    #region Enums

    public enum AmmoType { Ammo556 = 0, Ammo762 = 1, Ammo9mm = 2 }

    public enum ConsumableType { Heal = 0, Food = 1, Drink = 2 }

    #endregion Enums

    #region Classes

    [Serializable]
    public class item
    {
        #region Fields

        public AmmoType Ammotype = AmmoType.Ammo556;
        public int ConsumableAmount;
        public ConsumableType ConsumType = ConsumableType.Heal;
        public int Damage = 0;
        public GameObject Gameobject;
        public int ID;
        public string Name = "New item";
        public Vector3 offset;
        public int Range = 0;
        public Vector3 rotation;
        public Vector3 scale;
        public Texture2D texture;
        public Game.ItemType Type = Game.ItemType.Resource;
        public Game.wt WeaponType = Game.wt.Nothing;
        private float cd;

        #endregion Fields

        #region Methods

        public void Use()
        {
            if (cd + 1 > Time.realtimeSinceStartup)
            {
                return;
            }
            cd = Time.realtimeSinceStartup;
            GameObject Holder = Game.player;
            Inventory Inven = Holder.GetComponent<Inventory>();
            if (Type == Game.ItemType.Weapon)
            {
                if (WeaponType == Game.wt.Melee)
                {
                    RaycastHit hit213 = new RaycastHit();
                    Vector3 Position = Holder.transform.position;
                    Position.y += 1.5f;
                    if (Physics.Raycast(Position, Holder.transform.forward, out hit213, Range))
                    {
                        if (hit213.transform.tag == "Player")
                        {
                            hit213.transform.gameObject.GetComponent<StoreVars>().Health -= Damage;
                        }
                        else if (hit213.transform.tag == "AI")
                        {
                            hit213.transform.gameObject.GetComponent<AIData>().Health -= Damage;
                        }
                        else
                        {
                            if (hit213.transform.tag == "Stone")
                            {
                                if (Name == "Pickaxe")
                                {
                                    if (hit213.transform.localScale.x > 0.2f)
                                    {
                                        hit213.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                                        Inven.AddToInv(5, Random.Range(1, 7), -1);
                                    }
                                    else
                                    {
                                        Game.Notice("The stone isn't big to give any useable stones.");
                                    }
                                }
                                else
                                {
                                    Game.Notice("You aren't using the right tool, try with a pickaxe", 2);
                                }
                            }
                            else if (hit213.transform.tag == "Tree")
                            {
                                if (Name == "Axe")
                                {
                                    if (hit213.transform.localScale.x > 0.1f)
                                    {
                                        hit213.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
                                        Inven.AddToInv(0, Random.Range(1, 10), -1);
                                    }
                                    else
                                    {
                                        Game.Notice("The tree isn't big enough to be chopped down.");
                                    }
                                }
                                else
                                {
                                    Game.Notice("You aren't using the right tool, try with an axe", 2);
                                }
                            }
                        }
                    }
                }
                else
                {
                    int Slot = -1;
                    if (Ammotype == AmmoType.Ammo556)
                    {
                        if (Holder.GetComponent<Inventory>().Find("5.56 Ammo") != null)
                        {
                            Slot = Holder.GetComponent<Inventory>().Find("5.56 Ammo").Slot;
                        }
                    }
                    else if (Ammotype == AmmoType.Ammo762)
                    {
                        if (Holder.GetComponent<Inventory>().Find("7.62 Ammo") != null)
                        {
                            Slot = Holder.GetComponent<Inventory>().Find("7.62 Ammo").Slot;
                        }
                    }
                    else if (Ammotype == AmmoType.Ammo9mm)
                    {
                        if (Holder.GetComponent<Inventory>().Find("9mm Ammo") != null)
                        {
                            Slot = Holder.GetComponent<Inventory>().Find("9mm Ammo").Slot;
                        }
                    }
                    if (Slot != -1)
                    {
                        //Item found do your thang
                        if (Holder.GetComponent<Inventory>().Remove(Slot, 1))
                        {
                            RaycastHit hit21 = new RaycastHit();
                            if (Physics.Raycast(Holder.transform.position, Vector3.forward, out hit21, Range))
                            {
                                if (hit21.transform.tag == "Player")
                                {
                                    hit21.transform.gameObject.GetComponent<StoreVars>().Health -= Damage;
                                }
                                else if (hit21.transform.tag == "AI")
                                {
                                    hit21.transform.gameObject.GetComponent<AIData>().Health -= Damage;
                                }
                            }
                            return;
                        }
                    }
                    Game.Notice("You don't have enough ammo for this gun!");
                }
            }
            else if (Type == Game.ItemType.Consumable)
            {
                if (Inven.Remove(Inven.Find(Name).Slot, 1))
                {
                    if (ConsumType == ConsumableType.Heal)
                    {
                        Holder.GetComponent<StoreVars>().Health += ConsumableAmount;
                        return;
                    }
                    else if (ConsumType == ConsumableType.Food)
                    {
                        Holder.GetComponent<StoreVars>().Food += ConsumableAmount;
                        return;
                    }
                    else if (ConsumType == ConsumableType.Drink)
                    {
                        Holder.GetComponent<StoreVars>().Water += ConsumableAmount;
                    }
                }
            }
            else if (Type == Game.ItemType.Building)
            {
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(Holder.transform.position, Vector3.forward, out hit, 10))
                {
                    float cosAngle = Vector3.Dot(Vector3.up, hit.normal);
                    if (cosAngle <= Mathf.Cos(15) && hit.transform.tag == "Chunk")
                    {
                        GameObject.Instantiate(Gameobject, hit.transform.position, Holder.transform.rotation);
                    }
                }
            }
        }

        #endregion Methods
    }

    #endregion Classes
}
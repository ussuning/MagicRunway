using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EquipableSlots : MonoBehaviour {

    private Dictionary<EquipableSlot, GameObject> _slots;
    public Dictionary<EquipableSlot, GameObject> slots
    {
        get
        {
            // Return a copy to prevent mutation.
            return new Dictionary<EquipableSlot, GameObject>(_slots);
        }
    }

    public delegate void CollisionEventHandler(Collision collision);
    public event CollisionEventHandler OnCollisionEnterEvt;

    public delegate void OnEquipEventHandler(EquipableSlot slot, GameObject equipment);
    public OnEquipEventHandler onEquip;
    public OnEquipEventHandler onUnequip;

    public void Equip(EquipableSlot slot, GameObject equipment)
    {
        _slots[slot] = equipment;
        onEquip?.Invoke(slot, equipment);
    }

    public GameObject UnEquip(EquipableSlot slot)
    {
        GameObject lastEquipped = _slots[slot]?.gameObject;
        _slots[slot] = null;
        
        onUnequip?.Invoke(slot, lastEquipped);

        return lastEquipped;
    }

    void Awake()
    {
        _slots = new Dictionary<EquipableSlot, GameObject>();
        // Initialize all slots with null value;
        foreach (EquipableSlot slot in EquipableSlotIterator.allSlots)
            Equip(slot, null);
    }

}

public enum EquipableSlot
{
    body,
    top,
    bottom,
    shoes,
    gloves,
    hat
}

public class EquipableSlotIterator
{
    static List<EquipableSlot> _allSlots = null;
    public static EquipableSlot [] allSlots
    {
        get
        {
            if (_allSlots == null)
            {
                _allSlots = new List<EquipableSlot>();
                string[] slotNames = System.Enum.GetNames(typeof(EquipableSlot));
                foreach (string slotName in slotNames)
                {
                    EquipableSlot slot = (EquipableSlot)System.Enum.Parse(typeof(EquipableSlot), slotName);
                    _allSlots.Add(slot);
                }
            }
            return _allSlots.ToArray();
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(EquipableSlots))]
public class EquipableSlotsEditor : Editor { 

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            GUILayout.Label("---Slots---");
            EquipableSlots myScript = (EquipableSlots)target;
            foreach (KeyValuePair<EquipableSlot, GameObject> kvp in myScript.slots)
            {
                GUILayout.Label(kvp.Key.ToString() + ": " + (kvp.Value != null ? kvp.Value.name : "empty"));
            }
        }
        else
        {
            GUILayout.Label("> Play to see slot contents <");
        }
    }
}
#endif

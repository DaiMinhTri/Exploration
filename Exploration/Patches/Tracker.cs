using System.Collections.Generic;
using System.Linq;
using SkillManager;
using UnityEngine;
using static Minimap;

namespace Exploration.Patches;

public class Tracker : MonoBehaviour
{
    public string m_resourceName = "";
    private float m_trackRange;
    private Sprite? m_pinIcon;
    private PinData? m_pinData;
    private int m_requiredLevel;

    private static readonly List<PinData> m_pinList = new();
    private static readonly object m_pinLock = new();

    public void Create(string resourceName, float trackRange, Sprite pinIcon, int requiredLevel)
    {
        m_resourceName = resourceName;
        m_trackRange = trackRange;
        m_pinIcon = pinIcon;
        m_pinData = null;
        m_requiredLevel = requiredLevel;
    }

    private void Update()
    {
        Player player = Player.m_localPlayer;
        if (!player) return;

        Vector3 position = transform.position;
        position.y = 0f;

        Vector2 delta = new Vector2(player.transform.position.x - position.x, player.transform.position.z - position.z);
        float magnitude = delta.magnitude;
        float skillFactor = player.GetSkillFactor("Exploration");

        bool iconsOnly = ExplorationPlugin.IconsOnly.Value == ExplorationPlugin.Toggle.On;
        bool inRange = magnitude < m_trackRange;

        lock (m_pinLock)
        {
            m_pinList.RemoveAll(pin => pin == null || Vector3.Distance(player.transform.position, pin.m_pos) > m_trackRange);

            while (m_pinList.Count > ExplorationPlugin.MaxPin.Value)
            {
                PinData? first = m_pinList.FirstOrDefault();
                if (first != null)
                {
                    Minimap.instance.RemovePin(first);
                    m_pinList.Remove(first);
                }
                else break;
            }
        }

        if (m_pinData == null && inRange && m_requiredLevel > 0 && skillFactor >= (float)m_requiredLevel / 100f && m_pinList.Count < ExplorationPlugin.MaxPin.Value)
        {
            lock (m_pinLock)
            {
                if (m_pinList.Count < ExplorationPlugin.MaxPin.Value)
                {
                    m_pinData = HaveSimilarPin(position, (PinType)8, m_resourceName, save: false)
                        ?? Minimap.instance.AddPin(position, (PinType)8, iconsOnly ? "" : m_resourceName, false, false, 0L, default);
                    if (m_pinData != null)
                    {
                        m_pinData.m_icon = m_pinIcon;
                        m_pinData.m_worldSize = 0f;
                        m_pinList.Add(m_pinData);
                    }
                }
            }
        }
        else if (m_pinData != null && !inRange)
        {
            RemovePin();
        }
        else if (m_pinData != null && (m_requiredLevel <= 0 || (float)m_requiredLevel / 100f > skillFactor))
        {
            RemovePin();
        }
        else if (m_pinData != null && ExplorationPlugin._hidePins)
        {
            RemovePin();
        }

        if (m_pinData != null) m_pinData.m_pos = position;
    }

    private void RemovePin()
    {
        if (m_pinData == null) return;
        try
        {
            lock (m_pinLock)
            {
                if (m_pinList.Contains(m_pinData)) m_pinList.Remove(m_pinData);
            }
            Minimap.instance.RemovePin(m_pinData);
        }
        catch { }
        finally { m_pinData = null; }
    }

    private static PinData? HaveSimilarPin(Vector3 pos, PinType type, string name, bool save)
    {
        string targetName = name;
        Vector3 flatPos = new Vector3(pos.x, 0f, pos.z);
        return Minimap.instance.m_pins.FirstOrDefault(x => x.m_name == targetName && x.m_type == type && x.m_save == save && CheckDistance(x.m_pos, flatPos, 20f));
    }

    private static bool CheckDistance(Vector3 pinPos, Vector3 refPos, float distance)
    {
        float dx = pinPos.x - refPos.x;
        float dz = pinPos.z - refPos.z;
        return dx * dx + dz * dz < distance * distance;
    }

    private void OnDestroy() => RemovePin();
}

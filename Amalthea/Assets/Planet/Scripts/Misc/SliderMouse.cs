using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderMouse : MonoBehaviour, IPointerEnterHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        Slider s = GetComponent<Slider>();
        LemonSpawn.WorldMC.m_playSpeed = 0;
    }
}

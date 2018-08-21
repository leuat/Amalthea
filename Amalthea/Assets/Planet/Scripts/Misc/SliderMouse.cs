using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderMouse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    bool mouseDown = false;
    bool inside = true;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!mouseDown)
            return;
        Slider s = GetComponent<Slider>();
        LemonSpawn.WorldMC.m_playSpeed = 0;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
//        Slider s = GetComponent<Slider>();
  //      LemonSpawn.WorldMC.m_playSpeed = 0;
    }

    public void OnMouseDown()
    {
 //       mouseDown = true;
   //     Debug.Log("DOWN");
       
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
            mouseDown = true;
        if (Input.GetMouseButtonUp(0))
            mouseDown = false;

    }


}

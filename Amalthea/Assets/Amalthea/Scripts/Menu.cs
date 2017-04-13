using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Amalthea
{


    public class MenuLayout
    {
        public float fontSize;
        public GUIStyle guistyle;
        public Texture2D background;
        public Color colorForeground, colorBackground;

        public enum MenuStyle { SolidColor, SolidFrame }

        public MenuLayout(float fs, GUIStyle glay, Color cf, Color cb, MenuStyle type)
        {
            fontSize = fs;
            guistyle = glay;
            colorBackground = cb;
            colorForeground = cf;
            if (type == MenuStyle.SolidColor)
                background = Util.createSolidTexture(cb);
            if (type == MenuStyle.SolidFrame)
                background = Util.createFrameTexture(cf, cb, 128, 4);
        }



    }

    public class Counter
    {
        public int count;
    }

    public class MenuItem 
    {

        public string text, name;
        public Texture2D image;
        public Vector2 size;
        public bool hasHorizontalChildren;
        public float sizeScale;
        public MenuLayout layout;
        public bool isRoot = false;
        public bool expand = false;
        public float timer = 0;
        public float openTime = 0;
        public float targetTimer = 0;
        public List<MenuItem> children = new List<MenuItem>();
        public MenuItem parent = null;
        public delegate void Callback(System.Object o);
        Callback callback;
        System.Object cbObject;
        public MenuItem(MenuItem par, string n, string txt, Texture2D img, Vector2 siz, bool isHorizontal, float sizeS, MenuLayout lay, Callback cb, System.Object cbO)
        {
            name = n;
            text = txt;
            image = img;
            size = siz;
            parent = par;
            hasHorizontalChildren = isHorizontal;
            sizeScale = sizeS;
            layout = lay;
            isRoot = parent == null;
            callback = cb;
            cbObject = cbO;
        }


        public MenuItem findItem(string n)
        {
            MenuItem found = null;
            foreach (MenuItem mi in children)
            {
                if (mi.name == n)
                    found = mi;
            }

            return found;

        }

        public void replaceItem(string item, int newIndex)
        {
            MenuItem found = findItem(item);
            if (found != null)
            {

                children.Remove(found);
                children.Insert(newIndex, found);
            }

        }

        public void deleteFromChildren(string n)
        {
            MenuItem found = findItem(n);

            if (found != null)
            {
                children.Remove(found);
            }
        }

        public void hasExpandedChildren(Counter c)
        {

            foreach (MenuItem mi in children)
            {
                if (mi.expand)
                    c.count++;
                mi.hasExpandedChildren(c);

            }
                
        }
        public Counter counter = new Counter();

        public void Render(Vector2 pos)
        {
            Vector2 add = new Vector2(0, size.y);

            if (!isRoot)
            {
                //            Rect r = new Rect(pos.x, pos.y, size.x*sizeScale, size.y*sizeScale);
                float scale = 0;// timer * 0.01f * Screen.height;
                Color c = Color.white*0.8f + Color.white*timer*0.2f;
                c.a = 1.0f;
                GUI.color = c;

                float margin = 0;
                if (image == null)
                    margin = size.y / 4;

                Rect r = new Rect(pos.x-1*scale, pos.y-1*scale+margin, size.x+2*scale, size.y+2*scale-2*margin);
                int a = 0;
                Rect clickRect = new Rect(pos.x-0*a, pos.y-0*a+margin, size.x+2*a, size.y+2*a-2*margin);
                if (GUI.Button(r, "", new GUIStyle()))
                {
                    //performSelect = this;
                    if (cbObject != null && callback!=null)
                        callback(cbObject);
                }
//                Debug.Log(timer);
                if (clickRect.Contains(Event.current.mousePosition) && parent.timer == 1)
                {
                    //clearChildren();
                   // if (openTime<10000)
                    openTime = 0.4f;
                    expand = true;
                    if (timer == 0)
                        SolarSystemViewverMain.PlaySound(SSVSettings.audioHoverMenu, 0.3f);
                    targetTimer = 1;
                }
                else
                {
                    counter.count = 0;
                    openTime-=Time.deltaTime;
                    hasExpandedChildren(counter);
                    if (counter.count == 0 && openTime<0)
                    {
                        targetTimer = 0;
                        if (timer == 0)
                            expand = false;
                    }
                }
                timer += Mathf.Sign(targetTimer-timer) * 0.1f;
                timer = Mathf.Clamp(timer, 0, 1);
//                Debug.Log(timer);
                GUI.DrawTexture(r, layout.background);
                if (text == null || text == "")
                {
                    //                    GUI.color = Color.white;
                    c = GUI.color;
                    c.a = 0.9f;
                    GUI.color = c;
                    GUI.DrawTexture(r, image);
                }
                else
                {
                    //                    Debug.Log("LABEL " + text);
                    GUI.color = layout.colorForeground;
                    GUI.Label(r, text, layout.guistyle);
                }

            }
            //else add = Vector2.zero;
            if (hasHorizontalChildren)
                add = new Vector2(size.x, 0);

            Vector2 startPos = pos + add;

            if (hasHorizontalChildren)
                startPos.y += size.y * 0.5f*(1-sizeScale);
            else
                startPos.x += size.x * 0.5f*(1-sizeScale);
                
            add *= sizeScale;
            int i = 0;
            if (isRoot)
            {
                expand = true;
                timer = 1;
            }

            if (expand)
            foreach (MenuItem mi in children)
            {
                mi.Render(startPos + add * i*timer);
                i++;
            }
            
        }
            


    }





}
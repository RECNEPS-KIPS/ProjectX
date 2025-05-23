
using System;
using UnityEngine;
using UnityEngine.UI;

public class StateBarUI : MonoBehaviour,IUI
{
   [SerializeField] private Image RedBloodBar;
   [SerializeField] private Image GreenBloodBar;
   private Transform cam;
   private float baseScale=8;
    private float baseDistance = 3;
    public void Init()
    {
        //GetUIImage();
    }
    private void Awake()
    {
        GetUIImage();
        if (RedBloodBar == null || GreenBloodBar == null)
        {
            Debug.LogError("RedBloodBar or GreenBloodBar is not initialized.");
        }
        cam = Camera.main.transform;
    }

    private void GetUIImage()
    {
       // RedBloodBar = transform.Find("HP Red").GetComponent<Image>();
       // GreenBloodBar = transform.Find("HP Green").GetComponent<Image>();
    }
    private void Update()
    {
        //Debug.Log(GreenBloodBar.fillAmount);
        //Debug.Log(RedBloodBar.fillAmount);
        if (GreenBloodBar.fillAmount < RedBloodBar.fillAmount)
        {
            RedBloodBar.fillAmount -= Time.deltaTime*0.05f;
        }
    }
    public void UpdateBlood(float percentage)
    {
      
        GreenBloodBar.fillAmount= percentage;
       
    }
    public void ShowAt(Vector3 worldPos)
    {

        Vector3 viewPos = Camera.main.WorldToViewportPoint(worldPos);
        if (viewPos.x > -0.2 && viewPos.x < 1.2 && viewPos.y > -0.2 && viewPos.y < 1.2 && viewPos.z > -0.2)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            this.transform.position = screenPos;

            float distance = Vector3.Distance(worldPos, cam.position);
            float scale = baseScale * baseDistance / distance;
            transform.localScale = new Vector3(scale, scale, scale);
        }
        
    }
}

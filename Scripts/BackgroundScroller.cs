using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] float backgroundScrollSpeed = 0.50f;

    Material backgroundMaterial;

    Vector2 offset;


    // Start is called before the first frame update
    void Start()
    {
        this.backgroundMaterial = this.gameObject.GetComponent<MeshRenderer>().material;
        this.offset = new Vector2(0, this.backgroundScrollSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        this.backgroundMaterial.mainTextureOffset += this.offset * Time.deltaTime; 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGrassColor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        int color = UnityEngine.Random.Range(1, 4);
        Shader grassShader;
 
        // Finding shader in Awake()
        grassShader = Shader.Find("Custom/GrassShader");
 
        // Changing  shader
        rend.materials[1].shader = grassShader;
        switch(color) {
            case 1:
                rend.materials[0].SetVector("_BaseColor", new Vector4(0.933333f, 0.19607843f, 0.101960784f, 1f));
                // Set up Grass Color: Yellow
                rend.materials[1].SetVector("_BaseColor", new Vector4(0.933333f, 0.19607843f, 0.101960784f, 1f));
                rend.materials[1].SetVector("_TipColor", new Vector4(1f, 0.980392157f, 0.607843137f, 1f));
                break;
            case 2:
                rend.materials[0].SetVector("_BaseColor", new Vector4(0.188235294f, 0.101960784f, 0.2156862745f, 1f));
                // Set up Grass Color: Violet
                rend.materials[1].SetVector("_BaseColor", new Vector4(0.188235294f, 0.101960784f, 0.2156862745f, 1f));
                rend.materials[1].SetVector("_TipColor", new Vector4(0.8f, 0.501960784f, 0.945098039f, 1f));
                break;
            case 3:
                rend.materials[0].SetVector("_BaseColor", new Vector4(0.211764706f, 0.5843137255f, 0.219607843f, 1f));
                // Set up Grass Color: Green
                rend.materials[1].SetVector("_BaseColor", new Vector4(0.211764706f, 0.5843137255f, 0.219607843f, 1f));
                rend.materials[1].SetVector("_TipColor", new Vector4(0.47843137255f, 0.8980392157f, 0.4862745098f, 1f));
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

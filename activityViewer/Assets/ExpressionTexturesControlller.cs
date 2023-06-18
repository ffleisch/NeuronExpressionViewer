using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InfixParser;

public class ExpressionTexturesControlller : MonoBehaviour
{
    Texture2DArray texArray;

    public void loadAttributes(List<LoadTextures.attributesEnum> attributes)
    {
        LoadTextures[] availableTextures = gameObject.GetComponents<LoadTextures>();
        Dictionary<LoadTextures.attributesEnum, LoadTextures> loadedAttributes = new();
        HashSet<LoadTextures.attributesEnum> wantedAttributes = new();
        foreach (var t in availableTextures)
        {
            loadedAttributes.Add(t.attribute, t);
        }
        foreach (var attr in attributes)
        {
            wantedAttributes.Add(attr);
        }

        foreach (var lt in availableTextures)
        {
            Debug.Log(lt.attribute);
            if (!wantedAttributes.Contains(lt.attribute))
            {
                Debug.Log("removing" + lt.attribute);
                Destroy(lt);
            }
        }

        foreach (var attr in attributes)
        {
            if (!loadedAttributes.ContainsKey(attr))
            {
                var newLoader = gameObject.AddComponent<LoadTextures>();
                newLoader.attribute = attr;
            }
        }
    }

    public void bindAttributesToShader()
    {
        LoadTextures[] availableTextures = gameObject.GetComponents<LoadTextures>();

        var indices = new Dictionary<LoadTextures.attributesEnum, int>();

        List<float> texturesInShader = new();
        List<Texture2D> textures = new();

        foreach (var lt in availableTextures)
        {
            textures.Add(lt.tex);
            texturesInShader.Add((int)lt.attribute);
        }

        var renderer = GetComponent<Renderer>();
        if (availableTextures.Length > 0)
        {
            texArray = new(textures[0].width, textures[0].height, textures.Count, TextureFormat.RGBA32, false);
            texArray.filterMode = FilterMode.Point;
            texArray.wrapMode = TextureWrapMode.Repeat;
            setArrayTexture();
            renderer.sharedMaterial.SetTexture("_attributesArrayTexture", texArray);
            renderer.sharedMaterial.SetInteger("_attributesArrayTextureWidth",textures[0].width);
            renderer.sharedMaterial.SetVector("_attributesArrayTextureTexelSize",textures[0].texelSize);
        }

        float[] attributeIndices = new float[16];
        texturesInShader.CopyTo(attributeIndices, 0);
        renderer.sharedMaterial.SetFloatArray("_attributeIndices", attributeIndices);
    }

    void setArrayTexture()
    {
        LoadTextures[] availableTextures = gameObject.GetComponents<LoadTextures>();

        for (int i = 0; i < availableTextures.Length; i++)
        {
            var lt = availableTextures[i];
            texArray.SetPixels(lt.tex.GetPixels(0), i, 0);//TODO this is bad, copy from gpu to cpu back to gpu
        }
        texArray.Apply();
    }

    void setSteps(int step)
    {
        foreach (var lt in gameObject.GetComponents<LoadTextures>())
        {
            lt.step = step;
        }
        if (texArray)
        {
            setArrayTexture();
        }
    }
}

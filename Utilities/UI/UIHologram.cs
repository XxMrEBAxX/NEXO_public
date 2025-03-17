using System;
using UnityEngine;
using UnityEngine.UI;

namespace BirdCase
{
    public class UIHologram : MonoBehaviour
    {
        [SerializeField]
        private Material hologramSampleMaterial;
        
        private Image hologramImage;
        private SlicedFilledImage hologramSlicedFilledImage;
        private Material hologramMaterial;
        private bool isSlicedFilledImage;

        [HideInInspector]
        public RectTransform rectTransform;

        public Color TextureColor
        {
            get
            {
                return hologramMaterial.GetColor("_Color");
            }
            set
            {
                if (isSlicedFilledImage)
                {
                    hologramSlicedFilledImage.color = value;
                }
                else
                {
                    hologramImage.color = value;
                }
                
                hologramMaterial.SetColor("_Color", value);
            }
        }
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            hologramImage = GetComponent<Image>();
            isSlicedFilledImage = ReferenceEquals(hologramImage, null);
            
            if(isSlicedFilledImage)
            {
                hologramSlicedFilledImage = GetComponent<SlicedFilledImage>();
                hologramMaterial = Instantiate(hologramSampleMaterial);
                hologramMaterial.SetTexture("_Texture", hologramSlicedFilledImage.sprite.texture);
                hologramMaterial.SetColor("_Color", hologramSlicedFilledImage.color);
                hologramSlicedFilledImage.material = hologramMaterial;
            }
            else
            {
                hologramMaterial = Instantiate(hologramSampleMaterial);
                hologramMaterial.SetTexture("_Texture", hologramImage.sprite.texture);
                hologramMaterial.SetColor("_Color", hologramImage.color);
                hologramImage.material = hologramMaterial;
            }
        }
    }
}

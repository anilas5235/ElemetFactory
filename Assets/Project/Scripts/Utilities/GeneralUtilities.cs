using UnityEngine;

namespace Project.Scripts.Utilities
{
    public static class GeneralUtilities
    {
        public static TextMesh CreateWorldText(string text,Vector3 localPosition = default, int fontSize = 40,
            Color color = default, Transform parent = null,  TextAnchor textAnchor = TextAnchor.MiddleCenter,
            TextAlignment textAlignment = TextAlignment.Center, int sortingOrder =0)
        {
            if(color == default) color = Color.white;
            return CreateWorldText(text, parent, localPosition, fontSize, color, textAnchor, textAlignment,
                sortingOrder);
        }

        public static TextMesh CreateWorldText(string text, Transform parent, Vector3 localPosition, int fontSize,
            Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("WorldText",typeof(TextMesh));
            
            Transform transform = gameObject.transform;
            transform.SetParent(parent,false);
            transform.localPosition = localPosition;

            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMesh;
        }

        public static Vector3 GetMousePosition()
        {
            Vector3 pos = GetMousePositionWithZ();
            pos.z = 0;
            return pos;
        }

        public static Vector3 GetMousePositionWithZ()
        {
            return Camera.main != null ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : default;
        }
        
        public static float GausFunction(float x, float a, float b, float c)
        {
            float frontFactor = 1 / a * Mathf.Sqrt(2 * Mathf.PI);
            float exponential = (-1 / 2f) * Mathf.Pow((x - b) / c, 2);
            return frontFactor * Mathf.Exp(exponential);
        }
        
        public static string TurnToHex(int number)
        {
            if (number < 10) return number.ToString();
            return number switch
            {
                10 => "a",
                11 => "b",
                12 => "c",
                13 => "d",
                14 => "e",
                15 => "f",
                _ => "error"
            };
        }
    
    }
}

using System;
using UnityEngine.UIElements;
using UnityEditor;

[CustomEditor(typeof(Samples))]
public class SamplesView : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new();

        SerializedObject so = new SerializedObject(target);
        SliderWithInput sliderWithInput = new SliderWithInput(so.FindProperty("a"), "AAA", -1, 20);
        root.Add(sliderWithInput);
        
        return root;
    }
}

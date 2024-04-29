using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SliderWithInput : VisualElement
{
    #region 用于在UI Builder中显示的样板文件
    public new class UxmlFactory : UxmlFactory<SliderWithInput>{ }
    public SliderWithInput(){ }
    #endregion

    private const string AssetPath = "Assets/UIElement/Editor/Resources/uxml_SliderWithInput.uxml";
    
    private Slider Slider => this.Q<Slider>("slider");
    private FloatField Input => this.Q<FloatField>("input");

    public SliderWithInput(SerializedProperty property, string label = "", float minValue = 0, float maxValue = 10)
    {
        Init(property, label, minValue, maxValue);
    }

    private void Init(SerializedProperty property, string label = "", float minValue = 0, float maxValue = 10)
    {
        VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetPath);
        // 也可使用 Resources.Load() as VisualTreeAsset
        // VisualTreeAsset asset = Resources.Load("uxml_SliderWithInput") as VisualTreeAsset;
        asset.CloneTree(this);

        Slider.lowValue = minValue;
        Slider.highValue = maxValue;
        Slider.label = label;
        Slider.BindProperty(property);
        Input.BindProperty(property);
    }
}

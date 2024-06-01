using BepInEx.Configuration;
using RiskOfOptions.Components.Options;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2.UI;
using System;
using System.ComponentModel;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Local.Option.Generator;

abstract class Option<T>(ConfigEntry<T> entry) : BaseOption, ITypedValueHolder<T>
		where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
{
	public override ConfigEntryBase ConfigEntry => entry;
	readonly T originalValue = entry.Value;

	protected GameObject CreateOptionGameObject<X>(
			GameObject prefab, Transform parent, out X field) where X : Field
	{
		GameObject obj = GameObject.Instantiate(prefab, parent);
		GameObject.DestroyImmediate(obj.GetComponent<ModSetting>());

		field = obj.AddComponent<X>();
		field.name = "Mod " + GetType().Name + " Field, " + Name;

		field.nameToken = GetNameToken();
		field.settingToken = Identifier;

		if ( field is Slider slider )
			slider.slider = obj.GetComponentInChildren<UnityEngine.UI.Slider>();

		field.valueText = obj.GetComponentInChildren<TMP_InputField>();
		field.nameLabel = obj.GetComponent<LanguageTextMeshController>();

		if ( entry.Description.AcceptableValues is AcceptableValueRange<T> range )
		{
			field.min = range.MinValue;
			field.max = range.MaxValue;
		}

		field.formatString ??= "{0}";
		return obj;
	}

	public T Value { get => entry.Value; set => entry.Value = value; }
	public T GetOriginalValue() => originalValue;
	public bool ValueChanged() => ! Value.Equals(GetOriginalValue());

	public override string OptionTypeName { get; set;
			} = typeof(T).Name.ToLowerInvariant() + "_field";

	protected abstract class Field : ModSettingsNumericField<T>
	{
		public override bool TryParse(
				string text, NumberStyles style, IFormatProvider provider, out T result)
		{
			try
			{
				result = (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(text);
				return true;
			}
			catch
			{
				result = default;
				return false;
			}
		}

		public override T Clamp(T value) =>
				value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
	}

	protected abstract class Slider : Field
	{
		internal UnityEngine.UI.Slider slider;

		public override void Awake()
		{
			base.Awake();

			slider.minValue = Convert.ToSingle(min);
			slider.maxValue = Convert.ToSingle(max);

			slider.onValueChanged.AddListener(OnSliderValueChanged);

			valueText.onEndEdit.AddListener(OnTextEdited);
			valueText.onSubmit.AddListener(OnTextEdited);
		}

		public override void Enable() => OnStateChanged(true);

		private void OnStateChanged(bool enabled)
		{
			Transform transform = slider.transform;
			Color color = enabled ? slider.colors.normalColor : slider.colors.disabledColor;

			slider.interactable = enabled;
			transform.Find("Fill Area/Fill").GetComponent<Image>().color = color;

			foreach ( var button in GetComponentsInChildren<HGButton>() )
			{
				button.interactable = enabled;
				color = enabled ? button.colors.normalColor : button.colors.disabledColor;
			}

			transform.parent.Find("TextArea").GetComponent<Image>().color = color;
		}

		public override void Disable() => OnStateChanged(false);

		public void OnSliderValueChanged(float value)
		{
			if ( ! InUpdateControls )
				SubmitValue((T) Convert.ChangeType(value, typeof(T)));
		}

		public override void OnUpdateControls()
		{
			base.OnUpdateControls();
			float value = Convert.ToSingle(Clamp(GetCurrentValue()));

			slider.value = value;
			valueText.text = string.Format(Separator.GetCultureInfo(), formatString, value);
		}

		public void MoveSlider(float delta) => slider.normalizedValue += delta;
	}

	public override BaseOptionConfig GetConfig() => new();
}

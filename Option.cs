using BepInEx.Configuration;
using RiskOfOptions.Components.Options;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2.UI;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Local.Option.Generator;

abstract class Option<T>(ConfigEntry<T> entry) : BaseOption, ITypedValueHolder<T>
		where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
{
	public override ConfigEntryBase ConfigEntry => entry;
	readonly T original = entry.Value;

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
	public T GetOriginalValue() => original;
	public bool ValueChanged() => ! entry.Value.Equals(original);

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

		void OnStateChanged(bool enabled)
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

		protected virtual void OnSliderValueChanged(float value)
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

		protected void MoveSlider(float delta) => slider.normalizedValue += delta;
	}

	public override BaseOptionConfig GetConfig() => configuration;
	readonly BaseOptionConfig configuration = new();
}

class ListOption(ConfigEntryBase entry, object[] list) : BaseOption, ITypedValueHolder<object>
{
	public override ConfigEntryBase ConfigEntry => entry;
	readonly object original = entry.BoxedValue;

	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent)
	{
		GameObject obj = GameObject.Instantiate(prefab, parent);
		GameObject.DestroyImmediate(obj.GetComponent<DropDownController>());

		var controller = obj.AddComponent<Controller>();
		controller.name = "Mod Option " + entry.SettingType.Name + " List, " + Name;

		controller.choices = list;
		controller.index = Array.IndexOf(list, entry.BoxedValue);

		controller.nameLabel = obj.GetComponentInChildren<LanguageTextMeshController>();
		controller.nameLabel.token = controller.nameToken = GetNameToken();

		controller.settingToken = Identifier;
		return obj;
	}

	public object Value { get => entry.BoxedValue; set => entry.BoxedValue = value; }
	public object GetOriginalValue() => original;
	public bool ValueChanged() => ! entry.BoxedValue.Equals(original);

	public override string OptionTypeName { get; set;
			} = entry.SettingType.Name.ToLowerInvariant() + "_list";

	class Controller : ModSettingsControl<object>
	{
		internal object[] choices;
		internal int index;

		public override void Awake()
		{
			dropdown = GetComponentInChildren<RooDropdown>();
			base.Awake();

			dropdown.OnValueChanged.AddListener(OnChoiceChanged);
		}

		RooDropdown dropdown;

		public override void Enable() => OnStateChanged(true);

		void OnStateChanged(bool enabled)
		{
			dropdown.interactable = enabled;
			foreach ( var button in GetComponentsInChildren<HGButton>() )
				button.interactable = enabled;
		}

		public override void Disable() => OnStateChanged(false);

		protected new void OnEnable()
		{
			base.OnEnable();

			dropdown.choices = choices.Select( element => element.ToString() ).ToArray();
			UpdateControls();
		}

		void OnChoiceChanged(int index) => SubmitValue(choices[this.index = index]);

		public override void OnUpdateControls()
		{
			base.OnUpdateControls();
			dropdown.SetChoice(index);
		}
	}

	public override BaseOptionConfig GetConfig() => configuration;
	readonly BaseOptionConfig configuration = new();
}

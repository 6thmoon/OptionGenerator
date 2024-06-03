using BepInEx.Configuration;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace Local.Option.Generator;

class SByteOption(ConfigEntry<sbyte> entry) : Option<sbyte>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Field
	{
		protected Control()
		{
			min = sbyte.MinValue;
			max = sbyte.MaxValue;
		}
	}
}

class SByteSliderOption(ConfigEntry<sbyte> entry) : Option<sbyte>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Slider { }
}

class ByteOption(ConfigEntry<byte> entry) : Option<byte>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Field
	{
		protected Control()
		{
			min = byte.MinValue;
			max = byte.MaxValue;
		}
	}
}

class ByteSliderOption(ConfigEntry<byte> entry) : Option<byte>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Slider { }
}

class Int16Option(ConfigEntry<short> entry) : Option<short>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Field
	{
		protected Control()
		{
			min = short.MinValue;
			max = short.MaxValue;
		}
	}
}

class Int16SliderOption(ConfigEntry<short> entry) : Option<short>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Slider { }
}

class UInt16Option(ConfigEntry<ushort> entry) : Option<ushort>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Field
	{
		protected Control()
		{
			min = ushort.MinValue;
			max = ushort.MaxValue;
		}
	}
}

class UInt16SliderOption(ConfigEntry<ushort> entry) : Option<ushort>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Slider { }
}

class Int32Option(ConfigEntry<int> entry) : IntFieldOption(entry) { }

class Int32SliderOption(ConfigEntry<int> entry) : IntSliderOption(entry, Initialize(entry))
{
	static IntSliderConfig Initialize(ConfigEntry<int> entry)
	{
		var config = new IntSliderConfig() { min = int.MinValue, max = int.MaxValue };

		if ( entry.Description.AcceptableValues is AcceptableValueRange<int> range )
		{
			config.min = range.MinValue;
			config.max = range.MaxValue;
		}

		return config;
	}
}

class UInt32Option(ConfigEntry<uint> entry) : Option<uint>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Field
	{
		protected Control()
		{
			min = uint.MinValue;
			max = uint.MaxValue;
		}
	}
}

class UInt32SliderOption(ConfigEntry<uint> entry) : Option<uint>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Slider { }
}

class Int64Option(ConfigEntry<long> entry) : Option<long>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Field
	{
		protected Control()
		{
			min = long.MinValue;
			max = long.MaxValue;
		}
	}
}

class Int64SliderOption(ConfigEntry<long> entry) : Option<long>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Slider { }
}

class UInt64Option(ConfigEntry<ulong> entry) : Option<ulong>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Field
	{
		protected Control()
		{
			min = ulong.MinValue;
			max = ulong.MaxValue;
		}
	}
}

class UInt64SliderOption(ConfigEntry<ulong> entry) : Option<ulong>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Slider { }
}

class SingleOption(ConfigEntry<float> entry) : FloatFieldOption(entry) { }

class SingleSliderOption(ConfigEntry<float> entry) : SliderOption(entry, Initialize(entry))
{
	public static SliderConfig Initialize(ConfigEntry<float> entry)
	{
		var config = new SliderConfig() { min = float.MinValue, max = float.MaxValue };

		if ( entry.Description.AcceptableValues is AcceptableValueRange<float> range )
		{
			config.min = range.MinValue;
			config.max = range.MaxValue;
		}

		if ( config.min is -50 or 0 or 50 && config.max is 50 or 100 or 150 or 200 )
			config.formatString = "{0:0.0}" + '%';
		else config.formatString = "{0:0.00}";

		return config;
	}
}

class DoubleOption(ConfigEntry<double> entry) : Option<double>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Field
	{
		protected Control()
		{
			formatString = "{0:0.00}";

			min = float.MinValue;
			max = float.MaxValue;
		}
	}
}

class DoubleSliderOption(ConfigEntry<double> entry) : Option<double>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent)
	{
		GameObject obj = CreateOptionGameObject(prefab, parent, out Control slider);

		if ( slider.min is -50 or 0 or 50 && slider.max is 50 or 100 or 150 or 200 )
			slider.formatString = "{0:0.0}" + '%';
		else slider.formatString = "{0:0.00}";

		return obj;
	}

	class Control : Slider { }
}

class DecimalOption(ConfigEntry<decimal> entry) : Option<decimal>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent) =>
			CreateOptionGameObject<Control>(prefab, parent, out _);

	class Control : Field
	{
		protected Control()
		{
			formatString = "{0:0.00}";

			min = decimal.MinValue;
			max = decimal.MaxValue;
		}
	}
}

class DecimalSliderOption(ConfigEntry<decimal> entry) : Option<decimal>(entry)
{
	public override GameObject CreateOptionGameObject(GameObject prefab, Transform parent)
	{
		GameObject obj = CreateOptionGameObject(prefab, parent, out Control slider);

		if ( slider.min is -50 or 0 or 50 && slider.max is 50 or 100 or 150 or 200 )
			slider.formatString = "{0:0.0}" + '%';
		else slider.formatString = "{0:0.00}";

		return obj;
	}

	class Control : Slider { }
}

class BooleanOption(ConfigEntry<bool> entry) : CheckBoxOption(entry) { }

class StringOption(ConfigEntry<string> entry) : StringInputFieldOption(entry, Initialize())
{
	static InputFieldConfig Initialize() => new()
	{
		submitOn = InputFieldConfig.SubmitEnum.OnExitOrSubmit,
		lineType = TMPro.TMP_InputField.LineType.SingleLine,
		richText = false
	};
}

using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using RiskOfOptions.Components.Panel;
using RiskOfOptions.Containers;
using RiskOfOptions.Options;
using RoR2;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Permissions;
using UnityEngine;
using Console = System.Console;
using DependencyFlags = BepInEx.BepInDependency.DependencyFlags;
using Language = RoR2.Language;
using Path = System.IO.Path;
using Settings = RiskOfOptions.ModSettingsManager;

[assembly: AssemblyVersion(Local.Option.Generator.Plugin.version)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Local.Option.Generator;

[BepInPlugin(identifier, "OptionGenerator", version)]
[BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
	public const string version = "0.1.5", identifier = "local.option.generator";
	static ConfigFile configuration; const string section = "Enabled";

	protected void Awake()
	{
		Harmony.CreateAndPatchAll(typeof(Plugin));
		configuration = Config;

		RoR2Application.onLoad += ( ) =>
		{
			foreach ( PluginInfo info in Chainloader.PluginInfos.Values )
				configuration.Bind(section, info.Metadata.GUID, true,
						$"If option menu should be generated for \"{ info.Metadata.Name }\".");
		};
	}

	[HarmonyPatch(typeof(ModOptionPanelController), nameof(ModOptionPanelController.Start))]
	[HarmonyPrefix]
	static void Generate()
	{
		foreach ( PluginInfo info in Chainloader.PluginInfos.Values )
		{
			string identifier = info.Metadata.GUID, name = info.Metadata.Name;

			configuration.TryGetEntry(section, identifier, out ConfigEntry<bool> enabled);
			if ( enabled?.Value is false )
			{
				RemoveOption(identifier);
				continue;
			}

			if ( info.Instance && CheckDependency(info) )
			{
				foreach ( ConfigFile configuration in ScanForConfig(info.Instance) )
					foreach ( ConfigDefinition definition in configuration.Keys )
					{
						ConfigEntryBase entry = configuration[definition];
						if ( HasEntry(identifier, entry) )
							continue;

						BaseOption option = CreateOption(entry);
						if ( option != null )
						{
							Settings.EnsureContainerExists(identifier, name);
							Settings.AddOption(option, identifier, name);
						}
					}
			}

			LoadIcon(info);
			SetDescription(info);
		}
	}

	static bool CheckDependency(PluginInfo info)
	{
		if ( info.Metadata.GUID is identifier )
			return true;

		foreach ( BepInDependency dependency in info.Dependencies )
			if ( dependency.DependencyGUID is RiskOfOptions.PluginInfo.PLUGIN_GUID )
				return false;

		return true;
	}

	static IEnumerable<ConfigFile> ScanForConfig(BaseUnityPlugin instance)
	{
		HashSet<ConfigFile> result = [ instance.Config ];

		Type type = instance.GetType();
		BindingFlags flag = BindingFlags.Instance | BindingFlags.Public |
				 BindingFlags.Static | BindingFlags.NonPublic;

		foreach ( FieldInfo field in type.GetFields(flag) )
			if ( typeof(ConfigEntryBase).IsAssignableFrom(field.FieldType) )
				result.Add(( field.GetValue(instance) as ConfigEntryBase )?.ConfigFile);
			else if ( typeof(ConfigFile).IsAssignableFrom(field.FieldType) )
				result.Add(field.GetValue(instance) as ConfigFile);

		foreach ( PropertyInfo property in type.GetProperties(flag) )
			if ( typeof(ConfigEntryBase).IsAssignableFrom(property.PropertyType) )
				result.Add(( property.GetValue(instance) as ConfigEntryBase )?.ConfigFile);
			else if ( typeof(ConfigFile).IsAssignableFrom(property.PropertyType) )
				result.Add(property.GetValue(instance) as ConfigFile);

		result.Remove(null);
		return result;
	}

	static bool HasEntry(string identifier, ConfigEntryBase entry)
	{
		try
		{
			Category category = Settings.OptionCollection[identifier][entry.Definition.Section];

			for ( int i = 0; i < category.OptionCount; ++i )
				if ( entry == category[i].ConfigEntry )
					return true;
		}
		catch ( KeyNotFoundException ) { }

		return false;
	}

	static BaseOption CreateOption(ConfigEntryBase entry)
	{
		AcceptableValueBase valid = entry.Description.AcceptableValues;
		Type type = valid?.GetType(), target = typeof(AcceptableValueList<>);

		if ( type?.IsGenericType is true && type.GetGenericTypeDefinition() == target )
		{
			MethodInfo method = target.MakeGenericType(valid.ValueType).GetProperty(
					nameof(AcceptableValueList<byte>.AcceptableValues)
				).GetMethod;

			Array result = method.Invoke(valid, null) as Array;
			return new ListOption(entry, result.Cast<object>().ToArray());
		}
		else type = valid switch
		{
			AcceptableValueRange<sbyte> => typeof(SByteSliderOption),
			AcceptableValueRange<byte> => typeof(ByteSliderOption),
			AcceptableValueRange<short> => typeof(Int16SliderOption),
			AcceptableValueRange<ushort> => typeof(UInt16SliderOption),
			AcceptableValueRange<int> => typeof(Int32SliderOption),
			AcceptableValueRange<uint> => typeof(UInt32SliderOption),
			AcceptableValueRange<long> => typeof(Int64SliderOption),
			AcceptableValueRange<ulong> => typeof(UInt64SliderOption),
			AcceptableValueRange<float> => typeof(SingleSliderOption),
			AcceptableValueRange<double> => typeof(DoubleSliderOption),
			AcceptableValueRange<decimal> => typeof(DecimalSliderOption),

			_ => entry switch
			{
				ConfigEntry<sbyte> => typeof(SByteOption),
				ConfigEntry<byte> => typeof(ByteOption),
				ConfigEntry<short> => typeof(Int16Option),
				ConfigEntry<ushort> => typeof(UInt16Option),
				ConfigEntry<int> => typeof(Int32Option),
				ConfigEntry<uint> => typeof(UInt32Option),
				ConfigEntry<long> => typeof(Int64Option),
				ConfigEntry<ulong> => typeof(UInt64Option),
				ConfigEntry<float> => typeof(SingleOption),
				ConfigEntry<double> => typeof(DoubleOption),
				ConfigEntry<decimal> => typeof(DecimalOption),
				ConfigEntry<bool> => typeof(BooleanOption),
				ConfigEntry<string> => typeof(StringOption),

				ConfigEntry<Color> => typeof(ColorOption),
				ConfigEntry<KeyboardShortcut> => typeof(KeyBindOption),

				_ => entry.SettingType.IsEnum ? typeof(ChoiceOption) : null
			}
		};

		if ( type is null )
		{
			Console.WriteLine("No option for type " + entry.GetType());
			return null;
		}
		else return Activator.CreateInstance(type, entry) as BaseOption;
	}

	static void LoadIcon(PluginInfo info)
	{
		if ( ! TryGetOption(info.Metadata.GUID, out OptionCollection option) )
			return;

		if ( option.icon != null || option.iconPrefab != null )
			return;

		if ( ! FindPath(info, "icon.png", out string path) )
			return;

		const int size = 256, scale = 2;
		Texture2D texture = new(size, size, TextureFormat.ARGB32, scale + 1, linear: false);

		try
		{
			if ( ! ImageConversion.LoadImage(texture, File.ReadAllBytes(path)) )
				throw new Exception();
		}
		catch
		{
			Console.WriteLine("Unable to load '" + path + "'\n");
			return;
		}

		Sprite icon = Sprite.Create(
				texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
		Settings.SetModIcon(icon, option.ModGuid, option.ModName);
	}

	static void SetDescription(PluginInfo info)
	{
		if ( ! TryGetOption(info.Metadata.GUID, out OptionCollection option) )
			return;

		if ( ! Language.IsTokenInvalid(option.DescriptionToken) )
			return;

		if ( ! FindPath(info, "manifest.json", out string path) )
			return;

		string description = "description";
		try
		{
			description = JSON.Parse(File.ReadAllText(path))[description];
		}
		catch
		{
			Console.WriteLine("Unable to load '" + path + "'\n");
			return;
		}

		Settings.SetModDescription(description, option.ModGuid, option.ModName);
	}

	static bool TryGetOption(string identifier, out OptionCollection option)
	{
		if ( Settings.OptionCollection.ContainsModGuid(identifier) )
		{
			option = Settings.OptionCollection[identifier];
			return true;
		}

		option = null;
		return false;
	}

	static void RemoveOption(string identifier)
	{
		if ( Settings.OptionCollection.ContainsModGuid(identifier) )
		{
			var list = Settings.OptionCollection._identifierModGuidMap.ToArray();
			Settings.OptionCollection._optionCollections.Remove(identifier);

			foreach ( KeyValuePair<string, string> element in list )
				if ( identifier == element.Value )
					Settings.OptionCollection._identifierModGuidMap.Remove(element.Key);
		}
	}

	static bool FindPath(PluginInfo info, string filename, out string path)
	{
		string directory = Path.GetDirectoryName(info.Location);
		path = Path.Combine(directory, filename);

		if ( ! File.Exists(path) )
			path = Path.Combine(Directory.GetParent(directory).FullName, filename);

		return File.Exists(path);
	}

	[HarmonyPatch(typeof(ModOptionPanelController),
			nameof(ModOptionPanelController.LoadOptionListFromCategory))]
	[HarmonyTranspiler]
	static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> IL)
	{
		foreach ( CodeInstruction instruction in IL )
		{
			if ( instruction.opcode == OpCodes.Isinst )
			{
				yield return new CodeInstruction(OpCodes.Dup);
				yield return instruction;
				yield return Transpilers.EmitDelegate(
					( object obj, object result ) =>
					{
						if ( result is not null )
							return result;
						else if ( CheckType(obj, instruction.operand as Type) )
							return obj;
						else return null;
					});
			}
			else yield return instruction;
		}
	}

	static bool CheckType(object obj, Type type) => type switch
	{
		_ when type == typeof(IntFieldOption) => obj switch
		{
			SByteOption => true, ByteOption => true,
			Int16Option => true, UInt16Option => true,
			Int32Option => true, UInt32Option => true,
			Int64Option => true, UInt64Option => true,
			_ => false
		},
		_ when type == typeof(IntSliderOption) => obj switch
		{
			SByteSliderOption => true, ByteSliderOption => true,
			Int16SliderOption => true, UInt16SliderOption => true,
			Int32SliderOption => true, UInt32SliderOption => true,
			Int64SliderOption => true, UInt64SliderOption => true,
			_ => false
		},
		_ when type == typeof(FloatFieldOption) => obj switch
		{
			SingleOption => true,
			DoubleOption => true,
			DecimalOption => true,
			_ => false
		},
		_ when type == typeof(SliderOption) => obj switch
		{
			SingleSliderOption => true,
			DoubleSliderOption => true,
			DecimalSliderOption => true,
			_ => false
		},
		_ when type == typeof(ChoiceOption) => obj switch
		{
			ListOption => true,
			_ => false
		},
		_ => false
	};
}

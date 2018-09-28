using System;
using System.IO;
using System.Reflection;

namespace LuaBinder
{
	public static class XLuaGenerate
	{
		public static void Main( string[] args )
		{
			if ( args.Length < 2 )
			{
				Console.WriteLine( @"XLuaGenerate assmbly_path output_path" );
				return;
			}

			GeneratorConfig.common_path = args[1];
			Assembly assembly;
			try
			{
				assembly = Assembly.LoadFrom( Path.GetFullPath( args[0] ) );
			}
			catch ( Exception e )
			{
				Console.WriteLine( e );
				return;
			}
			Generator.GenAll( new XLuaTemplates
			{
				LuaClassWrap = new XLuaTemplate
				{
					name = "LuaClassWrap",
					text = XLuaRes.LuaClassWrap_tpl,
				},
				LuaDelegateBridge = new XLuaTemplate
				{
					name = "LuaDelegateBridge",
					text = XLuaRes.LuaDelegateBridge_tpl,
				},
				LuaDelegateWrap = new XLuaTemplate
				{
					name = "LuaDelegateWrap",
					text = XLuaRes.LuaDelegateWrap_tpl,
				},
				LuaEnumWrap = new XLuaTemplate
				{
					name = "LuaEnumWrap",
					text = XLuaRes.LuaEnumWrap_tpl,
				},
				LuaInterfaceBridge = new XLuaTemplate
				{
					name = "LuaInterfaceBridge",
					text = XLuaRes.LuaInterfaceBridge_tpl,
				},
				LuaRegister = new XLuaTemplate
				{
					name = "LuaRegister",
					text = XLuaRes.LuaRegister_tpl,
				},
				LuaWrapPusher = new XLuaTemplate
				{
					name = "LuaWrapPusher",
					text = XLuaRes.LuaWrapPusher_tpl,
				},
				PackUnpack = new XLuaTemplate
				{
					name = "PackUnpack",
					text = XLuaRes.PackUnpack_tpl,
				},
				TemplateCommon = new XLuaTemplate
				{
					name = "TemplateCommon",
					text = XLuaRes.TemplateCommon_lua,
				},
			}, assembly.GetTypes() );
		}
	}
}

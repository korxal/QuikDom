using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using static LuaNET.Lua54.Lua;
using System.Threading.Tasks;
using System.Reflection;
using NetQuikConnector;
using LuaNET.Lua54;
using System.Text;
using System.IO;
using System;


namespace QuikLua;

public unsafe static class NetQuikConnector
{
    //This will appear as Lua module name in Quik
    private const string ConnectorName = "NetQuikConnector";

    //To store quotes
    private static readonly ConcurrentBag<string> QuoteCache = new ConcurrentBag<string>();

    //Quik script status
    private volatile static int Run = 1;

    //This loop is used 
    private static void DumpCacheLoop()
    {
        while(Run==1)
        {
            System.Threading.Thread.Sleep(10000);
            DumpCache();
        }
    }


    /// <summary>
    /// This method dumps quotes with Depth of Market to file.
    /// </summary>
    private static void DumpCache()
    {

        StringBuilder sb = new StringBuilder();
        int QuoteCount = QuoteCache.Count;
        while (QuoteCount > 0 && QuoteCache.TryTake(out string Quote))
        {
            QuoteCount--;
            sb.AppendLine(Quote);
        }
        File.AppendAllText($"C:\\Temp\\Quotes-{DateTime.Now:yyyy-MM-dd}.log", sb.ToString());
    }


    /// <summary>
    /// This is Like DLL Entry point for Quik.
    /// Called from Quik LUA engine on 'require ("NetQuikConnector")'
    /// luaopen_ - if fixed prefix for LUA library
    /// </summary>
    /// <param name="L"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly(EntryPoint = "luaopen_" + ConnectorName)]
    public static int luaopen_NetQuikConnector(lua_State L)
    {
        try
        {
            //Declare new LUA library with methods
            luaL_newlib(L, GetMethods);
            //Set LUA namespace
            lua_setglobal(L, ConnectorName);
        }
        catch (Exception e)
        {
            //This will crash QUIK and write exception to Windows Event log
            Environment.FailFast(e.Message);
        }

        Task.Factory.StartNew(DumpCacheLoop);
        return 0;
    }

    /// <summary>
    /// Static class constructor
    /// </summary>
    static NetQuikConnector()
    {
        NativeLibrary.SetDllImportResolver(typeof(NetQuikConnector).Assembly, ImportResolver);
        NativeLibrary.SetDllImportResolver(typeof(LuaNET.Lua54.Lua).Assembly, ImportResolver);
    }

    //Lua dll version is hard coded in LuaNET, we`ll have to reroute that...
    //https://learn.microsoft.com/en-us/dotnet/standard/native-interop/native-library-loading
    private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {  
        IntPtr libHandle = IntPtr.Zero;
        //LuaNet looks for lua546, but Quik terminal have "lua54"
        if (libraryName.StartsWith("lua54"))
            NativeLibrary.TryLoad("lua54.dll", assembly, DllImportSearchPath.ApplicationDirectory, out libHandle);

        return libHandle;
    }

    /// <summary>
    /// Simple ping method to check if .Net Part is working
    /// </summary>
    /// <param name="L"></param>
    /// <returns></returns>
    [QuikLuaMethod]
    [UnmanagedCallersOnly]
    public static int Test(lua_State L)
    {
        ulong l = 0;
        //Get string from stack
        var text = luaL_checklstring(L, 1, ref l);

        //File.WriteAllText("C:\\temp\\flags\\test.txt", text);
        //text = text.ToUpper();
        //Return string to Stack
        lua_pushstring(L, text==null?"":text);
        return 1;
    }



    /// <summary>
    /// This method is called from Quik LUA like this
    /// NetQuikConnector.NewQuote(QuoteStr);
    /// </summary>
    /// <param name="L"></param>
    /// <returns></returns>
    [QuikLuaMethod]
    [UnmanagedCallersOnly]
    public static int NewQuote(lua_State L)
    {
        ulong l = 0;
        var text = luaL_checklstring(L, 1, ref l);

        if(Run==1)
            QuoteCache.Add(text);

        return 0;
    }

    [QuikLuaMethod]
    [UnmanagedCallersOnly]
    public static int Dispose(lua_State L)
    {
        Run = 0;
        DumpCache();
        return 0;
    }


    //This method is defined in Lua docs. It methods of  our 'library' in lua with methods declared here
    public static luaL_Reg[] GetMethods = new luaL_Reg[]
    {
        AsLuaLReg("Test", &Test),
        AsLuaLReg("NewQuote", &NewQuote),
        AsLuaLReg("Dispose", &Dispose),

        AsLuaLReg(null, null)
    };


}
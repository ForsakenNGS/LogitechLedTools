using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Jint.Native;
using LedCSharp;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Web;

namespace LogitechLedTools
{
    class WebinterfaceHttp
    {
        private readonly HttpListener   _listener = new HttpListener();
        private readonly Assembly       _assembly = Assembly.GetExecutingAssembly();
        private Jint.Engine             engine;
        private Timer                   engineUpdateTimer;
        private Stopwatch               engineClock;
        private LogitechKeyboard        keyboard;
        private Timer                   keyboardUpdateTimer;
        private LogWriter               log;
        private bool                    active;
        private ProfileUpdater          updater;

        public WebinterfaceHttp() 
        {
            _listener.Prefixes.Add("http://localhost:8042/");
            log = new LogWriter("LogitechLedTools.log");
            active = false;
            updater = null;
            log.Log("Init logitech LED library...", LogWriter.Level.DEBUG);
            if (!LogitechGSDK.LogiLedInit())
            {
                throw new Exception("Failed to initialize Logitech LED library");
            }
            keyboard = new LogitechKeyboard();
            log.Log("Init javascript library library...", LogWriter.Level.DEBUG);
            engine = new Jint.Engine();
            engine.SetValue("Log", log);
            engine.SetValue("LogLevel_DEBUG", LogWriter.Level.DEBUG);
            engine.SetValue("LogLevel_INFO", LogWriter.Level.INFO);
            engine.SetValue("LogLevel_WARNING", LogWriter.Level.WARNING);
            engine.SetValue("LogLevel_ERROR", LogWriter.Level.ERROR);
            engine.SetValue("LogLevel_FATAL", LogWriter.Level.FATAL);
            engine.SetValue("LogitechKeyboard", keyboard);
            engine.SetValue("LogitechKeyboardType_MonochromeFull", LogitechKeyboardType.MonochromeFull);
            engine.SetValue("LogitechKeyboardType_ColoredFull", LogitechKeyboardType.ColoredFull);
            engine.SetValue("LogitechKeyboardType_PerKey", LogitechKeyboardType.PerKey);
            engine.SetValue("RegisterProfile", new Action(RegisterProfile));
            engine.SetValue("KeyBar_F1_F12", KeyBar.F1_F12);
            engine.SetValue("KeyBar_F1_F8", KeyBar.F1_F8);
            engine.SetValue("KeyBar_F9_F12", KeyBar.F9_F12);
            engine.SetValue("KeyBar_NUMPAD_CIRCLE", KeyBar.NUMPAD_CIRCLE);
            engine.SetValue("KeyBar_NUMPAD_BLOCK_A", KeyBar.NUMPAD_BLOCK_A);
            engine.SetValue("KeyBar_NUMPAD_BLOCK_B", KeyBar.NUMPAD_BLOCK_B);
            engine.SetValue("GetEngineClock", new Func<long>(GetEngineClock));
            engine.SetValue("GetFileUpdateDate", new Func<string,long>(WebinterfaceNative.GetFileUpdateDate));
            engine.SetValue("ImageFormat_Jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
            engine.SetValue("ImageFormat_Png", System.Drawing.Imaging.ImageFormat.Png);
            engine.SetValue("GetForegroundWindow", new Func<int>(WebinterfaceNative.GetForegroundWindowInt32));
            engine.SetValue("GetOpenWindows", new Func<IDictionary<UInt32, string>>(WebinterfaceNative.GetOpenWindows));
            engine.SetValue("GetWindowRect", new Func<int, RECT>(WebinterfaceNative.GetWindowRect));
            engine.SetValue("CaptureScreen", new Func<int, int, int, int, int, Bitmap>(WebinterfaceNative.CaptureScreen));
            engine.SetValue("LoadProfileClass", new Func<string, Jint.Native.JsValue>(LoadProfileClass));
            engine.SetValue("LoadProfileDisplay", new Func<string, Jint.Native.JsValue>(LoadProfileDisplay));
            engine.SetValue("ConfigJsonRead", new Func<string>(ConfigJsonRead));
            engine.SetValue("ConfigJsonWrite", new Action<string>(ConfigJsonWrite));
            log.Log("Starting update routine...", LogWriter.Level.DEBUG);
            engineClock = new Stopwatch();
            engineClock.Start();
            engineUpdateTimer = new Timer(new TimerCallback((o) =>
            {
                UpdateProfile();
            }));
            keyboardUpdateTimer = new Timer(new TimerCallback((o) =>
            {
                keyboard.UpdateLightning();
            }));
            ExecuteResourceFile("LogitechLedTools.base.js");
            log.Log("Startup routine successful!", LogWriter.Level.DEBUG);
        }

        public string ConfigJsonRead()
        {
            if (File.Exists("config.json"))
            {
                return File.ReadAllText("config.json");
            }
            return "{}";
        }

        public void ConfigJsonWrite(string jsonConfig)
        {
            File.WriteAllText("config.json", jsonConfig);
        }

        private JsValue LoadProfileClass(string name)
        {
            if (File.Exists("webinterface/profiles/" + name + "/profile.js"))
            {
                return ExecuteLocalFile("webinterface/profiles/" + name + "/profile.js");
            }
            else
            {
                return JsValue.Null;
            }
        }

        private JsValue LoadProfileDisplay(string name)
        {
            if (File.Exists("webinterface/profiles/" + name + "/display.js"))
            {
                return ExecuteLocalFile("webinterface/profiles/" + name + "/display.js");
            }
            else
            {
                return JsValue.Null;
            }
        }

        public void RegisterProfile()
        {
            var activeProfile = HandleScriptFunction("GetActiveProfile");
            if (!activeProfile.IsNull() && activeProfile.IsObject())
            {
                // Active profile present!
                if (activeProfile.AsObject().HasProperty("register") && activeProfile.AsObject().HasProperty("hwnd"))
                {
                    // Register function present
                    var windowHwnd = Convert.ToInt32(activeProfile.AsObject().GetProperty("hwnd").Value.AsNumber());
                    var registerFunction = activeProfile.AsObject().GetProperty("register").Value;
                    updater = new ProfileUpdater(windowHwnd);
                    HandleScriptFunction(registerFunction, JsValue.FromObject(engine, updater));
                }
            }
        }

        public void UpdateProfile()
        {
            var activeProfile = HandleScriptFunction("GetActiveProfile");
            if ((updater != null) && !activeProfile.IsNull() && activeProfile.IsObject())
            {
                // Active profile present!
                var activeProfileObj = activeProfile.AsObject();
                if (activeProfileObj.HasProperty("hwnd"))
                {
                    var jsUpdater = JsValue.FromObject(engine, updater);
                    // Update function present
                    int windowHwnd = Convert.ToInt32(activeProfileObj.GetProperty("hwnd").Value.AsNumber());
                    string scene = null;
                    // Update generic checks
                    updater.Execute();
                    if (activeProfileObj.HasProperty("updateScene"))
                    {
                        var updateSceneFunction = activeProfileObj.GetProperty("updateScene").Value;
                        scene = HandleScriptFunction(updateSceneFunction, jsUpdater).AsString();
                    }
                    // Scene specific checks
                    if (scene != null)
                    {
                        updater.Execute(scene);
                    }
                    // Process result
                    if (activeProfileObj.HasProperty("updateResult"))
                    {
                        var updateResultFunction = activeProfileObj.GetProperty("updateResult").Value;
                        HandleScriptFunction(updateResultFunction, jsUpdater, JsValue.FromObject(engine, scene));
                    }
                    // Update display
                    if (activeProfileObj.HasProperty("display") && activeProfileObj.HasProperty("data"))
                    {
                        var displayFunction = activeProfileObj.GetProperty("display").Value;
                        var profileData = activeProfileObj.GetProperty("data").Value;
                        HandleScriptFunction(displayFunction, profileData);
                    }
                }
            }
            // Do generic updates
            HandleScriptFunction("GetWindows");
        }

        public long GetEngineClock()
        {
            return engineClock.ElapsedMilliseconds;
        }

        private void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            //log.WriteEntry("Request: '" + request.Url.AbsolutePath + "'");
            try
            {
                switch (request.Url.AbsolutePath)
                {
                    default:
                        var filename = "webinterface" + request.Url.LocalPath;
                        if (File.Exists(filename))
                        {
                            if (filename.EndsWith(".html"))
                            {
                                response.ContentType = "text/html; charset=utf-8";
                            }
                            else if (filename.EndsWith(".js"))
                            {
                                response.ContentType = "application/javascript; charset=utf-8";
                            }
                            else if (filename.EndsWith(".css"))
                            {
                                response.ContentType = "text/css; charset=utf-8";
                            }
                            else if (filename.EndsWith(".jpg") || filename.EndsWith(".jpeg"))
                            {
                                response.ContentType = "image/jpeg";
                            }
                            else if (filename.EndsWith(".png"))
                            {
                                response.ContentType = "image/png";
                            }
                            OutputLocalFile(filename, response);
                        }
                        else
                        {
                            response.StatusCode = 404;
                        }
                        break;
                    case "/":
                        //log.WriteEntry("Result: index.html");
                        response.ContentType = "text/html; charset=utf-8";
                        OutputLocalFile("webinterface/index.html", response);
                        break;
                    case "/windows.json":
                        {
                            response.ContentType = "application/json; charset=utf-8";
                            OutputString(HandleScriptFunction("GetWindows").AsString(), response);
                        }
                        break;
                    case "/screenshot.json":
                        {
                            response.ContentType = "application/json; charset=utf-8";
                            var hwnd = int.Parse(request.QueryString["hwnd"]);
                            OutputString(HandleScriptFunction("GetWindowScreenshot", hwnd).AsString(), response);
                        }
                        break;
                    case "/profile.json":
                        {
                            response.ContentType = "application/json; charset=utf-8";
                            var name = request.QueryString["name"];
                            OutputString(HandleScriptFunction("LoadProfile", name).AsString(), response);
                        }
                        break;
                    case "/setting.json":
                        {
                            if (request.HttpMethod.ToLower().Equals("post"))
                            {
                                // Save
                                StreamReader postReader = new StreamReader(request.InputStream);
                                string postBody = postReader.ReadToEnd();
                                NameValueCollection postVars = HttpUtility.ParseQueryString(postBody);
                                foreach (string name in postVars.Keys)
                                {
                                    HandleScriptFunction("SetConfigValue", name, postVars[name]);
                                }
                                HandleScriptFunction("SaveConfig");
                                response.ContentType = "application/json; charset=utf-8";
                                OutputString("{\"success\":true}", response);
                            } else
                            {
                                // Read
                                response.ContentType = "application/json; charset=utf-8";
                                OutputString(HandleScriptFunction("GetConfigAsJson").AsString(), response);
                            }
                        }
                        break;
                }
            }
            catch (Jint.Runtime.JavaScriptException exception)
            {
                log.Log("JS Exception: " + exception.ToString(), LogWriter.Level.WARNING);
                throw exception;
            }
        }

        private JsValue HandleScriptFunction(string functionName, params JsValue[] parameters)
        {
            var jsFunction = engine.GetValue(functionName);
            return HandleScriptFunction(jsFunction, parameters);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private JsValue HandleScriptFunction(JsValue jsFunction, params JsValue[] parameters)
        {
            return jsFunction.Invoke(parameters);
        }

        private void OutputLocalFile(string filename, HttpListenerResponse response)
        {
            byte[] buf = File.ReadAllBytes(filename);
            response.ContentLength64 = buf.Length;
            response.OutputStream.Write(buf, 0, buf.Length);
        }

        private void ExecuteResourceFile(string filename)
        {
            StreamReader reader = new StreamReader(_assembly.GetManifestResourceStream(filename));
            engine.Execute(reader.ReadToEnd());
        }

        private JsValue ExecuteLocalFile(string filename)
        {
            return engine.Execute(File.ReadAllText(filename)).GetCompletionValue();
        }

        private void OutputResourceFile(string filename, HttpListenerResponse response)
        {
            StreamReader reader = new StreamReader(_assembly.GetManifestResourceStream(filename));
            OutputString(reader.ReadToEnd(), response);
        }

        private void OutputString(string output, HttpListenerResponse response)
        {
            byte[] buf = Encoding.UTF8.GetBytes(output);
            response.ContentLength64 = buf.Length;
            response.OutputStream.Write(buf, 0, buf.Length);
        }

        public bool IsActive()
        {
            return active;
        }

        public void Start()
        {
            _listener.Start();
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                HandleRequest(ctx.Request, ctx.Response);
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
            active = true;
            engineUpdateTimer.Change(250, 250);
            keyboardUpdateTimer.Change(50, 50);
        }

        public void Stop()
        {
            HandleScriptFunction("SaveConfig");
            active = false;
            engineUpdateTimer.Change(-1, -1);
            keyboardUpdateTimer.Change(-1, -1);
            _listener.Stop();
        }
    }
}

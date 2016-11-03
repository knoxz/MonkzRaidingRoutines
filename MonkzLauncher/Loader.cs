// Credits to the Honorbuddy community

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Media;
using Styx;
using Styx.Common;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using System.Net;

namespace Loader
{
    public class Loader : CombatRoutine
    {
        private CombatRoutine CC;
        private Assembly assembly;

        public Loader()
        {
            string routineDirectory = Path.Combine(Utilities.AssemblyDirectory, "Routines\\MonkzLauncher");
            string path = routineDirectory + @"\MonkzRoutines.dll";

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(@"https://github.com/knoxz/MonkzRaidingRoutines/raw/master/MonkzLauncher/MonkzRoutines.dll");
                request.Method = "HEAD";
                var response = (HttpWebResponse)request.GetResponse();
                Logging.Write("Checking for Updates!");

                if (response.LastModified > File.GetLastWriteTime(path))
                {
                    Logging.Write("Updates found. Downloading!");
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://github.com/knoxz/MonkzRaidingRoutines/raw/master/MonkzLauncher/MonkzRoutines.dll", path);
                        Logging.Write("Download complete!");
                    }
                }
                else
                {
                    Logging.Write("No Update necessary. Have Fun with the routine!");
                }

            } catch(Exception e)
            {
                Logging.Write("There was an error with updates!");
                Logging.Write(""+e);
            }

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs e)
                {
                    try
                    {
                        AssemblyName requestedName = new AssemblyName(e.Name);
                        if (requestedName.Name == "Honorbuddy")
                        {
                            return Assembly.LoadFile(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                        }
                        if (requestedName.Name == "GreyMagic")
                        {
                            return Assembly.LoadFile(Utilities.AssemblyDirectory + @"\GreyMagic.dll");
                        }
                        if (requestedName.Name == "MonkzRaidingRoutinesLoader")
                            return assembly;
                        return null;
                    }
                    catch (System.Exception)
                    {
                        return null;
                    }
                };

                byte[] Bytes = File.ReadAllBytes(path);
                assembly = Assembly.Load(Bytes);

                foreach (Type t in assembly.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(CombatRoutine)) && t.IsClass)
                    {
                        object obj = Activator.CreateInstance(t);
                        CC = (CombatRoutine)obj;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    if (exSub is FileNotFoundException)
                    {
                        var exFileNotFound = exSub as FileNotFoundException;
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                //Display or log the error based on your application.
                Logging.Write(errorMessage);
            }
            catch (Exception e)
            {
                Logging.Write(Colors.DarkRed, "Error occurred initialising routine");
                Logging.Write(e.ToString());
            }
        }

        #region Overrides of CombatRoutine

        public override string Name { get { return CC.Name; } }
        public override WoWClass Class { get { return CC.Class; } }
        public override bool WantButton { get { return CC.WantButton; } }
        public override Composite CombatBehavior { get { return CC.CombatBehavior; } }
        public override Composite PreCombatBuffBehavior { get { return CC.PreCombatBuffBehavior; } }
        public override CapabilityFlags SupportedCapabilities { get { return CapabilityFlags.None; } }

        public override void Initialize()
        {
            CC.Initialize();
        }

        public override void OnButtonPress()
        {
            CC.OnButtonPress();
        }

        public override void Pulse()
        {
            CC.Pulse();
        }

        #endregion
    }
}